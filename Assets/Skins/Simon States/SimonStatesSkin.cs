using System.Linq;
using UnityEngine;

public class SimonStatesSkin : ModuleSkin
{
    public override string ModuleId { get { return "SimonV2"; } }
    public override string Name { get { return "USA"; } }

    protected override void OnStart()
    {
        var x = GetPrefab("USA").transform;
        var chosen = Enumerable
            .Range(0, x.childCount)
            .OrderBy(_ => Random.Range(0f, 1f))
            .Take(4)
            .Select(x.GetChild)
            .Select(m => m.GetComponent<MeshFilter>().sharedMesh)
            .ToArray();

        Log("Chose buttons: " + chosen.Select(m => m.name).Join(", "));

        for (int i = 0; i < 4; i++)
            transform.GetChild(i + 3).GetComponent<MeshFilter>().sharedMesh = chosen[i];
    }
}
