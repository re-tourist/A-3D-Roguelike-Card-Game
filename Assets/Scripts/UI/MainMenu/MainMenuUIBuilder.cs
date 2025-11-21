using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
 

/// <summary>
/// 在运行时于主菜单场景下构建左下角按钮列。
/// 若已经手动搭建UI，可不启用该脚本或关闭 buildOnStart。
/// </summary>
public class MainMenuUIBuilder : MonoBehaviour
{
    public bool buildOnStart = false;
    public Vector2 panelSize = new Vector2(360f, 380f);
    public Vector2 panelOffset = new Vector2(80f, 0f);
    public Font defaultFont; // 可选：自定义字体
    public bool allowDragInPlay = true;
    public bool rememberPosition = false;
    public float layoutSpacing = 20f;
    public float globalFontSize = 28f;
    public float globalLineSpacing = 4f;
    public TMP_FontAsset fontAsset;
    public bool useTimesNewRoman = true;
    public TextAlignmentOptions textAlignment = TextAlignmentOptions.Center;
    public PanelAnchor anchor = PanelAnchor.CenterLeft;

    public enum PanelAnchor { BottomLeft, BottomRight, TopLeft, TopRight, CenterLeft, CenterRight, Center }

    struct P { public float x; public float y; }
    struct S { public float fontSize; public float lineSpacing; public int alignment; public float layoutSpacing; public bool useTimesNewRoman; public string fontPath; }

 

 

    public void BuildIfMissing()
    {
        var controller = GetComponent<MainMenuController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<MainMenuController>();
        }

        // 已存在按钮则不重复创建
        if (controller.continueButton != null && controller.newGameButton != null &&
            controller.abandonButton != null && controller.settingsButton != null && controller.exitButton != null)
        {
            return;
        }

        var canvas = GetComponentInParent<Canvas>();
        var parent = canvas != null ? canvas.transform : transform;

        var rootGo = new GameObject("MainMenu", typeof(RectTransform));
        var rootRt = rootGo.GetComponent<RectTransform>();
        rootRt.SetParent(parent, false);
        rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero; rootRt.offsetMax = Vector2.zero;

        var leftBar = new GameObject("LeftBlackBar", typeof(RectTransform), typeof(Image));
        var lrt = leftBar.GetComponent<RectTransform>();
        lrt.SetParent(rootGo.transform, false);
        lrt.anchorMin = new Vector2(0f, 0f); lrt.anchorMax = new Vector2(0f, 1f);
        lrt.pivot = new Vector2(0f, 0.5f);
        lrt.sizeDelta = new Vector2(240f, 0f);
        lrt.anchoredPosition = new Vector2(0f, 0f);
        var limg = leftBar.GetComponent<Image>(); limg.color = Color.black; limg.raycastTarget = false;

