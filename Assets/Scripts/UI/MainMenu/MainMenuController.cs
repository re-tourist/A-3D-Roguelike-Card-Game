using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using Game.Localization;

    public class MainMenuController : MonoBehaviour
    {
    [Header("按钮容器（可选）")]
    public RectTransform buttonsPanel;

    [Header("按钮引用")]
    public Button continueButton;
    public Button newGameButton;
    public Button abandonButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("行为配置")]
    public bool hideContinueWhenNoSave = true;
        public bool hideAbandonWhenNoSave = false;
        GameObject exitModal;

    void Start()
    {
        WireButtons();
        RefreshButtonsBySaveState();
    }

    void OnEnable()
    {
        RefreshButtonsBySaveState();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowExitModal();
        }
    }

    public GameObject settingsPanel; // 可选：设置面板（无则运行时创建）

    void WireButtons()
    {
        AutoWireIfMissing();
        if (continueButton != null) { continueButton.onClick.RemoveAllListeners(); continueButton.onClick.AddListener(OnContinue); }
        if (newGameButton != null) { newGameButton.onClick.RemoveAllListeners(); newGameButton.onClick.AddListener(OnNewGame); }
        if (abandonButton != null) { abandonButton.onClick.RemoveAllListeners(); abandonButton.onClick.AddListener(OnAbandon); }
        if (settingsButton != null) { settingsButton.onClick.RemoveAllListeners(); settingsButton.onClick.AddListener(OnSettings); }
        if (exitButton != null) { exitButton.onClick.RemoveAllListeners(); exitButton.onClick.AddListener(OnExit); }
    }

    void AutoWireIfMissing()
    {
        var btns = GetComponentsInChildren<UnityEngine.UI.Button>(true);
        System.Func<UnityEngine.UI.Button, string> getLabel = b =>
        {
            var t = b.transform.Find("Text")?.GetComponent<TMPro.TextMeshProUGUI>();
            return t != null ? t.text : b.gameObject.name;
        };
        if (newGameButton == null)
        {
            foreach (var b in btns) { var label = getLabel(b); if (!string.IsNullOrEmpty(label) && (label.Contains("开始") || label.Contains("Start"))) { newGameButton = b; break; } }
        }
        if (continueButton == null)
        {
            foreach (var b in btns) { var label = getLabel(b); if (!string.IsNullOrEmpty(label) && (label.Contains("继续") || label.Contains("Continue"))) { continueButton = b; break; } }
        }
        if (abandonButton == null)
        {
            foreach (var b in btns) { var label = getLabel(b); if (!string.IsNullOrEmpty(label) && (label.Contains("放弃") || label.Contains("Give Up") || label.Contains("Abandon"))) { abandonButton = b; break; } }
        }
        if (settingsButton == null)
        {
            foreach (var b in btns) { var label = getLabel(b); if (!string.IsNullOrEmpty(label) && (label.Contains("设置") || label.Contains("Settings"))) { settingsButton = b; break; } }
        }
        if (exitButton == null)
        {
            foreach (var b in btns) { var label = getLabel(b); if (!string.IsNullOrEmpty(label) && (label.Contains("退出") || label.Contains("Exit") || label.Contains("Quit"))) { exitButton = b; break; } }
        }
        if (buttonsPanel == null)
        {
            var commonParent = newGameButton != null ? newGameButton.transform.parent : (continueButton != null ? continueButton.transform.parent : (settingsButton != null ? settingsButton.transform.parent : transform));
            buttonsPanel = commonParent as RectTransform;
        }
    }

    void RefreshButtonsBySaveState()
    {
        bool hasSave = SaveManager.TryLoadMapProgress(out var _, out var _);
        if (newGameButton != null) newGameButton.gameObject.SetActive(true);
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        if (abandonButton != null) abandonButton.gameObject.SetActive(true);
        if (settingsButton != null) settingsButton.gameObject.SetActive(true);
        if (exitButton != null) exitButton.gameObject.SetActive(true);

        if (hasSave)
        {
            if (newGameButton != null) newGameButton.interactable = false;
            if (continueButton != null) { continueButton.interactable = true; }
            if (abandonButton != null) { abandonButton.interactable = true; }
        }
        else
        {
            if (newGameButton != null) newGameButton.interactable = true;
            if (continueButton != null) { continueButton.interactable = false; }
            if (abandonButton != null) { abandonButton.interactable = false; }
        }
    }

    void OnContinue()
    {
        SceneFlowManager.Instance?.LoadScene(SceneFlowManager.SceneType.Map);
    }

    void OnNewGame()
    {
        SaveManager.ClearMapProgress();
        SceneFlowManager.Instance?.LoadScene(SceneFlowManager.SceneType.Map);
    }

    void OnAbandon()
    {
        // 放弃当前游戏：清空存档并刷新按钮状态
        SaveManager.ClearMapProgress();
        RefreshButtonsBySaveState();
        Debug.Log("[MainMenu] Current game abandoned.");
    }

    void OnSettings()
    {
        if (settingsPanel != null) { settingsPanel.SetActive(true); return; }
        var canvas = GetComponentInParent<Canvas>();
        var panel = canvas != null ? canvas.transform.Find("SettingsPanel") : transform.Find("SettingsPanel");
        if (panel != null) { settingsPanel = panel.gameObject; settingsPanel.SetActive(true); return; }
        var go = new GameObject("SettingsPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(canvas != null ? canvas.transform : transform, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(640f, 360f);
        var img = go.GetComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        var titleGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = titleGo.GetComponent<RectTransform>();
        trt.SetParent(go.transform, false);
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(16f, 16f);
        trt.offsetMax = new Vector2(-16f, -16f);
        var txt = titleGo.GetComponent<TextMeshProUGUI>();
        txt.text = "设置界面开发中";
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontSize = 28;
        txt.color = Color.white;
        settingsPanel = go;
        settingsPanel.SetActive(true);
    }

    void EnsureSettingsPanel()
    {
        if (settingsPanel != null) return;

        var canvas = GetComponentInParent<Canvas>();
        var parent = canvas != null ? canvas.transform : transform;

        // 全屏半透明遮罩
        settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        var rt = settingsPanel.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var bg = settingsPanel.GetComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0f, 0f, 0f, 0.5f);
        settingsPanel.SetActive(false);

        // 中间内容面板
        var content = new GameObject("Content", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.VerticalLayoutGroup));
        var crt = content.GetComponent<RectTransform>();
        crt.SetParent(settingsPanel.transform, false);
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(480f, 360f);
        var cimg = content.GetComponent<UnityEngine.UI.Image>();
        cimg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        var vlg = content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 16, 16);
        vlg.spacing = 12f;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        // 标题
        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = titleGo.GetComponent<RectTransform>();
        trt.SetParent(content.transform, false);
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(8f, 8f);
        trt.offsetMax = new Vector2(-8f, -8f);
        var title = titleGo.GetComponent<TextMeshProUGUI>();
        title.text = LanguageManager.Tr("settings");
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 28;
        title.color = Color.white;
        var uiFont = LanguageManager.GetTMPFont(28);
        if (uiFont != null) title.font = uiFont;

        // 语言切换行
        var langRow = new GameObject("LanguageRow", typeof(RectTransform), typeof(UnityEngine.UI.HorizontalLayoutGroup));
        var lrt = langRow.GetComponent<RectTransform>();
        lrt.SetParent(content.transform, false);
        lrt.sizeDelta = new Vector2(448f, 48f);
        var hl = langRow.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        hl.spacing = 12f; hl.childControlWidth = true; hl.childForceExpandWidth = true; hl.childControlHeight = true; hl.childForceExpandHeight = false;

        CreateLangButton(langRow.transform, "中文", () => { LanguageManager.SetLanguage(Language.Zh); RefreshSettingsTexts(); });
        CreateLangButton(langRow.transform, "English", () => { LanguageManager.SetLanguage(Language.En); RefreshSettingsTexts(); });

        // 返回按钮
        var btnGo = new GameObject("Back_Button", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        var brt = btnGo.GetComponent<RectTransform>();
        brt.SetParent(content.transform, false);
        brt.sizeDelta = new Vector2(448f, 48f);
        var bimg = btnGo.GetComponent<UnityEngine.UI.Image>();
        bimg.color = new Color(1f, 1f, 1f, 0.1f);
        var btn = btnGo.GetComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = bimg;
        btn.onClick.AddListener(CloseSettings);
        var btextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var btrt = btextGo.GetComponent<RectTransform>();
        btrt.SetParent(btnGo.transform, false);
        btrt.anchorMin = new Vector2(0f, 0f);
        btrt.anchorMax = new Vector2(1f, 1f);
        btrt.offsetMin = new Vector2(12f, 8f);
        btrt.offsetMax = new Vector2(-12f, -8f);
        var btext = btextGo.GetComponent<TextMeshProUGUI>();
        btext.text = LanguageManager.Tr("back");
        btext.alignment = TextAlignmentOptions.Center;
        btext.fontSize = 24;
        btext.color = Color.white;
        var bFont = LanguageManager.GetTMPFont(24);
        if (bFont != null) btext.font = bFont;
    }

    void CreateLangButton(Transform parent, string label, System.Action onClick)
    {
        var go = new GameObject(label + "_Lang", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(220f, 40f);
        var img = go.GetComponent<UnityEngine.UI.Image>();
        img.color = new Color(1f, 1f, 1f, 0.12f);
        var btn = go.GetComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick?.Invoke());
        var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = tgo.GetComponent<RectTransform>();
        trt.SetParent(go.transform, false);
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(12f, 8f);
        trt.offsetMax = new Vector2(-12f, -8f);
        var text = tgo.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 20;
        text.color = Color.white;
        var f = LanguageManager.GetTMPFont(20);
        if (f != null) text.font = f;
    }

    void RefreshSettingsTexts()
    {
        if (settingsPanel == null) return;
        var content = settingsPanel.transform.Find("Content");
        var title = content?.Find("Title")?.GetComponent<TextMeshProUGUI>();
        var backText = content?.Find("Back_Button/Text")?.GetComponent<TextMeshProUGUI>();
        if (title != null)
        {
            title.text = LanguageManager.Tr("settings");
            var f = LanguageManager.GetTMPFont(28);
            if (f != null) title.font = f;
        }
        if (backText != null)
        {
            backText.text = LanguageManager.Tr("back");
            var f = LanguageManager.GetTMPFont(24);
            if (f != null) backText.font = f;
        }
    }

    void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void OnExit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void ShowExitModal()
    {
        EnsureExitModal();
        if (exitModal != null) exitModal.SetActive(true);
    }

    void EnsureMaskOverlay()
    {
        var canvas = GetComponentInParent<Canvas>();
        var parent = canvas != null ? canvas.transform : transform;
        var root = new GameObject("MenuOverlayRoot", typeof(RectTransform));
        var rrt = root.GetComponent<RectTransform>();
        rrt.SetParent(parent, false);
        rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
        rrt.offsetMin = Vector2.zero; rrt.offsetMax = Vector2.zero;

        var gradGo = new GameObject("LeftGradient", typeof(RectTransform), typeof(GradientGraphic));
        var grt = gradGo.GetComponent<RectTransform>();
        grt.SetParent(root.transform, false);
        grt.anchorMin = new Vector2(0f, 0f);
        grt.anchorMax = new Vector2(0.6f, 1f);
        grt.offsetMin = Vector2.zero;
        grt.offsetMax = Vector2.zero;
        var gg = gradGo.GetComponent<GradientGraphic>();
        gg.orientation = GradientGraphic.Orientation.Horizontal;
        gg.startColor = new Color(0f, 0f, 0f, 0.8f);
        gg.endColor = new Color(0f, 0f, 0f, 0f);
        gg.raycastTarget = false;

        var wedgeGo = new GameObject("DiagonalTint", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        var wrt = wedgeGo.GetComponent<RectTransform>();
        wrt.SetParent(root.transform, false);
        wrt.anchorMin = new Vector2(0f, 0f);
        wrt.anchorMax = new Vector2(0f, 1f);
        wrt.pivot = new Vector2(0f, 0.5f);
        wrt.sizeDelta = new Vector2(640f, 0f);
        wrt.anchoredPosition = new Vector2(220f, 0f);
        wrt.localRotation = Quaternion.Euler(0f, 0f, -14f);
        var wimg = wedgeGo.GetComponent<UnityEngine.UI.Image>();
        wimg.color = new Color(0.6f, 0.75f, 1f, 0.18f);
        wimg.raycastTarget = false;

        root.transform.SetAsLastSibling();
    }

    void EnsureExitModal()
    {
        if (exitModal != null) return;
        var canvas = GetComponentInParent<Canvas>();
        var parent = canvas != null ? canvas.transform : transform;

        exitModal = new GameObject("ExitModal", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        var rt = exitModal.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var bg = exitModal.GetComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0f, 0f, 0f, 0.6f);
        bg.raycastTarget = true;
        exitModal.SetActive(false);

        var content = new GameObject("Content", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.VerticalLayoutGroup));
        var crt = content.GetComponent<RectTransform>();
        crt.SetParent(exitModal.transform, false);
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(420f, 220f);
        var cimg = content.GetComponent<UnityEngine.UI.Image>();
        cimg.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        var vlg = content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 16, 16);
        vlg.spacing = 12f;
        vlg.childControlHeight = true; vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = titleGo.GetComponent<RectTransform>();
        trt.SetParent(content.transform, false);
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(8f, 8f);
        trt.offsetMax = new Vector2(-8f, -8f);
        var title = titleGo.GetComponent<TextMeshProUGUI>();
        title.text = "确定退出游戏？";
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 26;
        title.color = Color.white;

        var row = new GameObject("Buttons", typeof(RectTransform), typeof(UnityEngine.UI.HorizontalLayoutGroup));
        var rrt = row.GetComponent<RectTransform>();
        rrt.SetParent(content.transform, false);
        rrt.sizeDelta = new Vector2(388f, 48f);
        var hl = row.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        hl.spacing = 12f; hl.childControlWidth = true; hl.childForceExpandWidth = true; hl.childControlHeight = true; hl.childForceExpandHeight = false;

        CreateModalButton(row.transform, "取消", () => { exitModal.SetActive(false); });
        CreateModalButton(row.transform, "退出", () => {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
    }

    void CreateModalButton(Transform parent, string label, System.Action onClick)
    {
        var go = new GameObject(label + "_Btn", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(180f, 44f);
        var img = go.GetComponent<UnityEngine.UI.Image>();
        img.color = new Color(1f, 1f, 1f, 0.12f);
        var btn = go.GetComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick?.Invoke());
        var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = tgo.GetComponent<RectTransform>();
        trt.SetParent(go.transform, false);
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(12f, 8f);
        trt.offsetMax = new Vector2(-12f, -8f);
        var text = tgo.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 22;
        text.color = Color.white;
    }
}
