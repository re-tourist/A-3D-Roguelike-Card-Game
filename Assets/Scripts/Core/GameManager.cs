using UnityEngine;

/// <summary>
/// 游戏主控：负责状态机、初始化流程与全局引用。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState { Boot, MainMenu, Map, Battle, Reward, GameOver }

    public GameState CurrentState { get; private set; } = GameState.Boot;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[GameManager] Initialized.");

        InitializeSystems();
    }

    private void InitializeSystems()
    {
        // 初始化其他核心系统（可放在此处）
        if (FindObjectOfType<SceneFlowManager>() == null)
            gameObject.AddComponent<SceneFlowManager>();

        if (FindObjectOfType<UIManager>() == null)
            gameObject.AddComponent<UIManager>();

        if (FindObjectOfType<AudioManager>() == null)
            gameObject.AddComponent<AudioManager>();

        // 初始化完成后，加载主菜单
        SetState(GameState.MainMenu);
        SceneFlowManager.Instance.LoadScene(SceneFlowManager.SceneType.MainMenu);
    }

    /// <summary>
    /// 修改游戏状态并广播事件
    /// </summary>
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        EventBus.Publish("OnGameStateChanged", newState);
        Debug.Log($"[GameManager] State changed to {newState}");
    }
}
