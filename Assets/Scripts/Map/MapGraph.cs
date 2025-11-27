using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Map
{
    // 旧图系统：与现有 MapNode.cs/MapController/SaveManager 保持一致
    public class MapGraph
    {
        public List<MapNode> nodes = new List<MapNode>();
        public MapNode Get(int id) { return nodes.Find(n => n.id == id); }
        public void AddNode(MapNode n) { nodes.Add(n); }
    }

    // 新图系统：分层非交叉连线所需的数据结构
    public class LayerNode
    {
        public int id;
        public int layer;
        public int index;
        public NodeType type;
        public bool reachable;
        public bool visited;
        public Vector2 normPos;
    }

    public class LayerEdge
    {
        public int fromId;
        public int toId;
    }

    public class LayerMapGraph
    {
        public List<LayerNode> nodes = new List<LayerNode>();
        public List<LayerEdge> edges = new List<LayerEdge>();
        public IEnumerable<LayerNode> NodesInLayer(int l) { return nodes.Where(n => n.layer == l); }
        public LayerNode GetNode(int id) { return nodes.FirstOrDefault(n => n.id == id); }
    }
}
