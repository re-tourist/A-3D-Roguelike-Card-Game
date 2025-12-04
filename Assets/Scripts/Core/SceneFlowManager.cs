using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// 控制场景加载与卸载（Additive 模式），
/// 保证常驻管理器跨场景切换稳定。
/// </summary>
public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance { get; private set; }
    public object LastContext { get; private set; }
    public SceneType PreviousNonMapSceneType { get; private set; }

    public enum SceneType { MainMenu, Map, Battle, Reward, Shop, Event, Rest, Elite }

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
        EnsureHUD();
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// 异步加载指定场景（Additive 模式）。
    /// </summary>
    public void LoadScene(SceneType type, object context = null)
    {
        StartCoroutine(LoadSceneAsync(type, context));
    }

    private IEnumerator LoadSceneAsync(SceneType type, object context)
    {
        LastContext = context;
        if (type != SceneType.Map) PreviousNonMapSceneType = type;
        string targetScene = GetSceneName(type);
        Debug.Log($"[SceneFlowManager] Loading scene: {targetScene}");

        #if !UNITY_EDITOR
        if (!Application.CanStreamedLevelBeLoaded(targetScene))
        {
            Debug.LogError($"[SceneFlowManager] Scene '{targetScene}' is not in Build Settings or invalid.");
            yield break;
        }
        #endif

        var loadedTarget = SceneManager.GetSceneByName(targetScene);
        if (!loadedTarget.IsValid() || !loadedTarget.isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
            loadedTarget = SceneManager.GetSceneByName(targetScene);
        }

        if (loadedTarget.IsValid())
        {
            SceneManager.SetActiveScene(loadedTarget);
            CleanupEventSystemsInActiveScene();
        }

        if (!string.IsNullOrEmpty(currentScene))
        {
            var s = SceneManager.GetSceneByName(currentScene);
            if (s.IsValid() && s.isLoaded && s.name != targetScene)
            {
                yield return SceneManager.UnloadSceneAsync(currentScene);
                Debug.Log($"[SceneFlowManager] Unloaded: {currentScene}");
            }
        }
        currentScene = targetScene;

        EventBus.Publish("OnSceneLoaded", type);
        Debug.Log($"[SceneFlowManager] Loaded: {targetScene}");
    }

    private string GetSceneName(SceneType type) => type switch
    {
        SceneType.MainMenu => "MainMenuSence",
        SceneType.Map => "MapSence",
        SceneType.Battle => "FightSence",
        SceneType.Reward => "RewardSence",
        SceneType.Shop => "ShopSence",
        SceneType.Event => "EventSence",
        SceneType.Rest => "RestSence",
        SceneType.Elite => "FightSence",
        _ => "99_DebugScene"
    };

    private void EnsureHUD()
    {
        if (FindObjectOfType<Game.UI.HUDController>() == null)
        {
            var go = new GameObject("HUD", typeof(Game.UI.HUDController));
        }
    }

    private void CleanupEventSystemsInActiveScene()
    {
        var active = SceneManager.GetActiveScene();
        var list = GameObject.FindObjectsOfType<EventSystem>();
        for (int i = 0; i < list.Length; i++)
        {
            var es = list[i];
            if (es != null && es.gameObject.scene == active)
            {
                es.gameObject.SetActive(false);
                Debug.Log("[SceneFlowManager] Disabled scene EventSystem in " + active.name);
            }
        }
    }
}
