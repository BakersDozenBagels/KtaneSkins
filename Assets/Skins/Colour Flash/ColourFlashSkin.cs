using System.Collections;
using UnityEngine;

public class ColourFlashSkin : ModuleSkin
{
    public override string ModuleId { get { return "ColourFlash"; } }

    public override string Name { get { return "Modern"; } }

    private TextMesh _screenText, _yesText, _noText;
    private Material _screenMat;

    protected override void OnStart()
    {
        transform.GetChild(2).gameObject.SetActive(false);
        var skin = AddPrefab();

        _screenText = skin.transform.Find("ScreenText").GetComponent<TextMesh>();
        _screenMat = skin.transform.Find("Screen").GetComponent<Renderer>().material;
        _yesText = skin.transform.Find("YesText").GetComponent<TextMesh>();
        _noText = skin.transform.Find("NoText").GetComponent<TextMesh>();

        var comp = GetComponent("ColourFlashModule");
        comp.GetType().SetField("Indicator", comp, _screenText);

        var yesSel = _yesText.GetComponent<KMSelectable>();
        var noSel = _noText.GetComponent<KMSelectable>();

        SetSelectableChildren(yesSel, noSel);

        _screenText.gameObject.SetActive(false);
        _yesText.gameObject.SetActive(false);
        _noText.gameObject.SetActive(false);
    }

    protected override void OnActivate()
    {
        _screenText.gameObject.SetActive(true);
        _yesText.gameObject.SetActive(true);
        _noText.gameObject.SetActive(true);
    }

    protected override void OnSolve()
    {
        StartCoroutine(SolveAnimation());
    }

    private IEnumerator SolveAnimation()
    {
        _screenText.gameObject.SetActive(false);
        _yesText.gameObject.SetActive(false);
        _noText.gameObject.SetActive(false);
        Audio.PlaySoundAtTransform("ColourFlash_Modern_Solve", transform);
        _screenMat.SetColor("_ColorB", Color.white);
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.07f, 0.2f));
        _screenMat.SetColor("_ColorB", Color.black);
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.07f, 0.2f));
        _screenMat.SetColor("_ColorB", Color.green);
    }

    protected override void OnStrike()
    {
        StartCoroutine(StrikeAnimation());
    }

    private IEnumerator StrikeAnimation()
    {
        _screenText.gameObject.SetActive(false);
        _yesText.gameObject.SetActive(false);
        _noText.gameObject.SetActive(false);
        _screenMat.SetColor("_ColorB", Color.red);
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.95f, 1.05f));
        _screenText.gameObject.SetActive(true);
        _yesText.gameObject.SetActive(true);
        _noText.gameObject.SetActive(true);
        _screenMat.SetColor("_ColorB", Color.black);
    }
}
