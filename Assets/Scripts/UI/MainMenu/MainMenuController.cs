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
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinue);
        }
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnNewGame);
        }
        if (abandonButton != null)
        {
            abandonButton.onClick.RemoveAllListeners();
            abandonButton.onClick.AddListener(OnAbandon);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettings);
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExit);
        }
    }

    void RefreshButtonsBySaveState()
    {
        bool hasSave = SaveManager.TryLoadMapProgress(out var _, out var _);

        if (continueButton != null)
        {
            continueButton.interactable = hasSave;
            if (hideContinueWhenNoSave) continueButton.gameObject.SetActive(hasSave);
        }
        if (abandonButton != null)
        {
            abandonButton.interactable = hasSave;
            if (hideAbandonWhenNoSave) abandonButton.gameObject.SetActive(hasSave);
        }
        if (newGameButton != null)
        {
            newGameButton.gameObject.SetActive(!hasSave);
        }
    }

    void OnContinue()
    {
        // 继续游戏：进入地图场景，自动从存档恢复
        SceneFlowManager.Instance?.LoadScene(SceneFlowManager.SceneType.Map);
    }

    void OnNewGame()
    {
        // 新游戏：清空存档并进入地图场景
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
        Debug.Log("[MainMenu] Open settings panel.");
        EnsureSettingsPanel();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
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
        ShowExitModal();
    }

    void ShowExitModal()
    {
        EnsureExitModal();
        if (exitModal != null) exitModal.SetActive(true);
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
