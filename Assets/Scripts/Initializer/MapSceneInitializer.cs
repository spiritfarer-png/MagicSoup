using UnityEngine;

/// <summary>
/// 进入地图场景后生成并打开地图。
/// </summary>
public sealed class MapSceneInitializer : MonoBehaviour
{
    [SerializeField]
    private int testSeed = 42;

    private void Start()
    {
        MapManager.Instance.GenerateNewMap(testSeed);
        MapManager.Instance.OpenMap();
    }
}