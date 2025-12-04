using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Game.Map;

namespace Game.Core
{
    public static class SaveManager
    {
        private const string KeyMap = "SAVE_MAP_PROGRESS";
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "game_save.json");

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

        [System.Serializable]
        public class PlayerData
        {
            public int money;
            public List<string> cards = new List<string>();
        }

        [System.Serializable]
        public class LayerNodeSerializable
        {
            public int id;
            public int layer;
            public int index;
            public NodeType type;
            public bool reachable;
            public bool visited;
            public float normX;
            public float normY;
        }

        [System.Serializable]
        public class LayerEdgeSerializable
        {
            public int fromId;
            public int toId;
        }

        [System.Serializable]
        public class LayerMapProgress
        {
            public int currentNodeId;
            public List<LayerNodeSerializable> nodes;
            public List<LayerEdgeSerializable> edges;
        }

        [System.Serializable]
        public class GameSaveData
        {
            public PlayerData player = new PlayerData();
            public MapProgress map; // 旧图系统
            public LayerMapProgress layerMap; // 新图系统
        }

        static GameSaveData LoadGameSave()
        {
            if (!File.Exists(SaveFilePath)) return new GameSaveData();
            try
            {
                var json = File.ReadAllText(SaveFilePath);
                var data = JsonUtility.FromJson<GameSaveData>(json);
                return data ?? new GameSaveData();
            }
            catch
            {
                return new GameSaveData();
            }
        }

        static void SaveGameSave(GameSaveData data)
        {
            var json = JsonUtility.ToJson(data);
            File.WriteAllText(SaveFilePath, json);
        }

        public static int GetPlayerCardCount()
        {
            var data = LoadGameSave();
            var list = data?.player?.cards;
            return list != null ? list.Count : 0;
        }

        public static int GetPlayerMoney()
        {
            var data = LoadGameSave();
            return data?.player != null ? data.player.money : 0;
        }

        public static void SaveMapProgress(MapGraph graph, int currentNodeId)
        {
            if (DevFlags.DisableSaves) return;
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

            var data = LoadGameSave();
            data.map = mp;
            SaveGameSave(data);
            Debug.Log($"Map progress saved: node={currentNodeId}, nodes={graph.nodes.Count}");
        }

        public static bool TryLoadMapProgress(out MapGraph graph, out int currentNodeId)
        {
            graph = null;
            currentNodeId = -1;
            if (DevFlags.DisableSaves) return false;
            if (!File.Exists(SaveFilePath)) return false;
            var json = File.ReadAllText(SaveFilePath);
            var data = JsonUtility.FromJson<GameSaveData>(json);
            var mp = data?.map;
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
            if (DevFlags.DisableSaves)
            {
                if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
                return;
            }
            if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
        }

        public static void SaveLayerMapProgress(LayerMapGraph graph, int currentNodeId)
        {
            if (DevFlags.DisableSaves) return;
            var data = LoadGameSave();
            var lm = new LayerMapProgress
            {
                currentNodeId = currentNodeId,
                nodes = new List<LayerNodeSerializable>(),
                edges = new List<LayerEdgeSerializable>()
            };

            foreach (var n in graph.nodes)
            {
                lm.nodes.Add(new LayerNodeSerializable
                {
                    id = n.id,
                    layer = n.layer,
                    index = n.index,
                    type = n.type,
                    reachable = n.reachable,
                    visited = n.visited,
                    normX = n.normPos.x,
                    normY = n.normPos.y
                });
            }
            foreach (var e in graph.edges)
            {
                lm.edges.Add(new LayerEdgeSerializable { fromId = e.fromId, toId = e.toId });
            }

            data.layerMap = lm;
            SaveGameSave(data);
            Debug.Log($"LayerMap progress saved: node={currentNodeId}, nodes={graph.nodes.Count}, edges={graph.edges.Count}");
        }

        public static bool TryLoadLayerMapProgress(out LayerMapGraph graph, out int currentNodeId)
        {
            graph = null;
            currentNodeId = -1;
            if (DevFlags.DisableSaves) return false;
            if (!File.Exists(SaveFilePath)) return false;
            try
            {
                var json = File.ReadAllText(SaveFilePath);
                var data = JsonUtility.FromJson<GameSaveData>(json);
                var lm = data?.layerMap;
                if (lm == null) return false;
                var g = new LayerMapGraph();
                foreach (var n in lm.nodes)
                {
                    g.nodes.Add(new LayerNode
                    {
                        id = n.id,
                        layer = n.layer,
                        index = n.index,
                        type = n.type,
                        reachable = n.reachable,
                        visited = n.visited,
                        normPos = new UnityEngine.Vector2(n.normX, n.normY)
                    });
                }
                foreach (var e in lm.edges)
                {
                    g.edges.Add(new LayerEdge { fromId = e.fromId, toId = e.toId });
                }
                graph = g;
                currentNodeId = lm.currentNodeId;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
