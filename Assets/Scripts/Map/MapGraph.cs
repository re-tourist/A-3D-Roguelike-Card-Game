using System.Collections.Generic;

namespace Game.Map
{
    public class MapGraph
    {
        public List<MapNode> nodes = new List<MapNode>();
        private Dictionary<int, MapNode> byId = new Dictionary<int, MapNode>();

        public void AddNode(MapNode n)
        {
            nodes.Add(n);
            byId[n.id] = n;
        }

        public MapNode Get(int id)
        {
            MapNode n;
            return byId.TryGetValue(id, out n) ? n : null;
        }

        public List<MapNode> GetTier(int tier)
        {
            var list = new List<MapNode>();
            foreach (var n in nodes)
                if (n.tier == tier) list.Add(n);
            return list;
        }
    }
}