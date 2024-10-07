using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(KMService), typeof(KMGameInfo), typeof(KMModSettings))]
public class ModuleSkinsService : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _prefabs;
    private Dictionary<string, GameObject> _prefabsDict;
    public Dictionary<string, GameObject> PrefabsLookup { get { return _prefabsDict = _prefabsDict ?? _prefabs.ToDictionary(k => k.name, k => k); } }

    #region Singleton Management
    public static ModuleSkinsService Instance { get; set; }
    private static bool _started;

    public void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Module Skins] Duplicate service! Destroying one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (!_started)
        {
            StartCoroutine(Init());
        }
    }

    private IEnumerator Init()
    {
        var gameInfo = GetComponent<KMGameInfo>();
        gameInfo.OnStateChange += StateChange;

        yield return null;

        ProcessSkins();

        _started = true;
    }

    public void OnDisable()
    {
        if (Instance == this)
            Instance = null;

        var gameInfo = GetComponent<KMGameInfo>();
        gameInfo.OnStateChange -= StateChange;
    }

    private static void StateChange(KMGameInfo.State state)
    {
        if (state == KMGameInfo.State.Gameplay && IsEnabled)
            Instance.StartCoroutine(OnEnterGameplay());
    }
    #endregion

    private static bool IsEnabled { get { return Instance != null; } }
    private static List<ModuleSkin> s_skins;
    private static Dictionary<SkinName, ModuleSkin> s_skinLookup;
    private static Settings _settings;

    private void UpdateSettings()
    {
        var settings = GetComponent<KMModSettings>();
        settings.RefreshSettings();
        bool dirty = false;

        Debug.Log("[Module Skins] Reading settings...");

        var jsettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
        };

        #region Read settings
        if (settings.Settings == "")
        {
            Debug.Log("[Module Skins] Empty or nonexistent settings file. Writing default values...");
            settings.Settings = "{\"Version\":1,\"ModuleIds\":{}}";
            dirty = true;
        }

        try
        {
            _settings = JsonConvert.DeserializeObject<Settings>(settings.Settings, jsettings);
        }
        catch (JsonException ex)
        {
            Debug.Log("[Module Skins] Exception when reading settings file. Overwriting with default values...");
            Debug.LogException(ex);
            _settings = new Settings() { Version = 1, Modules = new Dictionary<string, Dictionary<string, int>>() };
            dirty = true;
        }

        _settings.Modules = _settings.Modules ?? new Dictionary<string, Dictionary<string, int>>();
        #endregion

        #region Determine all valid skins
        List<SkinName> skinNames = new List<SkinName>(s_skins.Count);
        HashSet<SkinName> skinNamesSet = new HashSet<SkinName>();

        foreach (var skin in s_skins)
        {
            skinNames.Add(skin.SkinName);
            skinNamesSet.Add(skin.SkinName);
        }
        #endregion

        #region Remove excess skins
        List<string> excessModules = new List<string>();
        foreach (var module in _settings.ModuleIds)
        {
            List<string> excessKeys = new List<string>();
            bool anyExist = false;
            foreach (var name in _settings.Skins(module))
            {
                if (name.Key == Settings.DefaultSkinName)
                    continue;
                if (!skinNamesSet.Contains(new SkinName(module, name.Key)))
                    excessKeys.Add(name.Key);
                else
                    anyExist = true;
            }
            if (!anyExist)
            {
                excessModules.Add(module);
                continue;
            }
            foreach (var key in excessKeys)
            {
                Debug.LogFormat("[Module Skins] Found unknown skin '{0}.{1}'. Removing it.", module, key);
                _settings.Remove(module, key);
            }
            if (excessKeys.Count > 0)
                dirty = true;
        }
        foreach (var module in excessModules)
        {
            Debug.LogFormat("[Module Skins] Found unknown skin set '{0}'. Removing it.", module);
            _settings.Remove(module);
        }
        if (excessModules.Count > 0)
            dirty = true;
        #endregion

        #region Add new skins
        foreach (var name in skinNames)
        {
            Dictionary<string, int> dict;
            if (!_settings.TryGetModule(name.Module, out dict))
            {
                _settings.AddModule(name.Module, dict = new Dictionary<string, int>
                {
                    { Settings.DefaultSkinName, 0 }
                });
                Debug.LogFormat("[Module Skins] Inserting skin settings for skin set '{0}'.", name.Module);
                dirty = true;
            }
            if (!dict.ContainsKey(Settings.DefaultSkinName))
            {
                _settings.AddSkin(name.Module, Settings.DefaultSkinName, 0);
                Debug.LogFormat("[Module Skins] Inserting default skin settings for '{0}.{1}'.", name.Module, Settings.DefaultSkinName);
                dirty = true;
            }
            if (!dict.ContainsKey(name.Name))
            {
                _settings.AddSkin(name.Module, name.Name, 1);
                Debug.LogFormat("[Module Skins] Inserting default skin settings for '{0}.{1}'.", name.Module, name.Name);
                dirty = true;
            }
        }
        #endregion

        if (dirty)
            File.WriteAllText(settings.SettingsPath, JsonConvert.SerializeObject(_settings, jsettings));
    }

    private void ProcessSkins()
    {
        var mst = typeof(ModuleSkin);
        s_skins = mst
            .Assembly
            .GetTypes()
            .Where(t => mst.IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => (ModuleSkin)gameObject.AddComponent(t))
            .ToList();

        s_skinLookup = s_skins.ToDictionary(s => s.SkinName, s => s);

        UpdateSettings();
    }

    private static Type GetSkinFor(string moduleId)
    {
        Dictionary<string, int> dict;
        if (!_settings.TryGetModule(moduleId, out dict))
            return null;
        var weight = dict.Values.Sum();
        if (weight == 0)
        {
            Debug.LogWarningFormat("[Module Skins] No skins enabled for module '{0}'. Using the default.", moduleId);
            return null;
        }

        var choice = UnityEngine.Random.Range(0, weight);
        Debug.LogFormat("[Module Skins] Choosing from: {0} ({2}/{1})", dict.Select(kvp => kvp.Key + " = " + kvp.Value).Aggregate((a, b) => a + "; " + b), weight, choice);

        string chosen = null;
        foreach (var kvp in dict)
        {
            choice -= kvp.Value;
            if (choice < 0)
            {
                chosen = kvp.Key;
                break;
            }
        }

        if (chosen == Settings.DefaultSkinName)
        {
            Debug.LogFormat("[Module Skins] Chose default skin '{1}' for module '{0}'.", moduleId, chosen);
            return null;
        }

        Debug.LogFormat("[Module Skins] Chose skin '{1}' for module '{0}'.", moduleId, chosen);
        return s_skinLookup[new SkinName(moduleId, chosen)].GetType();
    }

    private static IEnumerator OnEnterGameplay()
    {
        if (!IsEnabled)
            yield break;

        IList bombs = null;
        int bombCount = -1;
        yield return new WaitUntil(() => (bombCount = (bombs = ReflectionHelper.GameplayStateBombs).Count) != 0);

        Debug.LogFormat("[Module Skins] {0} bombs{1} found.", bombCount, bombCount == 1 ? "" : "s");
        Instance.UpdateSettings();
        foreach (var bomb in bombs)
        {
            var bombObject = ((MonoBehaviour)bomb).gameObject;

            foreach (var module in bombObject.GetComponentsInChildren<KMBombModule>())
            {
                var skinType = GetSkinFor(module.ModuleType);
                if (skinType != null)
                    ((ModuleSkin)module.gameObject.AddComponent(skinType)).Begin(module, null);
            }
            foreach (var module in bombObject.GetComponentsInChildren<KMNeedyModule>())
            {
                var skinType = GetSkinFor(module.ModuleType);
                if (skinType != null)
                    ((ModuleSkin)module.gameObject.AddComponent(skinType)).Begin(null, module);
            }
        }
    }
}
