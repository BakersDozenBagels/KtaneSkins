using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ModuleSkin : MonoBehaviour
{
    /// <summary>
    /// The module this skin applies to.
    /// </summary>
    public abstract string ModuleId { get; }
    /// <summary>
    /// The name of this skin to appears in settings.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Called once ever. Use this to set up cached reflection.
    /// </summary>
    protected virtual void Initialize() { }
    /// <summary>
    /// Called after the skinned module receives Awake(), but before it receives Start().
    /// Always called after <see cref="Initialize"/>.
    /// </summary>
    protected virtual void OnStart() { }
    /// <summary>
    /// Called when the skinned module solves, or when the skinned needy module deactivates.
    /// </summary>
    protected virtual void OnSolve() { }
    /// <summary>
    /// Called when the skinned module strikes.
    /// </summary>
    protected virtual void OnStrike() { }
    /// <summary>
    /// Called when the lights turn on.
    /// </summary>
    protected virtual void OnActivate() { }
    /// <summary>
    /// Called when the skinned needy module activates.
    /// </summary>
    protected virtual void OnNeedyActivation() { }
    /// <summary>
    /// Called when the skinned needy module forcefully deactivates.
    /// </summary>
    protected virtual void OnNeedyDeactivation() { }

    /// <summary>
    /// The mod's <see cref="KMAudio"/>.
    /// </summary>
    protected KMAudio Audio { get { return ModuleSkinsService.Instance.Audio; } }
    [Obsolete("Use Audio instead.")]
#pragma warning disable IDE1006 // Naming Styles
    protected new KMAudio audio { get { return Audio; } }
#pragma warning restore IDE1006 // Naming Styles
    /// <summary>
    /// Get the skin's default prefab.
    /// </summary>
    /// <returns>The original prefab.</returns>
    /// <remarks>Be sure to <see cref="UnityEngine.Object.Instantiate(UnityEngine.Object)"/> the returned prefab.</remarks>
    protected GameObject GetPrefab()
    {
        return GetPrefab(SkinName.ToString());
    }
    /// <summary>
    /// Get a prefab from the mod service.
    /// </summary>
    /// <param name="name">The name of the prefab to get.</param>
    /// <returns>The original prefab.</returns>
    /// <remarks>Be sure to <see cref="UnityEngine.Object.Instantiate(UnityEngine.Object)"/> the returned prefab.</remarks>
    protected GameObject GetPrefab(string name)
    {
        GameObject go;
        if (!ModuleSkinsService.Instance.PrefabsLookup.TryGetValue(name, out go))
            throw new ArgumentException(
                string.Format("That prefab ({0}) does not exist on the service instance.", name),
                "name"
            );
        return go;
    }
    /// <summary>
    /// Adds the skin's default prefab to the module.
    /// </summary>
    /// <returns>The instantiated prefab's transform.</returns>
    protected Transform AddPrefab() { return AddPrefab(SkinName.ToString()); }
    /// <summary>
    /// Adds the specified prefab to the module.
    /// </summary>
    /// <returns>The instantiated prefab's transform.</returns>
    protected Transform AddPrefab(string name)
    {
        var skin = Instantiate(GetPrefab(name), transform).transform;
        skin.localPosition = Vector3.zero;
        return skin;
    }
    /// <summary>
    /// Copies callbacks from the mod's children selectables to the corresponding new ones and sets the new selectables as the children.
    /// </summary>
    /// <param name="arr">The new child selectables.</param>
    protected void SetSelectableChildren(params KMSelectable[] arr) { SetSelectableChildren(null, arr); }
    /// <summary>
    /// Copies callbacks from the mod's children selectables to the corresponding new ones and sets the new selectables as the children.
    /// </summary>
    /// <param name="rowLength">The new <see cref="KMSelectable.ChildRowLength"/> to use.</param>
    /// <param name="arr">The new child selectables.</param>
    protected void SetSelectableChildren(int? rowLength, params KMSelectable[] arr)
    {
        var parent = GetComponent<KMSelectable>();
        if (parent.Children.Length != arr.Length)
            throw new ArgumentException("Wrong number of KMSelectable children: " + arr.Length + "/" + parent.Children.Length);

        for (int i = 0; i < arr.Length; i++)
        {
            var orig = parent.Children[i];
            parent.Children[i] = arr[i];

            if (arr[i] == null)
                continue;

            arr[i].Parent = parent;
            arr[i].OnCancel += () => { return orig.OnCancel == null || orig.OnCancel(); };
            arr[i].OnDefocus += () => { if (orig.OnDefocus != null) orig.OnDefocus(); };
            arr[i].OnDeselect += () => { if (orig.OnDeselect != null) orig.OnDeselect(); };
            arr[i].OnFocus += () => { if (orig.OnFocus != null) orig.OnFocus(); };
            arr[i].OnHighlight += () => { if (orig.OnHighlight != null) orig.OnHighlight(); };
            arr[i].OnHighlightEnded += () => { if (orig.OnHighlightEnded != null) orig.OnHighlightEnded(); };
            arr[i].OnInteract += () => { return orig.OnInteract != null && orig.OnInteract(); };
            arr[i].OnInteractEnded += () => { if (orig.OnInteractEnded != null) orig.OnInteractEnded(); };
            arr[i].OnLeft += () => { if (orig.OnLeft != null) orig.OnLeft(); };
            arr[i].OnRight += () => { if (orig.OnRight != null) orig.OnRight(); };
            arr[i].OnSelect += () => { if (orig.OnSelect != null) orig.OnSelect(); };
        }

        if (rowLength != null)
            parent.ChildRowLength = rowLength.Value;

        parent.Parent = null;
        parent.UpdateChildrenProperly();
    }
    /// <summary>
    /// <see langword="true"/> if and only if the attached <see cref="KMBombModule"/> is solved.
    /// </summary>
    protected bool IsSolved { get; private set; }

    private string _loggingTag;
    private string LoggingTag { get { return _loggingTag = _loggingTag ?? string.Format("[Module Skins] [{0}] ", SkinName); } }
    /// <summary>
    /// Log a message with the correct logging tag.
    /// </summary>
    /// <param name="message">The message to log.</param>
    protected void Log(string message)
    {
        Debug.Log(LoggingTag + message);
    }
    /// <summary>
    /// Log a formatted message with the correct logging tag.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The format arguments to interpolate into the message.</param>
    protected void LogFormat(string message, params string[] args)
    {
        Debug.LogFormat(LoggingTag + message, args);
    }

    /// <summary>
    /// Sounds to replace with ones from this mod.
    /// </summary>
    protected virtual Dictionary<string, string> SoundOverrides { get { return new Dictionary<string, string>(); } }

    protected virtual void OnSound(string name, Transform transform, bool isByRef) { }

    private static readonly HashSet<SkinName> s_initialized = new HashSet<SkinName>();
    /// <summary>
    /// The compound name of this skin.
    /// </summary>
    public SkinName SkinName { get { return new SkinName(ModuleId, Name); } }
    /// <summary>
    /// DO NOT USE IN SKIN. Used by the service to perform housekeeping functions.
    /// </summary>
    /// <param name="solvable"></param>
    /// <param name="needy"></param>
    public void Begin(KMBombModule solvable, KMNeedyModule needy)
    {
        if (s_initialized.Add(SkinName))
            Initialize();

        if (solvable != null)
        {
            solvable.OnPass += () => { OnSolve(); IsSolved = true; return false; };
            solvable.OnStrike += () => { OnStrike(); return false; };
            solvable.OnActivate += OnActivate;
        }
        else if (needy != null)
        {
            needy.OnPass += () => { OnSolve(); return false; };
            needy.OnStrike += () => { OnStrike(); return false; };
            needy.OnNeedyActivation += OnNeedyActivation;
            needy.OnNeedyDeactivation += OnNeedyDeactivation;
            needy.OnActivate += OnActivate;
        }

        OnStart();

        var audio = GetComponent<KMAudio>();
        if (audio)
        {
            var overrides = SoundOverrides;
            var origHandlerRef = audio.HandlePlaySoundAtTransformWithRef;
            audio.HandlePlaySoundAtTransformWithRef = (string name, Transform transform, bool loop) =>
            {
                string newName;
                KMAudio.KMAudioRef ret;
                if (overrides.TryGetValue(name, out newName))
                    ret = Audio.PlaySoundAtTransformWithRef(newName, transform);
                else
                    ret = origHandlerRef(name, transform, loop);

                OnSound(name, transform, true);
                return ret;
            };

            var origHandler = audio.HandlePlaySoundAtTransform;
            audio.HandlePlaySoundAtTransform = (string name, Transform transform) =>
            {
                string newName;
                if (overrides.TryGetValue(name, out newName))
                    Audio.PlaySoundAtTransform(newName, transform);
                else
                    origHandler(name, transform);

                OnSound(name, transform, false);
            };
        }
    }
}
