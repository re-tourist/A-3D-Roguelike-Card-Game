using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 控制场景加载/卸载（Additive 模式）
/// 保证 BootstrapScene 常驻，其它场景动态切换。
/// </summary>
public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance { get; private set; }

    public enum SceneType { MainMenu, Map, Battle, Reward }

    private string currentScene = string.Empty;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[SceneFlowManager] Initialized.");
    }

    /// <summary>
    /// 异步加载指定场景（Additive 模式）
    /// </summary>
    public void LoadScene(SceneType type, object context = null)
    {
        StartCoroutine(LoadSceneAsync(type, context));
    }

    private IEnumerator LoadSceneAsync(SceneType type, object context)
    {
        string targetScene = GetSceneName(type);
        Debug.Log($"[SceneFlowManager] Loading scene: {targetScene}");

        if (!string.IsNullOrEmpty(currentScene))
        {
            yield return SceneManager.UnloadSceneAsync(currentScene);
            Debug.Log($"[SceneFlowManager] Unloaded: {currentScene}");
        }

        yield return SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        currentScene = targetScene;

        EventBus.Publish("OnSceneLoaded", type);
        Debug.Log($"[SceneFlowManager] Loaded: {targetScene}");
    }

    private string GetSceneName(SceneType type) => type switch
    {
        SceneType.MainMenu => "00_MainMenu",
        SceneType.Map => "01_MapScene",
        SceneType.Battle => "02_BattleScene",
        SceneType.Reward => "03_RewardScene",
        _ => "99_DebugScene"
    };
}
