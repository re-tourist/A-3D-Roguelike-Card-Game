using UnityEngine;

namespace Game.Map
{
    public enum MapNodeType
    {
        Start,
        Battle,
        Elite,
        Shop,
        Event,
        Rest,
        Treasure,
        Boss
    }

    public enum MapNodeState
    {
        Locked,
        Available,
        Visited
    }
}