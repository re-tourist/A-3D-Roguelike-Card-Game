using UnityEngine;

public class ESCMenuInput : MonoBehaviour
{
    [SerializeField] private ESCMenuController escMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escMenu?.Toggle();
        }
    }
}