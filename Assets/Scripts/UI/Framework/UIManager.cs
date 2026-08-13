using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    [Header("Layer Roots")]
    [SerializeField] private RectTransform normalRoot;
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private RectTransform systemRoot;

    [Header("View Prefabs")]
    [SerializeField] private List<UIView> viewPrefabs = new();

    private readonly Dictionary<Type, UIView> prefabMap = new();
    private readonly Dictionary<Type, UIView> instanceMap = new();
    private readonly Dictionary<UILayer, List<UIView>> viewsMap = new();

    private void Awake()
    {
        if (instance != null && instance != this)
            throw new InvalidOperationException("场景中存在多个 UIManager。");

        instance = this;
        InitializeLayerDict();
        BuildPrefabMap();
    }

    private void OnValidate()
    {
        if (normalRoot == null || popupRoot == null || systemRoot == null)
            throw new InvalidOperationException("UIManager 的层级容器没有全部绑定。");

        if (normalRoot == popupRoot || normalRoot == systemRoot || popupRoot == systemRoot)
            throw new InvalidOperationException("Normal、Popup、System 必须绑定不同的层级容器。");

        if (normalRoot.parent != transform || popupRoot.parent != transform || systemRoot.parent != transform)
            throw new InvalidOperationException("三个层级容器必须是 UIManager 的直接子节点。");

        if (normalRoot.GetSiblingIndex() >= popupRoot.GetSiblingIndex() || popupRoot.GetSiblingIndex() >= systemRoot.GetSiblingIndex())
            throw new InvalidOperationException("层级容器的顺序必须是 Normal、Popup、System。");

    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }



    private void InitializeLayerDict()
    {
        viewsMap.Add(UILayer.Normal, new List<UIView>());
        viewsMap.Add(UILayer.Popup, new List<UIView>());
        viewsMap.Add(UILayer.System, new List<UIView>());
    }

    private void BuildPrefabMap()
    {
        for (int i = 0; i < viewPrefabs.Count; i++)
        {
            UIView prefab = viewPrefabs[i];
            if (prefab == null)
            {
                Debug.LogError($"UI Prefab 列表中的第 {i} 项为空。", this);
                continue;
            }

            if (prefab.gameObject.activeSelf)
            {
                Debug.LogError($"UI Prefab {prefab.name} 的根节点必须处于禁用状态。", prefab);
                continue;
            }

            if (prefab.gameObject.scene.IsValid())
            {
                Debug.LogError($"{prefab.name} 是场景对象，不是 Prefab 资源。", prefab);
                continue;
            }

            Type viewType = prefab.GetType();
            if (!prefabMap.TryAdd(viewType, prefab))
                Debug.LogError($"UI Prefab 类型重复配置：{viewType.FullName}", prefab);
        }
    }

    public T Open<T>(object param = null) where T : UIView
    {
        Type viewType = typeof(T);

        if (!instanceMap.TryGetValue(viewType, out UIView view) || view == null)
        {
            if (!prefabMap.TryGetValue(viewType, out UIView prefab))
                throw new InvalidOperationException($"没有配置 UI Prefab：{viewType.FullName}");

            RectTransform layerRoot = GetLayerRoot(prefab.Layer);
            view = Instantiate(prefab, layerRoot, false);
            instanceMap[viewType] = view;
        }

        MoveToTop(view);
        view.Open(param);

        return (T)view;
    }

    public bool Close<T>() where T : UIView
    {
        return instanceMap.TryGetValue(typeof(T), out UIView view) && Close(view);
    }

    public bool Close(UIView view)
    {
        if (view == null || !view.IsOpen) return false;
        viewsMap[view.Layer].Remove(view);
        view.Close();
        return true;
    }

    public bool TryGet<T>(out T view) where T : UIView
    {
        if (instanceMap.TryGetValue(typeof(T), out UIView result) && result != null)
        {
            view = (T)result;
            return true;
        }

        view = null;
        return false;
    }

    private RectTransform GetLayerRoot(UILayer layer)
    {
        return layer switch
        {
            UILayer.Normal => normalRoot,
            UILayer.Popup => popupRoot,
            UILayer.System => systemRoot,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null)
        };
    }

    private void MoveToTop(UIView view)
    {
        List<UIView> views = viewsMap[view.Layer];
        views.Remove(view);
        views.Add(view);
        view.transform.SetAsLastSibling();
    }
}