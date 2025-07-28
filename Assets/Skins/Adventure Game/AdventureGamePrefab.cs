using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdventureGamePrefab : MonoBehaviour
{
    const float MIN_Z = -0.05981505f, MAX_Z = 0.05981505f;

    [SerializeField]
    private Texture2D[] _textures;
    [SerializeField]
    private TextMesh _stats;
    [SerializeField]
    private Renderer _enemy;
    [SerializeField]
    private GameObject _button;
    [SerializeField]
    private GameObject _defeated;

    private List<GameObject> _buttons;
    private Coroutine _animation;

    private Dictionary<string, Texture2D> _textureDict;
    private Texture2D GetTexure(string name) { return (_textureDict = _textureDict ?? _textures.ToDictionary(x => x.name, x => x))[name]; }

    public string Stats { set { _stats.text = value; } }
    public string Enemy { set { _enemy.material.mainTexture = GetTexure(value); } }
    public List<string> Items
    {
        set
        {
            if (value.Count > _buttons.Count) throw new ArgumentOutOfRangeException();

            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = GetTexure(value[i]);
                _buttons[i].SetActive(true);
            }
        }
    }

    public KMSelectable[] GetSelectables() { return _buttons.Select(b => b.GetComponent<KMSelectable>()).ToArray(); }

    private void Awake()
    {
        _buttons = new List<GameObject>(8) { _button };
        for (int i = 0; i < 7; i++)
            _buttons.Add(Instantiate(_button, _button.transform.parent));
        for (int i = 0; i < 8; i++)
        {
            _buttons[i].transform.SetPositionZ(MIN_Z + ((MAX_Z - MIN_Z) / 7f) * i);
            _buttons[i].SetActive(false);
        }

        _defeated.SetActive(false);
    }

    public void Remove(int i)
    {
        Destroy(_buttons[i]);
        _buttons.RemoveAt(i);
        if (_animation != null)
            StopCoroutine(_animation);
        _animation = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float[] starts = _buttons.Select(b => b.transform.localPosition.z).ToArray();
        float[] ends = _buttons.Select((_, i) => MIN_Z + ((MAX_Z - MIN_Z) / (_buttons.Count - 1)) * i).ToArray();

        const float Duration = 1.4f;
        float start = Time.time;
        while (Time.time - start < Duration)
        {
            for (int i = 0; i < _buttons.Count; i++)
                _buttons[i].transform.SetPositionZ(Easing.InOutSine(Time.time - start, starts[i], ends[i], Duration));
            yield return null;
        }
        for (int i = 0; i < _buttons.Count; i++)
            _buttons[i].transform.SetPositionZ(ends[i]);
    }

    public void Solve() { _defeated.SetActive(true); }
}
