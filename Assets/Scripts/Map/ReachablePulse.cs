using UnityEngine;
using UnityEngine.UI;

namespace Game.Map
{
    public class ReachablePulse : MonoBehaviour
    {
        public bool enabledPulse = false;
        public float minScale = 1f;
        public float maxScale = 1.25f;
        public float speed = 2.0f;
        public float phaseOffset = 0.0f;
        RectTransform rt;
        void Awake() { rt = GetComponent<RectTransform>(); }
        void OnEnable() { rt = GetComponent<RectTransform>(); }
        void Update()
        {
            if (!enabledPulse || rt == null) return;
            float t = Time.time * speed + phaseOffset;
            float k = 0.5f * (1f + Mathf.Sin(t));
            float s = Mathf.Lerp(minScale, maxScale, k);
            rt.localScale = new Vector3(s, s, 1f);
        }
    }
}
