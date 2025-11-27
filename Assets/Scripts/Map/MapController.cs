using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.Map
{
    public class MapController : MonoBehaviour
    {
        [Header("容器与预制体")]
        public RectTransform mapContainer;     // 用于承载节点与线段
        public NodeView nodePrefab;            // 节点预制体（Button+Image）
        public Image linePrefab;               // 线段预制体（UI Image，颜色与宽度可配置）

        [Header("生成器配置")]
        public int lanes = 5;
        public int tiers = 10;
        public int seed = 0;

        [Header("布局参数")]
        public float horizontalPadding = 100f;
        public float verticalPadding = 80f;
        public float nodeSize = 48f;
        public float lineThickness = 6f;

        [Header("滚动设置")]
        public bool enableScroll = true;
        public float scrollSensitivity = 30f;
        public float verticalScale = 1.4f;

        private MapGraph graph;
        private MapGenerator generator;
        private Dictionary<int, NodeView> nodeViews = new Dictionary<int, NodeView>();

        void Awake()
        {
            generator = new MapGenerator
            {
                lanes = lanes,
                tiers = tiers,
                seed = seed
            };
            EnsureContainer();
            EnsureDefaults();
        }

        void Start()
        {
            // 优先尝试从存档恢复
            if (Core.SaveManager.TryLoadMapProgress(out var loadedGraph, out var currentNodeId))
            {
                ClearContainer();
                graph = loadedGraph;
                // 基于当前位置更新下一步可选
                generator.UpdateAvailability(graph, currentNodeId);
                PlaceNodes();
                DrawConnections();
            }
            else
            {
                BuildMap();
            }
        }

        public void BuildMap()
        {
            ClearContainer();
            graph = generator.Generate();
            PlaceNodes();
            DrawConnections();
        }

        void ClearContainer()
        {
            if (mapContainer == null) return;
            for (int i = mapContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(mapContainer.GetChild(i).gameObject);
            }
            nodeViews.Clear();
        }

        void PlaceNodes()
        {
            var size = mapContainer.rect.size;
            float width = size.x - horizontalPadding * 2f;
            float height = size.y - verticalPadding * 2f;

            foreach (var node in graph.nodes)
            {
                var nv = nodePrefab != null ? Instantiate(nodePrefab, mapContainer) : CreateNodeView();
                nodeViews[node.id] = nv;

                // 计算位置：X按列，Y按lane
                float x = horizontalPadding + (tiers <= 1 ? width * 0.5f : (width * node.tier / (tiers - 1)));
                float y = verticalPadding + (lanes <= 1 ? height * 0.5f : (height * node.lane / (lanes - 1)));

                var rt = nv.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
                rt.anchoredPosition = new Vector2(x, y);
                rt.sizeDelta = new Vector2(nodeSize, nodeSize);

                nv.Bind(node, this);
            }
        }

        void DrawConnections()
        {
            if (linePrefab == null) return; // 可选：没有线段预制体则跳过

            foreach (var node in graph.nodes)
            {
                var fromView = nodeViews[node.id];
                var rtFrom = fromView.GetComponent<RectTransform>();
                Vector2 start = rtFrom.anchoredPosition;

                foreach (var toId in node.forward)
                {
                    var toView = nodeViews[toId];
                    var rtTo = toView.GetComponent<RectTransform>();
                    Vector2 end = rtTo.anchoredPosition;
                    CreateLine(start, end);
                }
            }
        }

        void CreateLine(Vector2 start, Vector2 end)
        {
            Image line;
            if (linePrefab != null)
                line = Instantiate(linePrefab, mapContainer);
            else
            {
                var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(mapContainer, false);
                line = go.GetComponent<Image>();
            }
            var rt = line.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);

            Vector2 dir = end - start;
            float length = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rt.sizeDelta = new Vector2(length, lineThickness);
            rt.anchoredPosition = start + dir * 0.5f;
            rt.localRotation = Quaternion.Euler(0, 0, angle);
            line.color = new Color(1f, 1f, 1f, 0.25f); // 浅色线条
        }

        void EnsureContainer()
        {
            if (mapContainer != null) return;
            EnsureEventSystem();
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var canvases = FindObjectsOfType<Canvas>();
                foreach (var c in canvases)
                {
                    if (c.gameObject.scene == gameObject.scene) { canvas = c; break; }
                }
            }
            Transform parent = canvas != null ? canvas.transform : transform;
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

                var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
                var bgRt = bgGo.GetComponent<RectTransform>();
                bgRt.SetParent(vprt, false);
                bgRt.anchorMin = new Vector2(0f, 0f);
                bgRt.anchorMax = new Vector2(1f, 1f);
                bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
                var bgImg = bgGo.GetComponent<Image>();
                bgImg.color = Color.black;
                bgImg.raycastTarget = false;

                var contentGo = new GameObject("MapContainer", typeof(RectTransform));
                var rt = contentGo.GetComponent<RectTransform>();
                rt.SetParent(vprt, false);
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;

                var sr = scrollGo.GetComponent<ScrollRect>();
                sr.viewport = vprt;
                sr.content = rt;
                sr.vertical = true;
                sr.horizontal = false;
                sr.movementType = ScrollRect.MovementType.Clamped;
                sr.scrollSensitivity = scrollSensitivity;

                mapContainer = rt;
                mapContainer.transform.SetAsLastSibling();

                var vpSize = vprt.rect.size;
                mapContainer.sizeDelta = new Vector2(vpSize.x, vpSize.y * Mathf.Max(1f, verticalScale));
            }
            else
            {
                var host = new GameObject("MapContainer", typeof(RectTransform));
                var rt = host.GetComponent<RectTransform>();
                rt.SetParent(parent, false);
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                mapContainer = rt;
                mapContainer.transform.SetAsLastSibling();
            }
        }

        void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        NodeView CreateNodeView()
        {
            var go = new GameObject("NodeView", typeof(RectTransform), typeof(Image), typeof(Button), typeof(NodeView));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(mapContainer, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.18f);
            var btn = go.GetComponent<Button>();
            var ringGo = new GameObject("Ring", typeof(RectTransform), typeof(Image));
            var rrt = ringGo.GetComponent<RectTransform>();
            rrt.SetParent(go.transform, false);
            rrt.anchorMin = new Vector2(0f, 0f); rrt.anchorMax = new Vector2(1f, 1f);
            rrt.offsetMin = new Vector2(-4f, -4f); rrt.offsetMax = new Vector2(4f, 4f);
            var ringImg = ringGo.GetComponent<Image>();
            ringImg.color = new Color(1f, 1f, 1f, 0.2f);
            var nv = go.GetComponent<NodeView>();
            nv.icon = img;
            nv.button = btn;
            nv.ring = ringImg;
            return nv;
        }

        void EnsureDefaults()
        {
            if (horizontalPadding < 0f) horizontalPadding = 0f;
            if (verticalPadding < 0f) verticalPadding = 0f;
            nodeSize = Mathf.Clamp(nodeSize, 24f, 128f);
            lineThickness = Mathf.Clamp(lineThickness, 1f, 12f);
        }

        public void OnNodeClicked(MapNode node)
        {
            if (node.state != MapNodeState.Available) return;

            // 标记访问并解锁下一层
            node.state = MapNodeState.Visited;
            generator.UpdateAvailability(graph, node.id);

            // 刷新所有节点视图
            foreach (var kv in nodeViews)
            {
                kv.Value.Bind(graph.Get(kv.Key), this);
            }

            // 根据节点类型执行场景切换或事件（示例）
            switch (node.type)
            {
                case MapNodeType.Battle:
                    TryLoad(SceneFlowManager.SceneType.Battle);
                    break;
                case MapNodeType.Shop:
                    TryLoad(SceneFlowManager.SceneType.Shop);
                    break;
                case MapNodeType.Event:
                    TryLoad(SceneFlowManager.SceneType.Event);
                    break;
                case MapNodeType.Elite:
                    TryLoad(SceneFlowManager.SceneType.Elite);
                    break;
                case MapNodeType.Rest:
                    TryLoad(SceneFlowManager.SceneType.Rest);
                    break;
                case MapNodeType.Treasure:
                    TryLoad(SceneFlowManager.SceneType.Reward);
                    break;
                case MapNodeType.Boss:
                    TryLoad(SceneFlowManager.SceneType.Battle);
                    break;
            }

            // 保存进度（节点状态与当前位置）
            Core.SaveManager.SaveMapProgress(graph, node.id);
        }

        void TryLoad(SceneFlowManager.SceneType type)
        {
            SceneFlowManager.Instance?.LoadScene(type);
        }

    public Color GetColorForType(MapNodeType type)
        {
            switch (type)
            {
                case MapNodeType.Start: return new Color(0.6f, 0.9f, 1f);
                case MapNodeType.Battle: return new Color(1f, 0.5f, 0.5f);
                case MapNodeType.Elite: return new Color(0.95f, 0.25f, 0.25f);
                case MapNodeType.Shop: return new Color(0.6f, 1f, 0.6f);
                case MapNodeType.Event: return new Color(1f, 0.9f, 0.6f);
                case MapNodeType.Rest: return new Color(0.8f, 0.8f, 1f);
                case MapNodeType.Treasure: return new Color(1f, 0.85f, 0.2f);
                case MapNodeType.Boss: return new Color(0.7f, 0.2f, 0.2f);
                default: return Color.white;
            }
        }
    }
}
