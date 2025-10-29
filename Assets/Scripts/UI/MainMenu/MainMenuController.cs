using UnityEngine;
using UnityEngine.UI;
using Game.Core;

public class MainMenuController : MonoBehaviour
{
    [Header("按钮容器（可选）")]
    public RectTransform buttonsPanel;

    [Header("按钮引用")]
    public Button continueButton;
    public Button newGameButton;
    public Button abandonButton;
    public Button menuButton;
    public Button exitButton;

    [Header("行为配置")]
    public bool hideContinueWhenNoSave = true;
    public bool hideAbandonWhenNoSave = false;

    void Start()
    {
        WireButtons();
        RefreshButtonsBySaveState();
    }

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
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMenu);
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

    void OnMenu()
    {
        // 菜单：当前为主菜单场景，允许重新加载或打开设置面板
        // 如有设置场景，可替换为对应跳转
        Debug.Log("[MainMenu] Open menu/settings.");
        // 例如：SceneFlowManager.Instance?.LoadScene(SceneFlowManager.SceneType.MainMenu);
    }

    void OnExit()
    {
        Debug.Log("[MainMenu] Exit requested.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}