using UnityEngine;
using Game.Core;

public class DevControlPanel : MonoBehaviour
{
    public bool disableSaves = false;
    public bool forceMainMenu = false;
    public bool forceMap = false;

    void Awake()
    {
        DevFlags.DisableSaves = disableSaves;
        if (forceMainMenu && SceneFlowManager.Instance != null)
            SceneFlowManager.Instance.LoadScene(SceneFlowManager.SceneType.MainMenu);
        if (forceMap && SceneFlowManager.Instance != null)
            SceneFlowManager.Instance.LoadScene(SceneFlowManager.SceneType.Map);
    }
}