        var rightBar = new GameObject("RightBlackBar", typeof(RectTransform), typeof(Image));
        var rrt = rightBar.GetComponent<RectTransform>();
        rrt.SetParent(rootGo.transform, false);
        rrt.anchorMin = new Vector2(1f, 0f); rrt.anchorMax = new Vector2(1f, 1f);
        rrt.pivot = new Vector2(1f, 0.5f);
        rrt.sizeDelta = new Vector2(240f, 0f);
        rrt.anchoredPosition = new Vector2(0f, 0f);
        var rimg = rightBar.GetComponent<Image>(); rimg.color = Color.black; rimg.raycastTarget = false;

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.SetParent(rootGo.transform, false);
        titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -80f);
        titleRt.sizeDelta = new Vector2(1000f, 100f);
        var title = titleGo.GetComponent<TextMeshProUGUI>();
        title.text = "万象骗局";
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 64f;
        title.color = Color.white;
        var tf = ResolveTMPFont(); if (tf != null) title.font = tf;

        var panelGo = new GameObject("WidgetList", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.SetParent(rootGo.transform, false);
        panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = new Vector2(0f, -20f);
        panelRt.sizeDelta = new Vector2(panelSize.x, panelSize.y);
        var panelImg = panelGo.GetComponent<Image>(); panelImg.color = new Color(0f, 0f, 0f, 0.35f);
        var vlg = panelGo.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(12, 12, 12, 12);
        vlg.spacing = layoutSpacing;
        vlg.childControlHeight = true; vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
        var csf = panelGo.GetComponent<ContentSizeFitter>(); csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        controller.buttonsPanel = panelRt;
        controller.newGameButton = CreateButton(panelRt, "开始游戏", OnNewGameClick);
        controller.continueButton = CreateButton(panelRt, "继续当前游戏", OnContinueClick);
        controller.abandonButton = CreateButton(panelRt, "放弃当前游戏", OnAbandonClick);
        controller.settingsButton = CreateButton(panelRt, "设置", OnSettingsClick);
        controller.exitButton = CreateButton(panelRt, "退出", OnExitClick);
        controller.continueButton.gameObject.SetActive(false);
        controller.abandonButton.gameObject.SetActive(false);
    }

    Button CreateButton(RectTransform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + "_Button", typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(panelSize.x - 24f, 48f);

        var img = go.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.1f);

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 56f;
        le.preferredHeight = 56f;
        le.flexibleWidth = 1f;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = textGo.GetComponent<RectTransform>();
        trt.SetParent(go.transform, false);
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(12f, 8f);
        trt.offsetMax = new Vector2(-12f, -8f);

        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = textAlignment;
        text.color = Color.white;
        text.fontSize = globalFontSize;
        text.lineSpacing = globalLineSpacing;
        var f = ResolveTMPFont();
        if (f != null) text.font = f;

        return btn;
    }

    // 代理点击事件到 MainMenuController（避免重复查找）
    void OnContinueClick() => GetComponent<MainMenuController>().SendMessage("OnContinue", SendMessageOptions.DontRequireReceiver);
    void OnNewGameClick() => GetComponent<MainMenuController>().SendMessage("OnNewGame", SendMessageOptions.DontRequireReceiver);
    void OnAbandonClick() => GetComponent<MainMenuController>().SendMessage("OnAbandon", SendMessageOptions.DontRequireReceiver);
    void OnSettingsClick() => GetComponent<MainMenuController>().SendMessage("OnSettings", SendMessageOptions.DontRequireReceiver);
    void OnExitClick() => GetComponent<MainMenuController>().SendMessage("OnExit", SendMessageOptions.DontRequireReceiver);

    TMP_FontAsset ResolveTMPFont()
    {
        if (fontAsset != null) return fontAsset;
        return TMP_Settings.defaultFontAsset;
    }

    void ApplyStyles(RectTransform panel)
    {
        var texts = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
        var f = ResolveTMPFont();
        foreach (var t in texts)
        {
            t.fontSize = globalFontSize;
            t.lineSpacing = globalLineSpacing;
            t.alignment = textAlignment;
            if (f != null) t.font = f;
        }
        var vlg = panel.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) vlg.spacing = layoutSpacing;
    }

 

 

    void ApplyAnchor(RectTransform rt, PanelAnchor a)
    {
        switch (a)
        {
            case PanelAnchor.BottomLeft:
                rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(0f, 0f); rt.pivot = new Vector2(0f, 0f); break;
            case PanelAnchor.BottomRight:
                rt.anchorMin = new Vector2(1f, 0f); rt.anchorMax = new Vector2(1f, 0f); rt.pivot = new Vector2(1f, 0f); break;
            case PanelAnchor.TopLeft:
                rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f); break;
            case PanelAnchor.TopRight:
                rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(1f, 1f); break;
            case PanelAnchor.CenterLeft:
                rt.anchorMin = new Vector2(0f, 0.5f); rt.anchorMax = new Vector2(0f, 0.5f); rt.pivot = new Vector2(0f, 0.5f); break;
            case PanelAnchor.CenterRight:
                rt.anchorMin = new Vector2(1f, 0.5f); rt.anchorMax = new Vector2(1f, 0.5f); rt.pivot = new Vector2(1f, 0.5f); break;
            case PanelAnchor.Center:
                rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f); break;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Main Menu/Setup Static UI")]
    static void SetupStaticUI()
    {
        var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGo = new GameObject("UI_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = canvasGo.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvas = c;
        }
        var host = new GameObject("MainMenu", typeof(MainMenuController), typeof(MainMenuUIBuilder));
        host.transform.SetParent(canvas.transform, false);
        var builder = host.GetComponent<MainMenuUIBuilder>();
        builder.BuildIfMissing();
        UnityEditor.Selection.activeObject = host;
    }
#endif

 
}
