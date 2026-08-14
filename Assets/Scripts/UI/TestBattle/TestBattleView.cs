using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 地图流程测试使用的虚拟战斗界面。
/// </summary>
public sealed class TestBattleView : UIView
{
    [SerializeField] private TMP_Text battleText;
    [SerializeField] private Button winButton;
    [SerializeField] private Button loseButton;

    private MapNodeData currentNode;

    private void Awake()
    {
        winButton.onClick.AddListener(OnWinClicked);
        loseButton.onClick.AddListener(OnLoseClicked);
    }

    protected override void OnOpen(object param)
    {
        currentNode = param as MapNodeData;

        if (currentNode == null)
        {
            battleText.text = "没有地图节点数据";
            return;
        }

        battleText.text =
            $"虚拟战斗\n\n" +
            $"节点 ID：{currentNode.Id}\n" +
            $"层数：{currentNode.FloorIndex + 1}\n" +
            $"类型：{currentNode.NodeType}";
    }

    protected override void OnClose()
    {
        currentNode = null;
    }

    private void OnWinClicked()
    {
        MapManager.Instance.OnTestBattleFinished(true);
    }

    private void OnLoseClicked()
    {
        MapManager.Instance.OnTestBattleFinished(false);
    }
}