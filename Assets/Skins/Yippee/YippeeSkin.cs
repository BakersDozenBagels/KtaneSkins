using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class YippeeSkin : ModuleSkin
{
    public override string ModuleId { get { return "Yippee"; } }
    public override string Name { get { return "HD"; } }

    protected override Dictionary<string, string> SoundOverrides { get { return new Dictionary<string, string>() { { "solve sound", "Yippee" } }; } }

    private static Func<object, Material[]> s_getMaterials;
    private Material[] Materials { get { return s_getMaterials(GetComponent("Yippee")); } }

    protected override void Initialize()
    {
        var t = GetComponent("Yippee").GetType();

        var param = Expression.Parameter(typeof(object), "component");
        var paramT = Expression.Convert(param, t);
        var get = Expression.Field(paramT, t.GetField("materials", BindingFlags.Instance | BindingFlags.Public));
        s_getMaterials = Expression.Lambda<Func<object, Material[]>>(get, param).Compile();
    }

    private Transform _prefab;

    protected override void OnStart()
    {
        transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
        transform.GetChild(1).transform.localPosition = Vector3.zero;
        transform.GetChild(1).transform.localScale = Vector3.one * 0.1f;
        _prefab = AddPrefab();
        var mats = Materials;
        mats[1] = mats[0];
        mats[3] = mats[2];
    }

    protected override void OnSound(string name, Transform transform, KMAudio.KMAudioRef audioRef)
    {
        if (name == "solve sound")
            StartCoroutine(Anim());
    }

    private IEnumerator Anim()
    {
        _prefab.GetChild(1).gameObject.SetActive(true);
        foreach (var p in _prefab.GetComponentsInChildren<ParticleSystem>())
        {
            //p.transform.localScale *= transform.lossyScale.x;
            //var main = p.main;
            //main.simulationSpace = ParticleSystemSimulationSpace.Local;
            //p.GetComponent<ParticleSystemRenderer>().
            //main.startSizeMultiplier *= 1f / transform.lossyScale.x;
            //var spd = main.startSpeed;
            //spd.constantMin *= transform.lossyScale.x;
            //spd.constantMax *= transform.lossyScale.x;
            p.Play();
        }
        yield return new WaitForSeconds(2.5f);
        _prefab.GetChild(1).gameObject.SetActive(false);
    }
}