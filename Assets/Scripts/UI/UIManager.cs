using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 全局 UI 管理器
/// 控制 Canvas 层面上的面板加载与切换。
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private readonly Dictionary<string, GameObject> panels = new();

    [SerializeField] private Canvas mainCanvas;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[UIManager] Initialized.");

        EventBus.Subscribe("OnGameStateChanged", OnGameStateChanged);
    }

    private void Start()
    {
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
                Debug.LogWarning("[UIManager] Canvas not found!");
        }
    }

    private void OnGameStateChanged(object state)
    {
        if (state is GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.MainMenu:
                    ShowPanel("MainMenuPanel");
                    break;
                case GameManager.GameState.Map:
                    ShowPanel("MapPanel");
                    break;
                case GameManager.GameState.Battle:
                    ShowPanel("BattlePanel");
                    break;
            }
        }
    }

    /// <summary>
    /// 显示目标面板
    /// </summary>
    public void ShowPanel(string panelName)
    {
        foreach (var kv in panels)
            kv.Value.SetActive(kv.Key == panelName);
        Debug.Log($"[UIManager] Switched to {panelName}");
    }

    /// <summary>
    /// 注册UI面板（可在加载Prefab时调用）
    /// </summary>
    public void RegisterPanel(string name, GameObject panel)
    {
        if (!panels.ContainsKey(name))
            panels.Add(name, panel);
    }
}
