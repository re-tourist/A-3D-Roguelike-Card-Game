using UnityEngine;
using UnityEngine.EventSystems;

public class RuntimeDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public RectTransform target;
    public bool rememberPosition;
    public string prefsKey;
    Vector2 startPos;
    Vector2 pointerStart;

    void Awake()
    {
        if (rememberPosition && PlayerPrefs.HasKey(prefsKey))
        {
            var s = PlayerPrefs.GetString(prefsKey);
            var p = JsonUtility.FromJson<P>(s);
            target.anchoredPosition = new Vector2(p.x, p.y);
        }
    }

    public void OnBeginDrag(PointerEventData e)
    {
        startPos = target.anchoredPosition;
        RectTransform parent = target.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, e.position, e.pressEventCamera, out pointerStart);
    }

    public void OnDrag(PointerEventData e)
    {
        RectTransform parent = target.parent as RectTransform;
        Vector2 currentLocal;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, e.position, e.pressEventCamera, out currentLocal))
        {
            var delta = currentLocal - pointerStart;
            target.anchoredPosition = startPos + delta;
        }
    }

    void OnDisable()
    {
        if (rememberPosition)
        {
            var p = new P { x = target.anchoredPosition.x, y = target.anchoredPosition.y };
            PlayerPrefs.SetString(prefsKey, JsonUtility.ToJson(p));
            PlayerPrefs.Save();
        }
    }

    struct P { public float x; public float y; }
}