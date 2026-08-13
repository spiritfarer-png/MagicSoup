using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MapNodeView : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private TMP_Text nodeText;

    private int nodeId;
    private MapView mapView;

    public void Initialize(MapNodeData node, MapView owner)
    {
        nodeId = node.Id;
        mapView = owner;

        nodeText.text = GetNodeName(node.NodeType);

        button.interactable = node.State == MapNodeState.Available;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        mapView.OnNodeClicked(nodeId);
    }

    private string GetNodeName(MapNodeType nodeType)
    {
        switch (nodeType)
        {
            case MapNodeType.NormalBattle:
                return "NormalBattle";

            case MapNodeType.EliteBattle:
                return "EliteBattle";

            case MapNodeType.RandomEvent:
                return "?";

            case MapNodeType.Treasure:
                return "Treasure";

            case MapNodeType.Boss:
                return "Boss";

            default:
                return nodeType.ToString();
        }
    }
}