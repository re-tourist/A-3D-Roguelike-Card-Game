using UnityEngine;

public class ESCMenuAnimator : MonoBehaviour
{
    public RectTransform leftRift;
    public RectTransform rightRift;
    public CanvasGroup riftGroup;
    public CanvasGroup darkMask;
    public CanvasGroup menuButtons;

    // 可选：Glow 呼吸效果
    public RectTransform riftGlow;
    public CanvasGroup riftGlowGroup;

    private const float splitDistance = 320f;
    private const float duration = 0.45f;

    private Coroutine coLeft;
    private Coroutine coRight;
    private Coroutine coRiftFade;
    private Coroutine coDarkFade;
    private Coroutine coMenuFade;
    private Coroutine coGlowAlpha;
    private Coroutine coGlowScale;

    void Awake()
    {
        if (leftRift != null) leftRift.anchoredPosition = new Vector2(0f, leftRift.anchoredPosition.y);
        if (rightRift != null) rightRift.anchoredPosition = new Vector2(0f, rightRift.anchoredPosition.y);
        if (riftGroup != null) riftGroup.alpha = 0f;
        if (menuButtons != null) menuButtons.alpha = 0f;
        if (darkMask != null) darkMask.alpha = 0f;

        // 可选：SetupGlowBreath();
    }

    public void PlayOpen()
    {
        if (darkMask != null)
            StartRoutine(ref coDarkFade, AnimateAlpha(darkMask, 1f, 0.3f, 0f, EaseOutCubic));

        if (riftGroup != null)
        {
            riftGroup.alpha = 0f;
            StartRoutine(ref coRiftFade, AnimateAlpha(riftGroup, 1f, 0.2f, 0f, EaseOutCubic));
        }

        if (leftRift != null)
            StartRoutine(ref coLeft, AnimateAnchorPosX(leftRift, -splitDistance, duration, 0f, EaseOutCubic));
        if (rightRift != null)
            StartRoutine(ref coRight, AnimateAnchorPosX(rightRift, splitDistance, duration, 0f, EaseOutCubic));

        if (menuButtons != null)
            StartRoutine(ref coMenuFade, AnimateAlpha(menuButtons, 1f, 0.25f, duration - 0.1f, EaseOutCubic));
    }

    public void PlayClose()
    {
        if (menuButtons != null)
            StartRoutine(ref coMenuFade, AnimateAlpha(menuButtons, 0f, 0.2f, 0f, EaseInCubic));

        if (leftRift != null)
            StartRoutine(ref coLeft, AnimateAnchorPosX(leftRift, 0f, duration, 0f, EaseInCubic));
        if (rightRift != null)
            StartRoutine(ref coRight, AnimateAnchorPosX(rightRift, 0f, duration, 0f, EaseInCubic));

        if (darkMask != null)
            StartRoutine(ref coDarkFade, AnimateAlpha(darkMask, 0f, 0.3f, 0.1f, EaseInCubic));
        if (riftGroup != null)
            StartRoutine(ref coRiftFade, AnimateAlpha(riftGroup, 0f, 0.3f, 0.2f, EaseInCubic));
    }

    public void SetupGlowBreath()
    {
        if (riftGlowGroup != null)
        {
            riftGlowGroup.alpha = 0.3f;
            StartRoutine(ref coGlowAlpha, LoopAlphaYoyo(riftGlowGroup, 0.3f, 0.4f, 1.2f, EaseInOutSine));
        }
        if (riftGlow != null)
        {
            riftGlow.localScale = Vector3.one;
            StartRoutine(ref coGlowScale, LoopScaleYoyo(riftGlow, 1f, 1.05f, 1.4f, EaseInOutSine));
        }
    }

    private void StartRoutine(ref Coroutine handle, System.Collections.IEnumerator routine)
    {
        if (handle != null) StopCoroutine(handle);
        handle = StartCoroutine(routine);
    }

    private System.Collections.IEnumerator AnimateAlpha(CanvasGroup group, float target, float d, float delay, System.Func<float, float> ease)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        float start = group.alpha;
        float elapsed = 0f;
        while (elapsed < d)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / d);
            float e = ease != null ? ease(t) : t;
            group.alpha = start + (target - start) * e;
            yield return null;
        }
        group.alpha = target;
    }

    private System.Collections.IEnumerator AnimateAnchorPosX(RectTransform rt, float target, float d, float delay, System.Func<float, float> ease)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        float start = rt.anchoredPosition.x;
        float y = rt.anchoredPosition.y;
        float elapsed = 0f;
        while (elapsed < d)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / d);
            float e = ease != null ? ease(t) : t;
            float x = start + (target - start) * e;
            rt.anchoredPosition = new Vector2(x, y);
            yield return null;
        }
        rt.anchoredPosition = new Vector2(target, y);
    }

    private System.Collections.IEnumerator LoopAlphaYoyo(CanvasGroup group, float a, float b, float d, System.Func<float, float> ease)
    {
        while (true)
        {
            yield return AnimateAlpha(group, b, d, 0f, ease);
            yield return AnimateAlpha(group, a, d, 0f, ease);
        }
    }

    private System.Collections.IEnumerator LoopScaleYoyo(RectTransform rt, float a, float b, float d, System.Func<float, float> ease)
    {
        while (true)
        {
            yield return AnimateScale(rt, b, d, 0f, ease);
            yield return AnimateScale(rt, a, d, 0f, ease);
        }
    }

    private System.Collections.IEnumerator AnimateScale(RectTransform rt, float target, float d, float delay, System.Func<float, float> ease)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        float start = rt.localScale.x;
        float elapsed = 0f;
        while (elapsed < d)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / d);
            float e = ease != null ? ease(t) : t;
            float s = start + (target - start) * e;
            rt.localScale = new Vector3(s, s, s);
            yield return null;
        }
        rt.localScale = new Vector3(target, target, target);
    }

    private float EaseOutCubic(float t)
    {
        t -= 1f;
        return t * t * t + 1f;
    }

    private float EaseInCubic(float t)
    {
        return t * t * t;
    }

    private float EaseInOutSine(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
    }
}