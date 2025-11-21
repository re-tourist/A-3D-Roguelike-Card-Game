using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class StyleProxy : MonoBehaviour
{
    public TMP_FontAsset fontAsset;
    public bool useTimesNewRoman = true;
    public float fontSize = 24f;
    public float lineSpacing = 4f;
    public TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft;
    public float layoutSpacing = 16f;
    public string prefsKey = "MainMenuButtonsPanel_Style";

    float _lastFontSize;
    float _lastLineSpacing;
    TextAlignmentOptions _lastAlign;
    float _lastLayoutSpacing;
    TMP_FontAsset _lastFontAsset;
    bool _lastUseTimes;

    public void ApplyNow()
    {
        var f = ResolveTMPFont();
        var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            t.fontSize = fontSize;
            t.lineSpacing = lineSpacing;
            t.alignment = alignment;
            if (f != null) t.font = f;
        }
        var vlg = GetComponent<VerticalLayoutGroup>();
        if (vlg != null) vlg.spacing = layoutSpacing;

        _lastFontSize = fontSize;
        _lastLineSpacing = lineSpacing;
        _lastAlign = alignment;
        _lastLayoutSpacing = layoutSpacing;
        _lastFontAsset = fontAsset;
        _lastUseTimes = useTimesNewRoman;
    }

    TMP_FontAsset ResolveTMPFont()
    {
        if (fontAsset != null) return fontAsset;
        return TMP_Settings.defaultFontAsset;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying) ApplyNow();
    }
#endif

    void OnEnable()
    {
        ApplyNow();
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (_lastFontSize != fontSize || _lastLineSpacing != lineSpacing || _lastAlign != alignment ||
            _lastLayoutSpacing != layoutSpacing || _lastFontAsset != fontAsset || _lastUseTimes != useTimesNewRoman)
        {
            ApplyNow();
        }
    }

    void OnDisable()
    {
        var s = new S
        {
            fontSize = fontSize,
            lineSpacing = lineSpacing,
            alignment = (int)alignment,
            layoutSpacing = layoutSpacing,
            useTimesNewRoman = useTimesNewRoman,
            fontPath = ""
        };
#if UNITY_EDITOR
        if (fontAsset != null)
            s.fontPath = UnityEditor.AssetDatabase.GetAssetPath(fontAsset);
#endif
        PlayerPrefs.SetString(prefsKey, JsonUtility.ToJson(s));
        PlayerPrefs.Save();
    }

    struct S { public float fontSize; public float lineSpacing; public int alignment; public float layoutSpacing; public bool useTimesNewRoman; public string fontPath; }
}
