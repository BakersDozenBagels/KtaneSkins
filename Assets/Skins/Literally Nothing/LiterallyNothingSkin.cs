using UnityEngine;

public class LiterallyNothingSkin : ModuleSkin
{
    public override string ModuleId { get { return "literallyNothing"; } }
    public override string Name { get { return "WhatHow"; } }

    protected override void OnStart()
    {
        var skin = Instantiate(GetPrefab("literallyNothing_WhatHow"), transform);
        skin.transform.localPosition = Vector3.zero;
        GetComponent<KMSelectable>().OnInteract += () =>
        {
            Audio.PlaySoundAtTransform("'What' Bottom Text Meme (Sanctuary Guardian) - Sound Effect (HD) [pKzCXWGvIcg]", transform);
            return true;
        };
    }
}
