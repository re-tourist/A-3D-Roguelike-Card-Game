using System.Collections.Generic;
using UnityEngine;

namespace Game.Map
{
    public class MapGenerator
    {
        public int lanes = 5;
        public int tiers = 10;
        public int seed = 0;

        System.Random rnd;

        public MapGraph Generate()
        {
            rnd = seed == 0 ? new System.Random() : new System.Random(seed);
            var g = new MapGraph();

            int id = 0;
            for (int t = 0; t < tiers; t++)
            {
                for (int i = 0; i < lanes; i++)
                {
                    var type = ResolveType(t);
                    if (t == 0) type = MapNodeType.Start;
                    if (t == tiers - 1) type = MapNodeType.Boss;
                    var n = new MapNode(id, t, i, type);
                    if (t == 0) n.state = MapNodeState.Available;
                    g.AddNode(n);
                    id++;
                }
            }

            for (int t = 0; t < tiers - 1; t++)
            {
                int prevK = 0;
                for (int i = 0; i < lanes; i++)
                {
                    int srcId = t * lanes + i;
                    int kStay = Mathf.Clamp(i, 0, lanes - 1);
                    int kRight = Mathf.Clamp(i + 1, 0, lanes - 1);
                    float r = (float)rnd.NextDouble();
                    int k = r < 0.5f ? kStay : kRight;
                    k = Mathf.Max(k, prevK);
                    int dstId = (t + 1) * lanes + k;
                    g.Get(srcId).forward.Add(dstId);
                    prevK = k;
                }
            }

            return g;
        }

        public void UpdateAvailability(MapGraph g, int currentNodeId)
        {
            var cur = g.Get(currentNodeId);
            if (cur == null) return;
            int nextTier = cur.tier + 1;

            for (int i = 0; i < g.nodes.Count; i++)
            {
                var n = g.nodes[i];
                if (n.tier >= nextTier && n.state != MapNodeState.Visited)
                {
                    n.state = MapNodeState.Locked;
                }
            }

            foreach (var toId in cur.forward)
            {
                var dst = g.Get(toId);
                if (dst != null && dst.state != MapNodeState.Visited)
                {
                    dst.state = MapNodeState.Available;
                }
            }
        }

        MapNodeType ResolveType(int tier)
        {
            if (tier == tiers - 1) return MapNodeType.Boss;
            double r = rnd == null ? UnityEngine.Random.value : rnd.NextDouble();
            if (r < 0.5) return MapNodeType.Battle;
            if (r < 0.65) return MapNodeType.Elite;
            if (r < 0.8) return MapNodeType.Event;
            if (r < 0.92) return MapNodeType.Shop;
            return MapNodeType.Rest;
        }
    }
}
