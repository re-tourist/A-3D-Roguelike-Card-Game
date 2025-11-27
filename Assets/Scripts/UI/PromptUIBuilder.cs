using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PromptUIBuilder : MonoBehaviour
{
    [TextArea] public string prompt;
    public bool buildOnStart = true;
    public TextAsset designJson;
    public TMP_FontAsset fontAsset;

    [Serializable]
    public class Spec
    {
        public CanvasSpec canvas;
        public List<Element> layers;
    }
    [Serializable]
    public class CanvasSpec { public int width; public int height; }
    [Serializable]
    public class Element
    {
        public string type;
        public float[] anchorMin;
        public float[] anchorMax;
        public float[] pivot;
        public float[] size;
        public float[] position;
        public float angle;
        public string color;
        public string text;
        public int fontSize;
        public string align;
        public string[] items;
    }

    void Start()
    {
        if (buildOnStart) Build();
    }

    public void Build()
    {
        var spec = LoadSpec();
        if (spec == null) return;
        var canvas = GetComponentInParent<Canvas>();
        var parent = canvas != null ? canvas.transform : transform;
        var root = new GameObject("PromptUI", typeof(RectTransform));
        var rrt = root.GetComponent<RectTransform>();
        rrt.SetParent(parent, false);
        rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
        rrt.offsetMin = Vector2.zero; rrt.offsetMax = Vector2.zero;
        foreach (var e in spec.layers)
        {
            if (e.type == "gradient")
            {
                var go = new GameObject("Gradient", typeof(RectTransform), typeof(GradientGraphic));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(root.transform, false);
                ApplyRect(rt, e);
                var g = go.GetComponent<GradientGraphic>();
                g.orientation = GradientGraphic.Orientation.Horizontal;
                var cA = ParseColor(e.color, new Color(0f,0f,0f,0.8f));
                var cB = new Color(cA.r, cA.g, cA.b, 0f);
                g.SetColors(cA, cB);
                g.raycastTarget = false;
            }
            else if (e.type == "wedge")
            {
                var go = new GameObject("Wedge", typeof(RectTransform), typeof(Image));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(root.transform, false);
                ApplyRect(rt, e);
                rt.localRotation = Quaternion.Euler(0f,0f,e.angle);
                var img = go.GetComponent<Image>();
                img.color = ParseColor(e.color, new Color(0.6f,0.75f,1f,0.18f));
                img.raycastTarget = false;
            }
            else if (e.type == "text")
            {
                var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(root.transform, false);
                ApplyRect(rt, e);
                var t = go.GetComponent<TextMeshProUGUI>();
                t.text = e.text;
                t.fontSize = e.fontSize > 0 ? e.fontSize : 64;
                t.color = Color.white;
                t.alignment = ResolveAlign(e.align);
                if (fontAsset != null) t.font = fontAsset;
            }
            else if (e.type == "buttonList")
            {
                var panel = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
                var rt = panel.GetComponent<RectTransform>();
                rt.SetParent(root.transform, false);
                ApplyRect(rt, e);
                var v = panel.GetComponent<VerticalLayoutGroup>();
                v.padding = new RectOffset(12,12,12,12);
                v.spacing = 16f;
                v.childControlHeight = true; v.childControlWidth = true;
                v.childForceExpandHeight = false; v.childForceExpandWidth = true;
                if (e.items != null)
                {
                    foreach (var label in e.items)
                    {
                        var b = new GameObject(label+"_Button", typeof(RectTransform), typeof(Image), typeof(Button));
                        var brt = b.GetComponent<RectTransform>();
                        brt.SetParent(panel.transform, false);
                        brt.sizeDelta = new Vector2(420f, 56f);
                        var img = b.GetComponent<Image>(); img.color = new Color(1f,1f,1f,0.12f);
                        var le = b.AddComponent<LayoutElement>(); le.minHeight = 56f; le.preferredHeight = 56f; le.flexibleWidth = 1f;
                        var tg = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                        var trt = tg.GetComponent<RectTransform>(); trt.SetParent(b.transform, false);
                        trt.anchorMin = new Vector2(0f,0f); trt.anchorMax = new Vector2(1f,1f); trt.offsetMin = new Vector2(12f,8f); trt.offsetMax = new Vector2(-12f,-8f);
                        var txt = tg.GetComponent<TextMeshProUGUI>(); txt.text = label; txt.color = Color.white; txt.fontSize = 28f; txt.alignment = TextAlignmentOptions.Center; if (fontAsset!=null) txt.font = fontAsset;
                    }
                }
            }
        }
        root.transform.SetAsLastSibling();
    }

    Spec LoadSpec()
    {
        if (designJson != null && !string.IsNullOrEmpty(designJson.text))
        {
            return JsonUtility.FromJson<Spec>(designJson.text);
        }
        var s = new Spec();
        s.canvas = new CanvasSpec{width=1920,height=1080};
        s.layers = new List<Element>();
        s.layers.Add(new Element{type="gradient",anchorMin=new[]{0f,0f},anchorMax=new[]{0.55f,1f},color="#000000CC"});
        s.layers.Add(new Element{type="wedge",anchorMin=new[]{0f,0.5f},anchorMax=new[]{0f,0.5f},pivot=new[]{0f,0.5f},size=new[]{640f,0f},position=new[]{240f,0f},angle=-18f,color="#99BBFF2E"});
        s.layers.Add(new Element{type="text",anchorMin=new[]{0f,1f},anchorMax=new[]{0f,1f},pivot=new[]{0f,1f},size=new[]{1000f,120f},position=new[]{140f,-140f},text="万象骗局：欺诈终焉",fontSize=72,align="left"});
        s.layers.Add(new Element{type="buttonList",anchorMin=new[]{0f,0.5f},anchorMax=new[]{0f,0.5f},pivot=new[]{0f,0.5f},size=new[]{460f,0f},position=new[]{160f,-10f},items=new[]{"开始游戏","设置","退出"}});
        return s;
    }

    void ApplyRect(RectTransform rt, Element e)
    {
        if (e.anchorMin != null && e.anchorMin.Length==2) rt.anchorMin = new Vector2(e.anchorMin[0], e.anchorMin[1]);
        if (e.anchorMax != null && e.anchorMax.Length==2) rt.anchorMax = new Vector2(e.anchorMax[0], e.anchorMax[1]);
        if (e.pivot != null && e.pivot.Length==2) rt.pivot = new Vector2(e.pivot[0], e.pivot[1]);
        if (e.size != null && e.size.Length==2) rt.sizeDelta = new Vector2(e.size[0], e.size[1]);
        if (e.position != null && e.position.Length==2) rt.anchoredPosition = new Vector2(e.position[0], e.position[1]);
    }

    Color ParseColor(string hex, Color fallback)
    {
        if (string.IsNullOrEmpty(hex)) return fallback;
        if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
        return fallback;
    }

    TextAlignmentOptions ResolveAlign(string a)
    {
        if (string.IsNullOrEmpty(a)) return TextAlignmentOptions.Center;
        a = a.ToLowerInvariant();
        if (a=="left") return TextAlignmentOptions.MidlineLeft;
        if (a=="right") return TextAlignmentOptions.MidlineRight;
        return TextAlignmentOptions.Center;
    }
}
