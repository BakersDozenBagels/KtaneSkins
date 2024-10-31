using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class TheRadioSkin : ModuleSkin
{
    public override string ModuleId { get { return "KritRadio"; } }
    public override string Name { get { return "ONE"; } }

    private TextMesh _origText, _newText, _amfmText;
    private Transform _knobL, _knobR, _antenna;
    private float _knobLRot, _knobRRot, _knobLTarget, _knobRTarget;
    private const float RotationTimeDelta = 500f, RotationPressDelta = 71f;

    private static Func<object, string> s_getAMFM, s_getBarcode;
    private static readonly Func<object, KMAudio>[] s_getAudio = new Func<object, KMAudio>[6];
    private object _comp;
    private KMAudio.PlaySoundAtTransformHandler _foreignAudio;
    private KMAudio.PlayGameSoundAtTransformHandler _foreignGameAudio;
    private KMBombModule.KMPassEvent _foreignPass;
    private KMBombModule.KMStrikeEvent _foreignStrike;
    private KMSelectable.KMOnAddInteractionPunchDelegate _foreignPunch;

    private string _solveSong;
    private bool _animating = false;

    private string AMFM { get { return s_getAMFM(_comp); } }
    private string Barcode { get { return s_getBarcode(_comp); } }

    protected override void Initialize()
    {
        var compType = GetComponent("KritRadio").GetType();
        var param = Expression.Parameter(typeof(object), "radio");
        var cast = Expression.Convert(param, compType);
        var body = Expression.Field(cast, compType.GetField("CurrentTransmission", BindingFlags.NonPublic | BindingFlags.Instance));
        s_getAMFM = Expression.Lambda<Func<object, string>>(body, param).Compile();

        body = Expression.Field(cast, compType.GetField("Barcode", BindingFlags.NonPublic | BindingFlags.Instance));
        s_getBarcode = Expression.Lambda<Func<object, string>>(body, param).Compile();

        string[] audios = new string[] { "FrequencyChange", "StaticNoise", "DutchSongs", "EnglishSongs", "GermanSongs", "FrenchSongs" };
        for (int i = 0; i < audios.Length; i++)
        {
            body = Expression.Field(cast, compType.GetField(audios[i], BindingFlags.Public | BindingFlags.Instance));
            s_getAudio[i] = Expression.Lambda<Func<object, KMAudio>>(body, param).Compile();
        }
    }

    protected override void OnStart()
    {
        _comp = GetComponent("KritRadio");

        var audio = GetComponent<KMAudio>();
        _foreignGameAudio = audio.HandlePlayGameSoundAtTransform;
        audio.HandlePlayGameSoundAtTransform = OnPlayGameAudio;
        _foreignAudio = audio.HandlePlaySoundAtTransform;

        foreach (var getAudio in s_getAudio)
            getAudio(_comp).HandlePlaySoundAtTransform = OnPlayAudio;

        var mod = GetComponent<KMBombModule>();
        _foreignPass = mod.OnPass;
        mod.OnPass = null;
        _foreignStrike = mod.OnStrike;
        mod.OnStrike = null;

        var sel = GetComponent<KMSelectable>();
        _foreignPunch = sel.OnInteractionPunch;
        sel.OnInteractionPunch = Punch;

        transform.GetChild(3).gameObject.SetActive(false);
        transform.GetChild(5).gameObject.SetActive(false);

        _knobLTarget = _knobLRot = Random.Range(0f, 360f);
        _knobRTarget = _knobRRot = Random.Range(0f, 360f);

        var skin = AddPrefab();

        _knobL = skin.GetChild(2);
        _knobR = skin.GetChild(5);

        var buttons = new KMSelectable[]
        {
            skin.GetChild(9).GetComponent<KMSelectable>(),
            skin.GetChild(4).GetComponent<KMSelectable>(),
            skin.GetChild(3).GetComponent<KMSelectable>(),
            _knobR.GetComponent<KMSelectable>(),
            null
        };

        buttons[1].OnInteract += () => { if (_animating || IsSolved) return false; _knobLTarget += RotationPressDelta; RandomStatic(); return false; };
        buttons[2].OnInteract += () => { if (_animating || IsSolved) return false; _knobLTarget -= RotationPressDelta; RandomStatic(); return false; };
        buttons[3].OnInteract += () => { if (_animating || IsSolved) return false; _knobRTarget += RotationPressDelta; RandomStatic(); return false; };

        SetSelectableChildren(buttons);

        _origText = transform.GetChild(3).GetChild(5).GetChild(2).GetComponent<TextMesh>();
        _newText = skin.GetChild(7).GetComponent<TextMesh>();
        _amfmText = skin.GetChild(12).GetComponent<TextMesh>();

        _antenna = skin.GetChild(9);

        skin.GetChild(10).GetComponent<TextMesh>().text = Barcode;

        transform.GetChild(2).gameObject.SetActive(false);

        StartCoroutine(DoUpdates());
    }

    private void RandomStatic()
    {
        var choice = Random.Range(0, 6);
        Audio.PlaySoundAtTransform("KritRadio_ONE_Glitch" + choice, transform);
    }

    private void Punch(float intensityModifier)
    {
        if (!_animating)
            _foreignPunch(intensityModifier);
    }

    private void OnPlayAudio(string name, Transform _)
    {
        if (Regex.IsMatch(name, "^(?:English|Dutch|French|German)Song[1-4]$"))
        {
            _solveSong = name;
            Log("Captured song: " + name);
            return;
        }

        if (name == "BackgroundStaticNoise" || name == "FrequencyChange")
            return;

        if (name == "StaticNoise")
            StartCoroutine(Animate(correct: true));
        else if (name == "StaticNoiseIncorrect")
            StartCoroutine(Animate(correct: false));
        else
            throw new ArgumentException("Unexpected audio clip: " + name);
    }

    private void OnPlayGameAudio(KMSoundOverride.SoundEffect sound, Transform transform)
    {
        if (_animating || sound == KMSoundOverride.SoundEffect.CorrectChime)
            return;
        _foreignGameAudio(sound, transform);
    }

    private IEnumerator Animate(bool correct)
    {
        _animating = true;
        Audio.PlaySoundAtTransform("KritRadio_ONE_Anticipation", transform);
        const float Length = 0.3f, Total = 3.6f, Pause = Total - Length;

        float t = Time.unscaledTime;
        while (Time.unscaledTime - t < Length)
        {
            _antenna.localEulerAngles = new Vector3(0, Easing.InOutQuad(Time.unscaledTime - t, -75, -45, Length), 0);
            yield return null;
        }
        _antenna.localEulerAngles = new Vector3(0, -45, 0);

        yield return new WaitForSecondsRealtime(Pause);

        _animating = false;

        if (correct)
        {
            _foreignAudio(_solveSong, transform);
            _foreignPass();
            yield return AnimateUM();
            yield break;
        }

        _foreignStrike();

        t = Time.unscaledTime;
        while (Time.unscaledTime - t < Length)
        {
            _antenna.localEulerAngles = new Vector3(0, Easing.InOutQuad(Time.unscaledTime - t, -45, -75, Length), 0);
            yield return null;
        }
        _antenna.localEulerAngles = new Vector3(0, -75, 0);
    }

    private IEnumerator AnimateUM()
    {
        const int ChunkSize = 6;
        const float Length = 0.7f;

        _newText.characterSize = 0.0007f;
        _newText.transform.localPosition = new Vector3(0.0598f, _newText.transform.localPosition.y, _newText.transform.localPosition.z);
        _amfmText.characterSize = 0.0007f;
        _amfmText.text = "UM";

        Action<byte[]> display = (byte[] digits) =>
        {
            int i = 0;
            for (; i < 61; i++)
                if (digits[i] != 0)
                    break;
            StringBuilder sb = new StringBuilder(68 - i);
            for (; i < 63; i++)
            {
                sb.Append(digits[i]);
                if (i % 16 == 15)
                    sb.Append('\n');
            }
            sb.Append('.');
            sb.Append(digits[63]);
            _newText.text = sb.ToString();
        };

        var num = (int)(float.Parse(_origText.text) * 10);
        var curDigits = new byte[64];
        curDigits[63] = (byte)(num % 10);
        curDigits[62] = (byte)(num / 10 % 10);
        curDigits[61] = (byte)(num / 100 % 10);
        curDigits[60] = (byte)(num / 1000 % 10);

        byte[] targetDigits;
        switch (Random.Range(0, 8))
        {
            case 0:
                Log("Prepare for sheeps and bears.");
                targetDigits = "7138649508438832189721586195907707345862575104780865241322794351".Select(c => (byte)(c - '0')).ToArray();
                break;
            case 1:
                Log("Prepare for stones.");
                targetDigits = "7138649508438832189721586195807707345862675104780865281322794351".Select(c => (byte)(c - '0')).ToArray();
                break;
            case 2:
                Log("Prepare for a hidden cabin.");
                targetDigits = "0000000048378956413610815748674551005235239932464538275497169283".Select(c => (byte)(c - '0')).ToArray();
                break;
            case 3:
                Log("Prepare for the big city.");
                targetDigits = "0000000004827195482846217323454468273542584763547619224876657456".Select(c => (byte)(c - '0')).ToArray();
                break;
            case 4:
            case 5:
            case 6:
            case 7:
                Log("Prepare for something unknown.");
                targetDigits = new byte[64];
                targetDigits[0] = (byte)Random.Range(1, 10);
                for (int i = 1; i < 64; i++)
                    targetDigits[i] = (byte)Random.Range(0, 10);
                break;
            default:
                throw new Exception();
        }

        var curTargets = new byte[64];
        var prevDigits = new byte[64];
        curDigits.CopyTo(curTargets, 0);
        for (int section = 0; section < 64 + ChunkSize; section += ChunkSize)
        {
            RandomStatic();
            curTargets.CopyTo(prevDigits, 0);
            int i = 0;
            for (; i < section && i < 64; i++)
                curTargets[i] = targetDigits[i];
            for (; i < 64; i++)
                curTargets[i] = (byte)Random.Range(i == 0 ? 1 : 0, 10);
            float t = Time.time;
            while (Time.time - t < Length)
            {
                float delta = (Time.time - t) / Length;
                for (int j = 0; j < 64; j++)
                    curDigits[j] = (byte)Mathf.Lerp(prevDigits[j], curTargets[j], delta);
                display(curDigits);
                yield return null;
            }
        }
        display(targetDigits);
        Log("Tuned to " + _newText.text.Replace('\n', ' '));
    }

    private IEnumerator DoUpdates()
    {
        while (!IsSolved)
        {
            var max = RotationTimeDelta * Time.deltaTime;
            _knobLRot += Mathf.Clamp(_knobLTarget - _knobLRot, -max, max);
            _knobRRot += Mathf.Clamp(_knobRTarget - _knobRRot, -max, max);
            _knobL.localEulerAngles = new Vector3(90f, 0f, -_knobLRot);
            _knobR.localEulerAngles = new Vector3(90f, 0f, -_knobRRot);

            if (!_animating)
            {
                _newText.text = _origText.text;
                _amfmText.text = AMFM;
            }

            yield return null;
        }
    }
}
