using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
                var nv = Instantiate(nodePrefab, mapContainer);
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
            var line = Instantiate(linePrefab, mapContainer);
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
                    TryLoadSceneBattle();
                    break;
                case MapNodeType.Shop:
                    Debug.Log("Shop node selected - TODO: load Shop scene.");
                    break;
                case MapNodeType.Event:
                    Debug.Log("Event node selected - TODO: load Event scene.");
                    break;
                case MapNodeType.Elite:
                    Debug.Log("Elite node selected - TODO: load Elite battle scene.");
                    break;
                case MapNodeType.Rest:
                    Debug.Log("Rest node selected - TODO: open Rest UI.");
                    break;
                case MapNodeType.Treasure:
                    Debug.Log("Treasure node selected - TODO: open Reward UI.");
                    break;
                case MapNodeType.Boss:
                    TryLoadSceneBoss();
                    break;
            }

            // 保存进度（节点状态与当前位置）
            Core.SaveManager.SaveMapProgress(graph, node.id);
        }

        void TryLoadSceneBattle()
        {
            // 若项目已有 SceneFlowManager 的 Battle 映射则触发加载
            try
            {
                SceneFlowManager.Instance?.LoadScene(SceneFlowManager.SceneType.Battle);
            }
            catch { Debug.Log("SceneFlowManager not found or Battle scene missing."); }
        }

        void TryLoadSceneBoss()
        {
            try
            {
                SceneFlowManager.Instance?.LoadScene(SceneFlowManager.SceneType.Battle);
            }
            catch { Debug.Log("SceneFlowManager not found or Boss scene missing."); }
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