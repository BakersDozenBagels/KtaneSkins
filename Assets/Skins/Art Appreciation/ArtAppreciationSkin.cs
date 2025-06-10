using System;
using System.Collections.Generic;
using UnityEngine;

public class ArtAppreciationSkin : ModuleSkin
{
    public override string ModuleId { get { return "AppreciateArt"; } }
    public override string Name { get { return "EaTEoT"; } }

    private static Action<object, Material> s_setPostProcessMaterial;
    private Material PostProcessMaterial { set { s_setPostProcessMaterial(GetComponent("AppreciateArtModule"), value); } }

    protected override void Initialize()
    {
        var compType = GetComponent("AppreciateArtModule").GetType();
        var fldPostProcessMat = compType.GetField("PostProcessMaterial", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        s_setPostProcessMaterial = (c, m) => fldPostProcessMat.SetValue(c, m);
    }

    protected override Dictionary<string, string> SoundOverrides { get { return new Dictionary<string, string>() { { "Ambient", GetPrefab().GetComponent<ArtAppreciationArt>().Audio.name } }; } }

    protected override void OnStart()
    {
        var prefab = GetPrefab().GetComponent<ArtAppreciationArt>();
        var art = prefab.Art.PickRandom();

        LogFormat("Chose album cover {0}", art.name);

        var picFrame = transform.GetChild(3).GetChild(0);
        picFrame.GetChild(2).GetComponent<MeshRenderer>().material.mainTexture = art;
        picFrame.GetChild(3).GetChild(0).GetComponent<TextMesh>().text = "ART ~ Ivan Seal";

        PostProcessMaterial = prefab.PostProcessMaterial;
    }
}
