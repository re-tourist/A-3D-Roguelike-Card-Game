using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ȫ�� UI ������
/// ���� Canvas �����ϵ����������л���
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
    /// ��ʾĿ�����
    /// </summary>
    public void ShowPanel(string panelName)
    {
        foreach (var kv in panels)
            kv.Value.SetActive(kv.Key == panelName);
        Debug.Log($"[UIManager] Switched to {panelName}");
    }

    /// <summary>
    /// ע��UI��壨���ڼ���Prefabʱ���ã�
    /// </summary>
    public void RegisterPanel(string name, GameObject panel)
    {
        if (!panels.ContainsKey(name))
            panels.Add(name, panel);
    }
}
