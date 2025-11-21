using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Map
{
    public class MapRenderer : MonoBehaviour
    {
        public RectTransform targetRoot;
        public TMP_FontAsset fontAsset;
        public Color lineColor = new Color(1f, 1f, 1f, 0.35f);
        public float lineThickness = 3f;
        public Vector2 areaPadding = new Vector2(80f, 80f);
        public LayerMapGenerator generator;
        public bool useIcons = true;
        public float nodeSize = 256f;
        public float bossScale = 1.6f;
        public int curveSamples = 6;
        public float curveAmount = 0.15f; // 相对长度的弯曲幅度
        public bool handDrawn = true;
        public float wiggleAmplitude = 4f;
        public float wiggleFrequency = 1.6f;
        public float verticalScale = 2.5f;
        public bool enableScroll = true;
        public float scrollSensitivity = 30f;
        public Sprite monsterIcon;
        public Sprite eliteIcon;
        public Sprite shopIcon;
        public Sprite eventIcon;
        public Sprite restIcon;
        public Sprite bossIcon;
        public Sprite backgroundSprite;
        public bool backgroundFillScreen = true;
        public Sprite parchmentSprite;
        public Color parchmentTint = new Color(1f, 0.95f, 0.8f, 1f);

        readonly Dictionary<int, Button> nodeButtons = new Dictionary<int, Button>();

        void Start()
        {
            if (generator == null) generator = GetComponent<LayerMapGenerator>();
            if (generator == null) generator = gameObject.AddComponent<LayerMapGenerator>();
            if (targetRoot == null)
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    canvas.pixelPerfect = true;
                    var host = GameObject.Find("MainMenu");
                    Transform parent = canvas.transform;
                    if (host != null) parent = host.transform;
                    if (enableScroll)
                    {
                        var scrollGo = new GameObject("MapScroll", typeof(RectTransform), typeof(ScrollRect));
                        var srt = scrollGo.GetComponent<RectTransform>();
                        srt.SetParent(parent, false);
                        srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
                        srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;
                        var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
                        var vprt = vpGo.GetComponent<RectTransform>();
                        vprt.SetParent(scrollGo.transform, false);
                        vprt.anchorMin = Vector2.zero; vprt.anchorMax = Vector2.one;
                        vprt.offsetMin = Vector2.zero; vprt.offsetMax = Vector2.zero;
                        var contentGo = new GameObject("MapView", typeof(RectTransform));
                        targetRoot = contentGo.GetComponent<RectTransform>();
                        targetRoot.SetParent(vprt, false);
                        targetRoot.anchorMin = new Vector2(0.5f, 0.5f);
                        targetRoot.anchorMax = new Vector2(0.5f, 0.5f);
                        targetRoot.pivot = new Vector2(0.5f, 0.5f);
                        targetRoot.anchoredPosition = Vector2.zero;
                        var sr = scrollGo.GetComponent<ScrollRect>();
                        sr.viewport = vprt;
                        sr.content = targetRoot;
                        sr.vertical = true;
                        sr.horizontal = false;
                        sr.movementType = ScrollRect.MovementType.Clamped;
                        sr.scrollSensitivity = scrollSensitivity;
                        SetupBackground(vprt);
                        SetupParchment(targetRoot);
                    }
                    else
                    {
                        var go = new GameObject("MapView", typeof(RectTransform));
                        targetRoot = go.GetComponent<RectTransform>();
                        targetRoot.SetParent(parent, false);
                        targetRoot.anchorMin = Vector2.zero; targetRoot.anchorMax = Vector2.one;
                        targetRoot.offsetMin = Vector2.zero; targetRoot.offsetMax = Vector2.zero;
                        SetupBackground(parent);
                        SetupParchment(targetRoot);
                    }
                }
            }
            if (generator != null && targetRoot != null)
            {
                var g = generator.Generate();
                var parentRt = targetRoot.parent as RectTransform;
                if (enableScroll && parentRt != null)
                {
                    var vpSize = parentRt.rect.size;
                    targetRoot.sizeDelta = new Vector2(vpSize.x, vpSize.y * Mathf.Max(1f, verticalScale));
                }
                RenderGraph(g);
            }
        }

        void RenderGraph(LayerMapGraph g)
        {
            var size = targetRoot.rect.size;
            for (int i = 0; i < g.edges.Count; i++)
            {
                var e = g.edges[i];
                var a = g.GetNode(e.fromId);
                var b = g.GetNode(e.toId);
                var pa = NormToLocal(a.normPos, size);
                var pb = NormToLocal(b.normPos, size);
                DrawCurvedLine(pa, pb, a.type, b.type);
            }
            for (int i = 0; i < g.nodes.Count; i++)
            {
                var n = g.nodes[i];
                var p = NormToLocal(n.normPos, size);
                var btn = CreateNodeButton(n, p);
                nodeButtons[n.id] = btn;
                var pulse = btn.gameObject.AddComponent<ReachablePulse>();
                pulse.enabledPulse = n.reachable;
            }
        }

        Vector2 NormToLocal(Vector2 norm, Vector2 size)
        {
            float x = areaPadding.x + norm.x * (size.x - areaPadding.x * 2f);
            float y = areaPadding.y + norm.y * (size.y - areaPadding.y * 2f);
            return new Vector2(x, y) - size * 0.5f;
        }

        void DrawLine(Vector2 a, Vector2 b)
        {
            var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(targetRoot, false);
            var img = go.GetComponent<Image>();
            img.color = lineColor;
            var dir = b - a;
            float len = dir.magnitude;
            rt.sizeDelta = new Vector2(len, lineThickness);
            rt.anchoredPosition = (a + b) * 0.5f;
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rt.localRotation = Quaternion.Euler(0f, 0f, ang);
        }

        Button CreateNodeButton(LayerNode n, Vector2 pos)
        {
            var go = new GameObject("Node_" + n.id, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(targetRoot, false);
            rt.sizeDelta = new Vector2(nodeSize, nodeSize);
            rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.color = Color.white * 0.08f;
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            Sprite s = SpriteForType(n.type);
            if (useIcons && s != null)
            {
                var igo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                var irt = igo.GetComponent<RectTransform>();
                irt.SetParent(go.transform, false);
                irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
                irt.offsetMin = new Vector2(6f, 6f); irt.offsetMax = new Vector2(-6f, -6f);
                var iimg = igo.GetComponent<Image>();
                iimg.sprite = s; iimg.preserveAspect = true; iimg.color = Color.white;
                if (n.type == NodeType.Boss)
                {
                    irt.localScale = new Vector3(bossScale, bossScale, 1f);
                }
            }
            else
            {
                var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                var trt = tgo.GetComponent<RectTransform>();
                trt.SetParent(go.transform, false);
                trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                trt.offsetMin = new Vector2(6f, 6f); trt.offsetMax = new Vector2(-6f, -6f);
                var tmp = tgo.GetComponent<TextMeshProUGUI>();
                tmp.text = NodeLabel(n.type);
                tmp.fontSize = 24f;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
                if (fontAsset != null) tmp.font = fontAsset;
                if (n.type == NodeType.Boss)
                {
                    tmp.fontSize *= 3f;
                }
            }
            return btn;
        }

        string NodeLabel(NodeType t)
        {
            if (t == NodeType.Monster) return "M";
            if (t == NodeType.Elite) return "EM";
            if (t == NodeType.Shop) return "S";
            if (t == NodeType.Boss) return "B";
            if (t == NodeType.Event) return "E";
            if (t == NodeType.Rest) return "R";
            return "";
        }

        Sprite SpriteForType(NodeType t)
        {
            if (t == NodeType.Monster) return monsterIcon;
            if (t == NodeType.Elite) return eliteIcon;
            if (t == NodeType.Shop) return shopIcon;
            if (t == NodeType.Event) return eventIcon;
            if (t == NodeType.Rest) return restIcon;
            if (t == NodeType.Boss) return bossIcon;
            return null;
        }

        float RadiusForType(NodeType t)
        {
            float r = nodeSize * 0.5f;
            if (t == NodeType.Boss) r *= bossScale;
            return r;
        }

        void DrawCurvedLine(Vector2 a, Vector2 b, NodeType ta, NodeType tb)
        {
            var dir = b - a;
            float len = dir.magnitude;
            if (len < 1f) return;
            var n = dir / len;
            float ra = RadiusForType(ta);
            float rb = RadiusForType(tb);
            var a2 = a + n * ra;
            var b2 = b - n * rb;

            var mid = (a2 + b2) * 0.5f;
            var perp = new Vector2(-n.y, n.x);
            float bend = len * curveAmount;
            var c = mid + perp * bend;

            Vector2 prev = a2;
            for (int i = 1; i <= curveSamples; i++)
            {
                float t = i / (float)curveSamples;
                // 二次贝塞尔
                Vector2 p = (1 - t) * (1 - t) * a2 + 2 * (1 - t) * t * c + t * t * b2;
                if (handDrawn)
                {
                    float noise = Mathf.PerlinNoise(t * wiggleFrequency, (a.x + b.y) * 0.01f) * 2f - 1f;
                    p += perp * (noise * wiggleAmplitude);
                }
                DrawSegment(prev, p);
                prev = p;
            }
        }

        void DrawSegment(Vector2 a, Vector2 b)
        {
            var go = new GameObject("LineSeg", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(targetRoot, false);
            var img = go.GetComponent<Image>();
            img.color = lineColor;
            var dir = b - a;
            float len = dir.magnitude;
            if (len < 0.5f) { Destroy(go); return; }
            rt.sizeDelta = new Vector2(len, lineThickness);
            rt.anchoredPosition = (a + b) * 0.5f;
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rt.localRotation = Quaternion.Euler(0f, 0f, ang);
        }

        void SetupBackground(Transform parent)
        {
            if (backgroundSprite == null) return;
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            var rt = bg.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = bg.GetComponent<Image>();
            img.sprite = backgroundSprite;
            img.preserveAspect = true;
            bg.transform.SetSiblingIndex(0);
            if (backgroundFillScreen && backgroundSprite.texture != null)
            {
                var arf = bg.AddComponent<AspectRatioFitter>();
                float ar = (float)backgroundSprite.texture.width / Mathf.Max(1f, backgroundSprite.texture.height);
                arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                arf.aspectRatio = ar;
            }
        }

        void SetupParchment(RectTransform root)
        {
            if (parchmentSprite == null) return;
            var pg = new GameObject("Parchment", typeof(RectTransform), typeof(Image));
            var rt = pg.GetComponent<RectTransform>();
            rt.SetParent(root, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = pg.GetComponent<Image>();
            img.sprite = parchmentSprite;
            img.color = parchmentTint;
            img.preserveAspect = true;
            pg.transform.SetAsFirstSibling();
            if (parchmentSprite.texture != null)
            {
                var arf = pg.AddComponent<AspectRatioFitter>();
                float ar = (float)parchmentSprite.texture.width / Mathf.Max(1f, parchmentSprite.texture.height);
                arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                arf.aspectRatio = ar;
            }
        }
    }
}
