﻿using System.Collections;
using UnityEngine;

public class BrainfSkin : ModuleSkin
{
    public override string ModuleId { get { return "brainf"; } }
    public override string Name { get { return "Rolodex"; } }
    private TextMesh _mainText;
    private Animator _rolodex;

    private const string SymbolOrder = "-+<>,.[]";

    protected override void OnStart()
    {
        var skin = AddPrefab("Brainf_Rolodex");
        _rolodex = skin.GetComponentInChildren<Animator>();

        var mainScreen = transform.GetChild(2);
        _mainText = mainScreen.GetComponentInChildren<TextMesh>();
        mainScreen.gameObject.SetActive(false);

        // Keypad
        transform.GetChild(0).gameObject.SetActive(false);
        // Background
        transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        // Stage screen
        transform.GetChild(3).gameObject.SetActive(false);

        KMSelectable[] children = new KMSelectable[12];
        for (int i = 0; i < 12; ++i)
        {
            var button = skin.GetChild(2).GetChild(i).GetComponent<KMSelectable>();
            var pos = button.transform.localPosition;
            button.OnInteract += () =>
            {
                StartCoroutine(AnimateButton(button.transform, pos));
                return false;
            };
            children[i] = button;
        }

        children[10].OnInteract += () =>
        {
            Audio.PlaySoundAtTransform("Brainf_Rolodex_Solve", transform);
            return false;
        };

        SetSelectableChildren(children);

        var comp = GetComponent("BrainfScript");
        comp.GetType().SetField("stageMesh", comp, skin.GetChild(3).GetComponentInChildren<TextMesh>());

        StartCoroutine(AnimateRolodex());
    }

    private IEnumerator AnimateButton(Transform button, Vector3 orig)
    {
        const float Delta = -0.005f, Delay = 0.1f;

        float t = Time.time;
        while (Time.time - t < Delay)
        {
            button.localPosition = new Vector3(orig.x, orig.y + Mathf.Lerp(0, Delta, (Time.time - t) / Delay), orig.z);
            yield return null;
        }
        t = Time.time;
        while (Time.time - t < Delay)
        {
            button.localPosition = new Vector3(orig.x, orig.y + Mathf.Lerp(Delta, 0, (Time.time - t) / Delay), orig.z);
            yield return null;
        }
        button.transform.localPosition = orig;
    }

    private IEnumerator AnimateRolodex()
    {
        while (true)
        {
            var symbol = SymbolOrder.IndexOf(_mainText.text);
            if (symbol != -1)
                _rolodex.SetInteger("Symbol", symbol);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
