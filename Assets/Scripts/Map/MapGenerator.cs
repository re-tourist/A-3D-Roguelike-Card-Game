using UnityEngine;

namespace Game.Map
{
    public class MapGenerator
    {
        public int lanes = 5;       // 直线路条数（4或5）
        public int tiers = 10;      // 列层数（从起点到Boss）
        public int seed = 0;        // 随机种子，0表示系统随机

        public float battleWeight = 0.6f;
        public float eventWeight = 0.2f;
        public float shopWeight = 0.1f;
        public float restWeight = 0.1f;
        public int eliteEvery = 3;  // 每隔n层可能生成精英

        private System.Random rng;

        public MapGraph Generate()
        {
            rng = seed == 0 ? new System.Random() : new System.Random(seed);
            var graph = new MapGraph();

            // 起点节点（tier=0，放中间lane）
            var start = new MapNode(0, 0, Mathf.Clamp(lanes / 2, 0, lanes - 1), MapNodeType.Start)
            {
                state = MapNodeState.Visited // 视为已在起点
            };
            graph.AddNode(start);

            int idCounter = 1;

            // 中间层（1..tiers-2）
            for (int t = 1; t < tiers - 1; t++)
            {
                for (int l = 0; l < lanes; l++)
                {
                    var type = PickType(t);
                    var node = new MapNode(idCounter++, t, l, type);
                    graph.AddNode(node);
                }
            }

            // Boss层（最后一层，放中间lane）
            var boss = new MapNode(idCounter++, tiers - 1, Mathf.Clamp(lanes / 2, 0, lanes - 1), MapNodeType.Boss);
            graph.AddNode(boss);

            // 连接各层节点
            Connect(graph);

            // 从起点更新下一步可选节点
            UpdateAvailability(graph, start.id);
            return graph;
        }

        MapNodeType PickType(int tier)
        {
            if (eliteEvery > 0 && tier % eliteEvery == 0 && tier > 0 && tier < tiers - 1)
            {
                if (rng.NextDouble() < 0.25) return MapNodeType.Elite; // 25%概率在该层出现精英
            }

            double r = rng.NextDouble();
            if (r < battleWeight) return MapNodeType.Battle;
            r -= battleWeight;
            if (r < eventWeight) return MapNodeType.Event;
            r -= eventWeight;
            if (r < shopWeight) return MapNodeType.Shop;
            return MapNodeType.Rest;
        }

        void Connect(MapGraph graph)
        {
            // 按列连接：t -> t+1
            for (int t = 0; t < tiers - 1; t++)
            {
                var srcs = graph.GetTier(t);
                var dsts = graph.GetTier(t + 1);
                foreach (var s in srcs)
                {
                    if (dsts.Count == 0) continue;
                    int targetLane = Mathf.Clamp(s.lane, 0, lanes - 1);

                    // 候选：同lane + 邻近lane
                    var candidates = new System.Collections.Generic.List<MapNode>();
                    foreach (var d in dsts)
                    {
                        if (d.lane == targetLane) candidates.Add(d);
                        if (Mathf.Abs(d.lane - s.lane) == 1) candidates.Add(d);
                    }
                    if (candidates.Count == 0) candidates.Add(dsts[rng.Next(dsts.Count)]);

                    // 选择1-2个唯一目标
                    int count = 1 + (rng.NextDouble() < 0.5 ? 1 : 0);
                    for (int k = 0; k < count; k++)
                    {
                        var d = candidates[rng.Next(candidates.Count)];
                        if (!s.forward.Contains(d.id)) s.forward.Add(d.id);
                    }
                }
            }
        }

        public void UpdateAvailability(MapGraph graph, int fromNodeId)
        {
            foreach (var n in graph.nodes)
            {
                if (n.state != MapNodeState.Visited) n.state = MapNodeState.Locked;
            }

            var from = graph.Get(fromNodeId);
            if (from == null) return;
            foreach (var id in from.forward)
            {
                var n = graph.Get(id);
                if (n != null) n.state = MapNodeState.Available;
            }
        }
    }
}