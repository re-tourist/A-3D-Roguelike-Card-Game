using System.Collections.Generic;

namespace Game.Map
{
    [System.Serializable]
    public class MapNode
    {
        public int id;
        public int tier; // 列索引（0..tiers-1）
        public int lane; // 行索引（0..lanes-1）
        public MapNodeType type;
        public MapNodeState state;
        public List<int> forward; // 指向下一列节点的连接

        public MapNode(int id, int tier, int lane, MapNodeType type)
        {
            this.id = id;
            this.tier = tier;
            this.lane = lane;
            this.type = type;
            state = MapNodeState.Locked;
            forward = new List<int>();
        }
    }
}