using UnityEngine;

/// <summary>
/// ��Ϸ���أ�����״̬������ʼ��������ȫ�����á�
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// ��Ϸ״̬ö��
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
        // ��ʼ����������ϵͳ���ɷ��ڴ˴���
        if (FindObjectOfType<SceneFlowManager>() == null)
            gameObject.AddComponent<SceneFlowManager>();

        if (FindObjectOfType<UIManager>() == null)
            gameObject.AddComponent<UIManager>();

        if (FindObjectOfType<AudioManager>() == null)
            gameObject.AddComponent<AudioManager>();

        // ��ʼ����ɺ󣬼������˵�
        SetState(GameState.MainMenu);
        SceneFlowManager.Instance.LoadScene(SceneFlowManager.SceneType.MainMenu);
    }

    /// <summary>
    /// �޸���Ϸ״̬���㲥�¼�
    /// </summary>
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        EventBus.Publish("OnGameStateChanged", newState);
        Debug.Log($"[GameManager] State changed to {newState}");
    }
}
