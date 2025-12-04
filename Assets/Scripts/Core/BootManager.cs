using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        if (GameManager.Instance == null)
        {
            new GameObject("App", typeof(GameManager));
            Debug.Log("[BootManager] Created GameManager.");
        }

        if (FindObjectOfType<DevControlPanel>() == null)
        {
            var dev = new GameObject("DevPanel", typeof(DevControlPanel));
            Debug.Log("[BootManager] DevPanel created.");
        }

        if (FindObjectOfType<BattleManager>() == null)
        {
            new GameObject("BattleManager", typeof(BattleManager));
        }

        yield return null;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        var s = SceneManager.GetActiveScene().name;
        if (!Application.isPlaying && s == "BootSence")
        {
            var dev = FindObjectOfType<DevControlPanel>();
            if (dev == null)
            {
                var go = GameObject.Find("DevPanel") ?? new GameObject("DevPanel");
                if (go.GetComponent<DevControlPanel>() == null) go.AddComponent<DevControlPanel>();
            }
        }
    }
#endif
}
