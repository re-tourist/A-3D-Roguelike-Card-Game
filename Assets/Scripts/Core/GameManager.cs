using UnityEngine;
using UnityEngine.UI;

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
        if (FindObjectOfType<SceneFlowManager>() == null)
            gameObject.AddComponent<SceneFlowManager>();

        if (FindObjectOfType<UIManager>() == null)
            gameObject.AddComponent<UIManager>();

        if (FindObjectOfType<AudioManager>() == null)
            gameObject.AddComponent<AudioManager>();

        StartCoroutine(BootSequence());
    }

    private System.Collections.IEnumerator BootSequence()
    {
        while (SceneFlowManager.Instance == null || UIManager.Instance == null || AudioManager.Instance == null)
            yield return null;
        yield return null;
        SetState(GameState.MainMenu);
        SceneFlowManager.Instance.LoadScene(SceneFlowManager.SceneType.MainMenu);
        EventBus.Subscribe("OnSceneLoaded", OnSceneLoaded);
    }

    private void OnSceneLoaded(object payload)
    {
        if (payload is SceneFlowManager.SceneType type && type == SceneFlowManager.SceneType.MainMenu)
        {
            EnsureMainMenuUI();
        }
    }

    private void EnsureMainMenuUI()
    {
        var controller = FindObjectOfType<MainMenuController>();
        if (controller == null) return;
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
