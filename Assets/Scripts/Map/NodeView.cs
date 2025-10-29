using UnityEngine;
using UnityEngine.UI;

namespace Game.Map
{
    public class NodeView : MonoBehaviour
    {
        public Image icon;
        public Button button;
        public Image ring;

        private MapNode boundNode;
        private MapController controller;

        public void Bind(MapNode node, MapController mapController)
        {
            boundNode = node;
            controller = mapController;
            name = $"Node_{node.id}_{node.type}";
            Refresh();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => controller.OnNodeClicked(boundNode));
            }
        }

        public void Refresh()
        {
            if (icon != null)
            {
                icon.color = controller.GetColorForType(boundNode.type);
            }

            if (button != null)
            {
                button.interactable = boundNode.state == MapNodeState.Available;
            }

            if (ring != null)
            {
                ring.enabled = boundNode.state == MapNodeState.Available;
                ring.color = new Color(1f, 1f, 1f, boundNode.state == MapNodeState.Available ? 0.9f : 0.2f);
            }
        }
    }
}