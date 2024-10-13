using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlungerButtonHologram : ModuleSkin
{
    public override string ModuleId { get { return "plungerButton"; } }
    public override string Name { get { return "Hologram"; } }

    private const float CapMax = 0.0764f, CapMin = 0.03f, StemMin = 0.015f, StemOffset = 0.005f, AnimationLength = 2f, StemWidth = 0.75f;
    private const int StemRepeats = 7;

    private float CapPosition { get { return _cap.localPosition.y; } set { _cap.SetPositionY(value); } }
    private float _t = 0f;
    private Coroutine _routine;
    private Renderer[] _stems;
    private Transform _cap;
    private Renderer _surface;
    private Material _surfaceOff, _surfaceOn;
    private KMAudio.KMAudioRef _ref;

    protected override void OnStart()
    {
        var skin = Instantiate(GetPrefab("plungerButton_Hologram"), transform).transform;
        skin.localPosition = Vector3.zero;

        var fakeSurface = skin.GetChild(2).GetComponent<Renderer>();
        var comp = GetComponent("plungerButtonScript");
        comp.GetType().SetField("surface", comp, fakeSurface);

        _surface = transform.GetChild(0).GetChild(3).GetComponent<Renderer>();
        _surfaceOff = _surface.material;
        _surfaceOn = fakeSurface.material;

        var child = skin.GetChild(0).GetComponent<KMSelectable>();
        var parent = GetComponent<KMSelectable>();
        parent.Children[0].OnInteract += Press;
        parent.Children[0].OnInteractEnded += Release;
        child.OnInteract = parent.Children[0].OnInteract;
        child.OnInteractEnded = parent.Children[0].OnInteractEnded;
        child.Parent = parent;
        parent.Children[0] = child;
        parent.Parent = null; // The original has Parent set to itself, which freezes the game after CopySettingsFromProxy() and interacting with the bomb.
        parent.UpdateChildrenProperly();

        // Original button
        transform.GetChild(1).gameObject.SetActive(false);

        _cap = skin.GetChild(0);
        var stemTemplate = skin.GetChild(1);
        _stems = new Renderer[StemRepeats];
        for (int i = 0; i < StemRepeats; i++)
        {
            var stem = Instantiate(stemTemplate, skin);
            stem.gameObject.SetActive(true);
            _stems[i] = stem.GetComponent<Renderer>();
        }

        StartCoroutine(AnimateStem());
    }

    protected override void OnSolve()
    {
        Audio.PlaySoundAtTransform("578803__nomiqbomi__sparkle", transform);
    }

    private bool Press()
    {
        if (_routine != null)
            StopCoroutine(_routine);
        _routine = StartCoroutine(AnimateCap(CapMin));
        _surface.material = _surfaceOn;
        _ref = Audio.PlaySoundAtTransformWithRef("344534__newagesoup__electric-cicadas02", transform);
        return false;
    }

    private void Release()
    {
        if (_routine != null)
            StopCoroutine(_routine);
        _routine = StartCoroutine(AnimateCap(CapMax));
        _surface.material = _surfaceOff;
        if (_ref != null && _ref.StopSound != null)
            _ref.StopSound();
    }

    private IEnumerator AnimateCap(float to)
    {
        const float Duration = 0.04f;

        float from = CapPosition;
        float t = Time.time;

        while (Time.time - t < Duration)
        {
            CapPosition = Mathf.Lerp(from, to, (Time.time - t) / Duration);
            yield return null;
        }
        CapPosition = to;
    }

    private IEnumerator AnimateStem()
    {
        while (true)
        {
            _t += Time.deltaTime;
            _t %= AnimationLength;
            var t = _t / AnimationLength;

            var stemMax = CapPosition - StemOffset;

            for (int i = 0; i < StemRepeats; i++)
            {
                var ti = (t + (float)i / StemRepeats) % 1f;
                _stems[i].transform.SetPositionY(Mathf.Lerp(stemMax, StemMin, ti));
                _stems[i].material.SetFloat("_t", StemWidth * (1 - ti) * (1 - ti));

                var alpha = Mathf.Clamp01(ti * 10);
                _stems[i].material.color = new Color(0.1f, 0.5f, 0.3f, alpha);
            }
            yield return null;
        }
    }
}
