using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class TooltipView : UIView
{
    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] private Vector2 mouseOffset = new Vector2(20f, -20f);
    public ITooltipSource source { get { return tipSource; } }
    private ITooltipSource tipSource;
    private Canvas canvas;
    private RectTransform rectTransform;
    private RectTransform parentRect;


    private void Awake()
    {
        parentRect = (RectTransform)transform.parent;
        rectTransform = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
    }
    protected override void OnOpen(object param)
    {
        tipSource = param as ITooltipSource;
        string text = tipSource?.GetToolTip();
        if (string.IsNullOrEmpty(text))
        {
            CloseSelf();
            return;
        }
        tmp.text = text;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        UpdatePosition();
    }

    private void LateUpdate()
    {
        if (IsOpen) UpdatePosition();
    }

    private void UpdatePosition()
    {
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Input.mousePosition, uiCamera, out Vector2 position)) return;

        position += mouseOffset;

        Vector2 min = parentRect.rect.min - rectTransform.rect.min;
        Vector2 max = parentRect.rect.max - rectTransform.rect.max;
        position.x = Mathf.Clamp(position.x, min.x, max.x);
        position.y = Mathf.Clamp(position.y, min.y, max.y);

        rectTransform.localPosition = position;
    }
}
