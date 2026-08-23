using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class UIView : MonoBehaviour
{
    [SerializeField] private UILayer layer = UILayer.Normal;

    public UILayer Layer => layer;
    public bool IsOpen { get; private set; }
    public event Action<UIView> Closed;
    public void Open(object param)
    {
        if (IsOpen) return;
        IsOpen = true;
        gameObject.SetActive(true);
        OnOpen(param);
    }

    public void Close()
    {
        if (!IsOpen) return;
        OnClose();
        IsOpen = false;
        gameObject.SetActive(false);
        Closed?.Invoke(this);
    }

    public Tween PlayOpenTween()
    {
        transform.DOKill();
        transform.localScale = Vector3.zero;
        return transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public Tween PlayCloseTween(Action onComplete)
    {
        AudioManager.Instance.PlaySFX("µã»÷ÒôÐ§");
        return transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InCubic).OnComplete(() => onComplete());
    }

    protected void CloseSelf()
    {
        UIManager.instance.Close(this);
    }

    protected virtual void OnOpen(object param) { }
    protected virtual void OnClose() { }
}

public enum UILayer
{
    Normal = 0,
    Popup = 1,
    System = 2
}