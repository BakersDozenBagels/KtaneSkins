using System;
using System.Collections.Generic;
using UnityEngine;

internal abstract class ModuleSkin : MonoBehaviour
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
    /// Called once ever. Use this to set up Harmony patches.
    /// Be sure to disable them if <see cref="ModuleSkinService.IsEnabled"/> is <see langword="false"/>.
    /// </summary>
    protected virtual void Initialize() { }
    /// <summary>
    /// Called before the skinned module receives Awake().
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

    private static readonly HashSet<SkinName> s_initialized = new HashSet<SkinName>();
    public SkinName SkinName { get { return new SkinName(ModuleId, Name); } }
    public void Begin(KMBombModule solvable, KMNeedyModule needy)
    {
        if (s_initialized.Add(SkinName))
            Initialize();

        if (solvable != null)
        {
            solvable.OnPass += () => { OnSolve(); return false; };
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
    }
}
