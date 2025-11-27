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
        public float nodeSize = 64f;
        public float bossScale = 3f;
        public int curveSamples = 6;
        public float curveAmount = 0.15f; // 相对长度的弯曲幅度
        public bool handDrawn = true;
        public float wiggleAmplitude = 4f;
        public float wiggleFrequency = 1.6f;
        public float verticalScale = 1.4f;
        public float bossGap = 0.08f;
        public float bossPrevGapFactor = 1.5f;
        public bool enableScroll = true;
        public float scrollSensitivity = 30f;
        public bool enablePulse = true;
        public float pulseMaxScale = 1.25f;
        public float pulseSpeed = 2.2f;
        public Sprite monsterIcon;
        public Sprite eliteIcon;
        public Sprite shopIcon;
        public Sprite eventIcon;
        public Sprite restIcon;
        public Sprite bossIcon;
        public Sprite backgroundSprite;
        public bool backgroundFillScreen = true;
        public Sprite mapTextureSprite;
        public Color mapTextureTint = new Color(1f, 0.95f, 0.8f, 1f);

        readonly Dictionary<int, Button> nodeButtons = new Dictionary<int, Button>();
        readonly List<Vector2> placedPositions = new List<Vector2>();
        LayerMapGraph currentGraph;
        int currentNodeId = -1;

        void OnValidate()
        {
            nodeSize = Mathf.Clamp(nodeSize, 24f, 180f);
            bossScale = Mathf.Clamp(bossScale, 1f, 3f);
            verticalScale = Mathf.Clamp(verticalScale, 1f, 3f);
            bossGap = Mathf.Clamp01(bossGap);
            bossPrevGapFactor = Mathf.Clamp(bossPrevGapFactor, 1f, 3f);
            lineThickness = Mathf.Clamp(lineThickness, 2f, 12f);
            curveSamples = Mathf.Clamp(curveSamples, 4, 24);
            wiggleAmplitude = Mathf.Clamp(wiggleAmplitude, 0f, 8f);
            pulseMaxScale = Mathf.Clamp(pulseMaxScale, 1.05f, 2.0f);
            pulseSpeed = Mathf.Clamp(pulseSpeed, 0.2f, 6f);
        }

        void Start()
        {
            if (generator == null) generator = GetComponent<LayerMapGenerator>();
            if (generator == null) generator = gameObject.AddComponent<LayerMapGenerator>();
            if (targetRoot == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    var canvases = FindObjectsOfType<Canvas>();
                    foreach (var c in canvases) { if (c.gameObject.scene == gameObject.scene) { canvas = c; break; } }
                }
                if (canvas != null)
                {
                    canvas.pixelPerfect = true;
                    Transform parent = canvas.transform;
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
                        var vpImg = vpGo.GetComponent<Image>();
                        vpImg.color = new Color(0f, 0f, 0f, 0f);
                        vpImg.raycastTarget = true;
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
                        SetupMapTexture(targetRoot);
                    }
                    else
                    {
                        var go = new GameObject("MapView", typeof(RectTransform));
                        targetRoot = go.GetComponent<RectTransform>();
                        targetRoot.SetParent(parent, false);
                        targetRoot.anchorMin = Vector2.zero; targetRoot.anchorMax = Vector2.one;
                        targetRoot.offsetMin = Vector2.zero; targetRoot.offsetMax = Vector2.zero;
                        SetupBackground(parent);
                        SetupMapTexture(targetRoot);
                    }
                }
            }
            if (generator != null && targetRoot != null)
            {
                LayerMapGraph g;
                if (Game.Core.SaveManager.TryLoadLayerMapProgress(out var loaded, out var curId))
                {
                    g = loaded;
                    currentNodeId = curId;
                }
                else
                {
                    g = generator.Generate();
                    currentNodeId = -1;
                }
                var parentRt = targetRoot.parent as RectTransform;
                if (enableScroll && parentRt != null)
                {
                    var vpSize = parentRt.rect.size;
                    targetRoot.sizeDelta = new Vector2(vpSize.x, vpSize.y * Mathf.Max(1f, verticalScale));
                }
                ApplyAvailabilityByCurrentNode(g, currentNodeId);
                currentGraph = g;
                RenderGraph(g);
            }
        }

        void ApplyAvailabilityByCurrentNode(LayerMapGraph g, int curId)
        {
            for (int i = 0; i < g.nodes.Count; i++) g.nodes[i].reachable = false;
            if (curId < 0)
            {
                int minLayer = int.MaxValue;
                for (int i = 0; i < g.nodes.Count; i++) if (g.nodes[i].layer < minLayer) minLayer = g.nodes[i].layer;
                for (int i = 0; i < g.nodes.Count; i++) if (g.nodes[i].layer == minLayer) g.nodes[i].reachable = true;
                return;
            }
            for (int i = 0; i < g.edges.Count; i++)
            {
                var e = g.edges[i];
                if (e.fromId == curId)
                {
                    var to = g.GetNode(e.toId);
                    if (to != null) to.reachable = true;
                }
            }
        }

        void RenderGraph(LayerMapGraph g)
        {
            var size = targetRoot.rect.size;
            var positions = ComputeLayeredPositions(g, size);
            for (int i = 0; i < g.edges.Count; i++)
            {
                var e = g.edges[i];
                var a = g.GetNode(e.fromId);
                var b = g.GetNode(e.toId);
                var pa = positions[a.id];
                var pb = positions[b.id];
                DrawCurvedLine(pa, pb, a.type, b.type);
            }
            for (int i = 0; i < g.nodes.Count; i++)
            {
                var n = g.nodes[i];
                var p = positions[n.id];
                p = ClampToBoundsForType(p, size, n.type);
                p = AvoidOverlap(p, size, RadiusForType(n.type));
                var btn = CreateNodeButton(n, p);
                nodeButtons[n.id] = btn;
                placedPositions.Add(p);
            }
        }

        Dictionary<int, Vector2> ComputeLayeredPositions(LayerMapGraph g, Vector2 size)
        {
            var dict = new Dictionary<int, Vector2>();
            int maxLayer = 0;
            for (int i = 0; i < g.nodes.Count; i++) if (g.nodes[i].layer > maxLayer) maxLayer = g.nodes[i].layer;
            float usableW = size.x - areaPadding.x * 2f;
            float usableH = size.y - areaPadding.y * 2f;
            for (int l = 0; l <= maxLayer; l++)
            {
                var list = new List<LayerNode>();
                for (int i = 0; i < g.nodes.Count; i++) if (g.nodes[i].layer == l) list.Add(g.nodes[i]);
                int count = list.Count;
                float baseY = (count > 0 ? (usableH * (maxLayer == 0 ? 0.5f : (float)l / maxLayer)) : 0f);
                if (l == maxLayer - 1)
                {
                    float delta = usableH * (bossPrevGapFactor - 1f) / Mathf.Max(1, maxLayer);
                    baseY = Mathf.Max(0f, baseY - delta);
                }
                float y = -size.y * 0.5f + areaPadding.y + baseY;
                for (int i = 0; i < count; i++)
                {
                    float t = (i + 1f) / (count + 1f);
                    float x = -size.x * 0.5f + areaPadding.x + usableW * t;
                    dict[list[i].id] = new Vector2(x, y);
                }
            }
            return dict;
        }

        Vector2 NormToLocal(Vector2 norm, Vector2 size)
        {
            float x = Mathf.Lerp(areaPadding.x, size.x - areaPadding.x, Mathf.Clamp01(norm.x));
            float y = Mathf.Lerp(areaPadding.y, size.y - areaPadding.y, Mathf.Clamp01(norm.y));
            return new Vector2(x - size.x * 0.5f, y - size.y * 0.5f);
        }

        Vector2 ClampToBoundsForType(Vector2 p, Vector2 size, NodeType t)
        {
            float r = RadiusForType(t);
            float minX = -size.x * 0.5f + areaPadding.x + r;
            float maxX =  size.x * 0.5f - areaPadding.x - r;
            float minY = -size.y * 0.5f + areaPadding.y + r;
            float maxY =  size.y * 0.5f - areaPadding.y - r;
            return new Vector2(Mathf.Clamp(p.x, minX, maxX), Mathf.Clamp(p.y, minY, maxY));
        }

        Vector2 AvoidOverlap(Vector2 p, Vector2 size, float minDist)
        {
            int tries = 0;
            float step = Mathf.Max(8f, nodeSize * 0.25f);
            while (tries < 24)
            {
                bool ok = true;
                for (int i = 0; i < placedPositions.Count; i++)
                {
                    if ((placedPositions[i] - p).sqrMagnitude < (minDist * minDist)) { ok = false; break; }
                }
                if (ok) break;
                float dir = ((tries % 2) == 0) ? 1f : -1f;
                p.x += step * dir;
                p = ClampToBoundsForType(p, size, NodeType.Monster);
                tries++;
            }
            return p;
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
            img.color = new Color(1f, 1f, 1f, 0f);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = n.reachable;
            Sprite s = SpriteForType(n.type);
            if (useIcons && s != null)
            {
                var igo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                var irt = igo.GetComponent<RectTransform>();
                irt.SetParent(go.transform, false);
                irt.anchorMin = new Vector2(0.1f, 0.1f);
                irt.anchorMax = new Vector2(0.9f, 0.9f);
                irt.offsetMin = Vector2.zero; irt.offsetMax = Vector2.zero;
                var iimg = igo.GetComponent<Image>();
                iimg.sprite = s; iimg.preserveAspect = true; iimg.color = Color.white;
                iimg.raycastTarget = false;
                if (n.type == NodeType.Boss)
                {
                    irt.localScale = new Vector3(bossScale, bossScale, 1f);
                }
                if (enablePulse)
                {
                    var pulse = igo.AddComponent<ReachablePulse>();
                    pulse.enabledPulse = n.reachable;
                    float baseScale = (n.type == NodeType.Boss) ? bossScale : 1f;
                    pulse.minScale = baseScale;
                    pulse.maxScale = baseScale * pulseMaxScale;
                    pulse.speed = pulseSpeed;
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
                if (enablePulse)
                {
                    var pulse = tgo.AddComponent<ReachablePulse>();
                    pulse.enabledPulse = n.reachable;
                    float baseScale = (n.type == NodeType.Boss) ? bossScale : 1f;
                    pulse.minScale = baseScale;
                    pulse.maxScale = baseScale * pulseMaxScale;
                    pulse.speed = pulseSpeed;
                }
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnNodeClicked(n));
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

        void OnNodeClicked(LayerNode n)
        {
            if (currentGraph != null)
            {
                n.visited = true;
                ApplyAvailabilityByCurrentNode(currentGraph, n.id);
                RefreshInteractables();
                Game.Core.SaveManager.SaveLayerMapProgress(currentGraph, n.id);
            }
            var type = SceneTypeFor(n.type);
            SceneFlowManager.Instance?.LoadScene(type);
        }

        void RefreshInteractables()
        {
            for (int i = 0; i < currentGraph.nodes.Count; i++)
            {
                var node = currentGraph.nodes[i];
                if (!nodeButtons.TryGetValue(node.id, out var btn)) continue;
                btn.interactable = node.reachable;
                var icon = btn.transform.Find("Icon");
                if (icon != null)
                {
                    var pulse = icon.GetComponent<ReachablePulse>();
                    if (pulse != null) pulse.enabledPulse = enablePulse && node.reachable;
                }
                var text = btn.transform.Find("Text");
                if (text != null)
                {
                    var pulse = text.GetComponent<ReachablePulse>();
                    if (pulse != null) pulse.enabledPulse = enablePulse && node.reachable;
                }
            }
        }

        SceneFlowManager.SceneType SceneTypeFor(NodeType t)
        {
            if (t == NodeType.Monster) return SceneFlowManager.SceneType.Battle;
            if (t == NodeType.Elite) return SceneFlowManager.SceneType.Elite;
            if (t == NodeType.Shop) return SceneFlowManager.SceneType.Shop;
            if (t == NodeType.Event) return SceneFlowManager.SceneType.Event;
            if (t == NodeType.Rest) return SceneFlowManager.SceneType.Rest;
            if (t == NodeType.Boss) return SceneFlowManager.SceneType.Battle;
            return SceneFlowManager.SceneType.Map;
        }

        void SetupBackground(Transform parent)
        {
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            var rt = bg.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = bg.GetComponent<Image>();
            img.sprite = null;
            img.preserveAspect = false;
            img.color = Color.black;
            img.raycastTarget = false;
            bg.transform.SetSiblingIndex(0);
        }

        void SetupMapTexture(RectTransform root)
        {
            if (mapTextureSprite == null) return;
            var pg = new GameObject("MapTexture", typeof(RectTransform), typeof(Image));
            var rt = pg.GetComponent<RectTransform>();
            rt.SetParent(root, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = pg.GetComponent<Image>();
            img.sprite = mapTextureSprite;
            img.color = mapTextureTint;
            img.preserveAspect = true;
            img.raycastTarget = false;
            pg.transform.SetAsFirstSibling();
            if (mapTextureSprite.texture != null)
            {
                var arf = pg.AddComponent<AspectRatioFitter>();
                float ar = (float)mapTextureSprite.texture.width / Mathf.Max(1f, mapTextureSprite.texture.height);
                arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                arf.aspectRatio = ar;
            }
        }
    }
}
