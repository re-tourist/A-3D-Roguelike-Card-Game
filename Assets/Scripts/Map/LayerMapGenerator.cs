using System.Collections.Generic;
using UnityEngine;

namespace Game.Map
{
    public class LayerMapGenerator : MonoBehaviour
    {
        public int layers = 8;
        public int width = 6;
        public float stayProb = 0.5f;
        public float rightProb = 0.5f;
        public int seed = 0;
        public float timeAlpha = 0.2f;
        public float spacingX = 0.18f;
        public float spacingY = 0.16f;
        public Vector2 offset = new Vector2(0.1f, 0.1f);
        public float[] baseProbs = new float[] { 0.5f, 0.15f, 0.1f, 0.15f, 0.1f };

        System.Random rnd;

        public LayerMapGraph Generate()
        {
            rnd = seed == 0 ? new System.Random() : new System.Random(seed);
            var g = new LayerMapGraph();
            var lastSeen = new Dictionary<NodeType, int>();
            lastSeen[NodeType.Monster] = -999;
            lastSeen[NodeType.Elite] = -999;
            lastSeen[NodeType.Shop] = -999;
            lastSeen[NodeType.Event] = -999;
            lastSeen[NodeType.Rest] = -999;

            int id = 0;
            int bossId = -1;
            for (int l = 0; l < layers; l++)
            {
                int currWidth = (l == layers - 1) ? 1 : width; // 最后一层仅一个 Boss 节点
                for (int i = 0; i < currWidth; i++)
                {
                    var n = new LayerNode();
                    n.id = id++;
                    n.layer = l;
                    n.index = i;
                    n.type = ResolveType(l, lastSeen);
                    n.reachable = l == 0;
                    float x = offset.x + i * spacingX;
                    if (l == layers - 1)
                    {
                        // Boss 居中（横向）
                        x = offset.x + (width - 1) * spacingX * 0.5f;
                    }
                    n.normPos = new Vector2(x, offset.y + l * spacingY); // 起点在底部，向上递增
                    g.nodes.Add(n);
                    if (l == layers - 1) bossId = n.id;
                }

                if (l == layers - 1)
                {
                    // 最后一层唯一 Boss
                    g.nodes[g.nodes.Count - 1].type = NodeType.Boss;
                }
                if (l == layers - 2)
                {
                    for (int i = 0; i < width; i++)
                    {
                        int idx = g.nodes.Count - width + i;
                        g.nodes[idx].type = NodeType.Rest;
                    }
                }
            }

            for (int l = 0; l < layers - 1; l++)
            {
                int prevK = 0;
                int currWidth = (l == layers - 1) ? 1 : width;
                int nextWidth = (l + 1 == layers - 1) ? 1 : width;
                for (int i = 0; i < currWidth; i++)
                {
                    int srcId = l * width + i;
                    // 保底：同索引直连（满足“每行的第一个节点连接下一行第一个节点，以此类推”）
                    if (l + 1 == layers - 1)
                    {
                        // 倒数第二层全部直连 Boss
                        if (bossId >= 0) g.edges.Add(new LayerEdge { fromId = srcId, toId = bossId });
                    }
                    else
                    {
                        int kBase = Mathf.Clamp(i, 0, nextWidth - 1);
                        int dstBaseId = (l + 1) * width + kBase;
                        g.edges.Add(new LayerEdge { fromId = srcId, toId = dstBaseId });
                    }

                    // 额外：非交叉侧向连接（允许左/停/右），丰富分支（除最后一层）
                    if (nextWidth > 1)
                    {
                        int kLeft = Mathf.Clamp(i - 1, 0, nextWidth - 1);
                        int kStay = Mathf.Clamp(i, 0, nextWidth - 1);
                        int kRight = Mathf.Clamp(i + 1, 0, nextWidth - 1);
                        float r = (float)rnd.NextDouble();
                        int k = kStay;
                        if (r < 0.33f) k = kLeft; else if (r < 0.66f) k = kStay; else k = kRight;
                        k = Mathf.Max(k, prevK);
                        int dstId = (l + 1) * width + k;
                        g.edges.Add(new LayerEdge { fromId = srcId, toId = dstId });
                        prevK = k;
                    }
                }
            }

            return g;
        }

        NodeType ResolveType(int layer, Dictionary<NodeType, int> lastSeen)
        {
            if (layer == layers - 1) return NodeType.Boss;
            if (layer == layers - 2) return NodeType.Rest;
            var types = new NodeType[] { NodeType.Monster, NodeType.Elite, NodeType.Shop, NodeType.Event, NodeType.Rest };
            var scores = new List<float>();
            float sum = 0f;
            for (int t = 0; t < types.Length; t++)
            {
                int steps = layer - lastSeen[types[t]];
                float w = baseProbs[t] * (1f + steps * timeAlpha) + 0.1f;
                scores.Add(w);
                sum += w;
            }
            float pick = (float)rnd.NextDouble() * sum;
            float acc = 0f;
            for (int t = 0; t < types.Length; t++)
            {
                acc += scores[t];
                if (pick <= acc)
                {
                    lastSeen[types[t]] = layer;
                    return types[t];
                }
            }
            lastSeen[NodeType.Monster] = layer;
            return NodeType.Monster;
        }
    }
}
