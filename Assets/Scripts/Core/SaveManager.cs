using System.Collections.Generic;
using UnityEngine;
using Game.Map;

namespace Game.Core
{
    public static class SaveManager
    {
        private const string KeyMap = "SAVE_MAP_PROGRESS";

        [System.Serializable]
        public class MapProgress
        {
            public int currentNodeId;
            public List<MapNodeSerializable> nodes;
        }

        [System.Serializable]
        public class MapNodeSerializable
        {
            public int id;
            public int tier;
            public int lane;
            public MapNodeType type;
            public MapNodeState state;
            public List<int> forward;
        }

        public static void SaveMapProgress(MapGraph graph, int currentNodeId)
        {
            var mp = new MapProgress
            {
                currentNodeId = currentNodeId,
                nodes = new List<MapNodeSerializable>()
            };

            foreach (var n in graph.nodes)
            {
                mp.nodes.Add(new MapNodeSerializable
                {
                    id = n.id,
                    tier = n.tier,
                    lane = n.lane,
                    type = n.type,
                    state = n.state,
                    forward = new List<int>(n.forward)
                });
            }

            var json = JsonUtility.ToJson(mp);
            PlayerPrefs.SetString(KeyMap, json);
            PlayerPrefs.Save();
            Debug.Log($"Map progress saved: node={currentNodeId}, nodes={graph.nodes.Count}");
        }

        public static bool TryLoadMapProgress(out MapGraph graph, out int currentNodeId)
        {
            graph = null;
            currentNodeId = -1;
            if (!PlayerPrefs.HasKey(KeyMap)) return false;

            var json = PlayerPrefs.GetString(KeyMap);
            var mp = JsonUtility.FromJson<MapProgress>(json);
            if (mp == null) return false;

            graph = new MapGraph();
            foreach (var n in mp.nodes)
            {
                var node = new MapNode(n.id, n.tier, n.lane, n.type)
                {
                    state = n.state
                };
                node.forward.AddRange(n.forward ?? new List<int>());
                graph.AddNode(node);
            }

            currentNodeId = mp.currentNodeId;
            return true;
        }

        public static void ClearMapProgress()
        {
            PlayerPrefs.DeleteKey(KeyMap);
        }
    }
}