using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MapNodeView : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private TMP_Text nodeText;

    [Header("节点图标")]
    [Tooltip("图标 Image，预制体上可放置；未配置时自动动态创建")]
    [SerializeField]
    private Image nodeIcon;
    [Tooltip("五种节点类型对应的图标，按类型匹配")]
    [SerializeField]
    private Sprite normalBattleIcon;
    [SerializeField]
    private Sprite eliteBattleIcon;
    [SerializeField]
    private Sprite randomEventIcon;
    [SerializeField]
    private Sprite treasureIcon;
    [SerializeField]
    private Sprite bossIcon;

    private int nodeId;
    private MapView mapView;

    public void Initialize(MapNodeData node, MapView owner)
    {
        nodeId = node.Id;
        mapView = owner;

        nodeText.text = GetNodeName(node.NodeType);
        ApplyNodeIcon(node.NodeType);

        button.interactable = node.State == MapNodeState.Available;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    /// <summary>
    /// 根据节点类型设置图标；对应图标未配置时回退为文字显示。
    /// </summary>
    private void ApplyNodeIcon(MapNodeType nodeType)
    {
        Sprite sprite = GetNodeIcon(nodeType);

        if (sprite == null)
        {
            if (nodeIcon != null) nodeIcon.gameObject.SetActive(false);
            nodeText.gameObject.SetActive(true);
            return;
        }

        if (nodeIcon == null)
        {
            // 预制体未放置 Image 时动态创建，保证配置图片后开箱即用
            nodeIcon = GetComponent<Image>();
            if (nodeIcon == null)
            {
                nodeIcon = gameObject.AddComponent<Image>();
            }
        }

        nodeIcon.sprite = sprite;
        nodeIcon.preserveAspect = true;
        // 必须参与点击命中，否则隐藏文字后节点无法被 GraphicRaycaster 命中，按钮点击失效
        nodeIcon.raycastTarget = true;
        nodeIcon.gameObject.SetActive(true);
        nodeText.gameObject.SetActive(false);
    }

    private Sprite GetNodeIcon(MapNodeType nodeType)
    {
        switch (nodeType)
        {
            case MapNodeType.NormalBattle: return normalBattleIcon;
            case MapNodeType.EliteBattle: return eliteBattleIcon;
            case MapNodeType.RandomEvent: return randomEventIcon;
            case MapNodeType.Treasure: return treasureIcon;
            case MapNodeType.Boss: return bossIcon;
            default: return null;
        }
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
