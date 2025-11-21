using UnityEngine;
using UnityEngine.UI;

namespace Game.Map
{
    public class ReachablePulse : MonoBehaviour
    {
        public bool enabledPulse = false;
        public float verticalAmplitude = 2f;
        public float scaleAmplitude = 0.08f;
        public float speed = 2.2f;
        public float phaseOffset = 0.0f;
        Vector2 basePos;
        Vector3 baseScale = Vector3.one;
        RectTransform rt;
        void Awake() { rt = GetComponent<RectTransform>(); if (rt != null) { basePos = rt.anchoredPosition; baseScale = rt.localScale; } }
        void OnEnable() { if (rt != null) { basePos = rt.anchoredPosition; baseScale = rt.localScale; } }
        void Update()
        {
            if (!enabledPulse || rt == null) return;
            float t = Time.time * speed + phaseOffset;
            float y = Mathf.Sin(t) * verticalAmplitude;
            float s = 1f + Mathf.Sin(t) * scaleAmplitude;
            s = Mathf.Round(s * 100f) / 100f;
            float yq = Mathf.Round(basePos.y + y);
            rt.anchoredPosition = new Vector2(basePos.x, yq);
            rt.localScale = baseScale * s;
        }
    }
}
