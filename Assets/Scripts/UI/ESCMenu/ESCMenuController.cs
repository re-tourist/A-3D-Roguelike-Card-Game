using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Localization;

public class ESCMenuController : MonoBehaviour
{
    [SerializeField] private ESCMenuAnimator animator;
    [SerializeField] private CanvasGroup rootCanvasGroup;

    private bool isOpen = false;

    void Awake()
    {
        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = 1f; // 容器保持可见，交互由 blocksRaycasts 控制
            rootCanvasGroup.blocksRaycasts = false;
            rootCanvasGroup.interactable = false;
        }
    }

    public void Toggle()
    {
        if (isOpen) Hide();
        else Show();
    }

    public void Show()
    {
        isOpen = true;
        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.blocksRaycasts = true;
            rootCanvasGroup.interactable = true;
        }
        if (animator != null && animator.menuButtons != null)
        {
            var root = animator.menuButtons.transform;
            var resume = root.Find("ResumeButton/Text")?.GetComponent<TextMeshProUGUI>();
            var settings = root.Find("SettingsButton/Text")?.GetComponent<TextMeshProUGUI>();
            var quit = root.Find("QuitButton/Text")?.GetComponent<TextMeshProUGUI>();
            var font = LanguageManager.GetTMPFont(28);
            if (resume != null) { resume.text = LanguageManager.Tr("resume"); if (font != null) resume.font = font; }
            if (settings != null) { settings.text = LanguageManager.Tr("settings"); if (font != null) settings.font = font; }
            if (quit != null) { quit.text = LanguageManager.Tr("back_to_menu"); if (font != null) quit.font = font; }
        }
        animator?.PlayOpen();
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        isOpen = false;
        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.blocksRaycasts = false;
            rootCanvasGroup.interactable = false;
        }
        animator?.PlayClose();
        Time.timeScale = 1f;
    }

    // 按钮绑定
    public void OnResume() => Hide();

    public void OnSettings()
    {
        Debug.Log("[ESCMenu] Settings menu not implemented.");
        // 可扩展：UIManager.Instance.PushPanel("Settings");
    }

    public void OnQuit()
    {
        Time.timeScale = 1f;
        SceneFlowManager.Instance?.LoadScene(SceneFlowManager.SceneType.MainMenu);
    }
}