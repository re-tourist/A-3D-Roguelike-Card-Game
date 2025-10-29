using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 在运行时于主菜单场景下构建左下角按钮列。
/// 若已经手动搭建UI，可不启用该脚本或关闭 buildOnStart。
/// </summary>
public class MainMenuUIBuilder : MonoBehaviour
{
    public bool buildOnStart = true;
    public Vector2 panelSize = new Vector2(320f, 380f);
    public Vector2 panelOffset = new Vector2(24f, 24f); // 距离左下角的偏移
    public Font defaultFont; // 可选：自定义字体

    void Start()
    {
        if (buildOnStart)
        {
            BuildIfMissing();
        }
    }

    public void BuildIfMissing()
    {
        var controller = GetComponent<MainMenuController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<MainMenuController>();
        }

        // 已存在按钮则不重复创建
        if (controller.continueButton != null && controller.newGameButton != null &&
            controller.abandonButton != null && controller.menuButton != null && controller.exitButton != null)
        {
            return;
        }

        var canvas = GetComponentInParent<Canvas>();
        var parent = canvas != null ? canvas.transform : transform;

        // 创建面板并放置在左下角
        var panelGo = new GameObject("MainMenuButtonsPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.SetParent(parent, false);
        panelRt.anchorMin = new Vector2(0f, 0f);
        panelRt.anchorMax = new Vector2(0f, 0f);
        panelRt.pivot = new Vector2(0f, 0f);
        panelRt.sizeDelta = panelSize;
        panelRt.anchoredPosition = panelOffset;

        var panelImg = panelGo.GetComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.35f); // 半透明黑背景

        var vlg = panelGo.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(12, 12, 12, 12);
        vlg.spacing = 12f;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        var csf = panelGo.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 创建按钮
        controller.buttonsPanel = panelRt;
        controller.continueButton = CreateButton(panelRt, "继续游戏", OnContinueClick);
        controller.newGameButton = CreateButton(panelRt, "新游戏", OnNewGameClick);
        controller.abandonButton = CreateButton(panelRt, "放弃当前游戏", OnAbandonClick);
        controller.menuButton = CreateButton(panelRt, "菜单", OnMenuClick);
        controller.exitButton = CreateButton(panelRt, "退出", OnExitClick);
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

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var trt = textGo.GetComponent<RectTransform>();
        trt.SetParent(go.transform, false);
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(12f, 8f);
        trt.offsetMax = new Vector2(-12f, -8f);

        var text = textGo.GetComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
        text.fontSize = 20;
        if (defaultFont != null) text.font = defaultFont;

        return btn;
    }

    // 代理点击事件到 MainMenuController（避免重复查找）
    void OnContinueClick() => GetComponent<MainMenuController>().SendMessage("OnContinue", SendMessageOptions.DontRequireReceiver);
    void OnNewGameClick() => GetComponent<MainMenuController>().SendMessage("OnNewGame", SendMessageOptions.DontRequireReceiver);
    void OnAbandonClick() => GetComponent<MainMenuController>().SendMessage("OnAbandon", SendMessageOptions.DontRequireReceiver);
    void OnMenuClick() => GetComponent<MainMenuController>().SendMessage("OnMenu", SendMessageOptions.DontRequireReceiver);
    void OnExitClick() => GetComponent<MainMenuController>().SendMessage("OnExit", SendMessageOptions.DontRequireReceiver);
}