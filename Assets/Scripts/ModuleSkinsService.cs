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
    #region Singleton Management
    private static ModuleSkinsService Instance { get; set; }
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
            _started = true;

            ProcessSkins();

            var gameInfo = GetComponent<KMGameInfo>();
            gameInfo.OnStateChange += StateChange;
        }
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

        #region Read settings
        if (settings.Settings == "")
        {
            Debug.Log("[Module Skins] Empty or nonexistent settings file. Writing default values...");
            settings.Settings = "{\"version\":1}";
            dirty = true;
        }

        try
        {
            _settings = JsonConvert.DeserializeObject<Settings>(settings.Settings);
        }
        catch (JsonException ex)
        {
            Debug.Log("[Module Skins] Exception when reading settings file. Overwriting with default values...");
            Debug.LogException(ex);
            _settings = new Settings
            {
                Version = 1,
                Modules = new Dictionary<string, Dictionary<string, int>>()
            };
            dirty = true;
        }
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
        foreach (var module in _settings.Modules)
        {
            List<string> excessKeys = new List<string>();
            bool anyExist = false;
            foreach (var name in module.Value)
            {
                if (name.Key == Settings.DefaultSkinName)
                    continue;
                if (!skinNamesSet.Contains(new SkinName(module.Key, name.Key)))
                    excessKeys.Add(name.Key);
                else
                    anyExist = true;
            }
            if (!anyExist)
            {
                excessModules.Add(module.Key);
                continue;
            }
            foreach (var key in excessKeys)
            {
                Debug.LogFormat("[Module Skins] Found unknown skin '{0}.{1}'. Removing it.", module.Key, key);
                module.Value.Remove(key);
            }
            if (excessKeys.Count > 0)
                dirty = true;
        }
        foreach (var module in excessModules)
        {
            Debug.LogFormat("[Module Skins] Found unknown skin set '{0}'. Removing it.", module);
            _settings.Modules.Remove(module);
        }
        if (excessModules.Count > 0)
            dirty = true;
        #endregion

        #region Add new skins
        foreach (var name in skinNames)
        {
            Dictionary<string, int> dict;
            if (!_settings.Modules.TryGetValue(name.Module, out dict))
            {
                _settings.Modules[name.Module] = dict = new Dictionary<string, int>
                {
                    { Settings.DefaultSkinName, 0 }
                };
                dirty = true;
            }
            if (!dict.ContainsKey(Settings.DefaultSkinName))
            {
                dict[Settings.DefaultSkinName] = 0;
                dirty = true;
            }
            if (!dict.ContainsKey(name.Name))
            {
                dict[name.Name] = 1;
                dirty = true;
            }
        }
        #endregion

        if (dirty)
            File.WriteAllText(settings.SettingsPath, JsonConvert.SerializeObject(_settings));
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
        if (!_settings.Modules.TryGetValue(moduleId, out dict))
            return null;
        var weight = dict.Values.Sum();
        if (weight == 0)
        {
            Debug.LogWarningFormat("[Module Skins] No skins enabled for module '{0}'. Using the default.", moduleId);
            return null;
        }
        var choice = UnityEngine.Random.Range(0, weight);
        string chosen = null;
        foreach (var kvp in dict)
        {
            choice -= kvp.Value;
            if (choice < 0)
                chosen = kvp.Key;
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
        IList bombs = null;
        int bombCount = -1;
        yield return new WaitUntil(() => (bombCount = (bombs = ReflectionHelper.GameplayStateBombs).Count) != 0);

        Debug.LogFormat("[Module Skins] {0} bombs{1} found.", bombs.Count, bombs.Count == 1 ? "" : "s");
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
