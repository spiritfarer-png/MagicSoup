using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GuideUI : UIView
{
    public static GuideUI Instance { get; private set; }

    public Button Left;
    public Button Right;
    public Button Quit;
    public List<GameObject> GuidePages;

    private Tween currentTween;
    private int currentIndex = 0;

    private void Awake() { Instance = this; }

    private void Start()
    {
        Left.onClick.AddListener(OnLeftClick);
        Right.onClick.AddListener(OnRightClick);
        Quit.onClick.AddListener(OnQuitClick);

        InitializePages();
    }

    protected override void OnOpen(object param)
    {
        PlayOpenTween();
    }



    //只显示第0页 其余隐藏
    private void InitializePages()
    {
        if (GuidePages == null || GuidePages.Count == 0) return;

        for (int i = 0; i < GuidePages.Count; i++)
        {
            var cg = GetOrAddCanvasGroup(GuidePages[i]);
            cg.alpha = (i == 0) ? 1f : 0f;
            cg.blocksRaycasts = (i == 0);
            GuidePages[i].SetActive(i == 0);
        }

        currentIndex = 0;
        UpdateButtonState();
    }

    //上一页
   private void OnLeftClick()
    {
        AudioManager.Instance.PlaySFX("点击音效");
        if (currentIndex > 0)
        {
            SwitchPage(currentIndex - 1);
        }
    }
    //下一页
    private void OnRightClick()
    {
        AudioManager.Instance.PlaySFX("点击音效");
        if (currentIndex < GuidePages.Count - 1)
        {
            SwitchPage(currentIndex + 1);
        }
    }
    private Sequence switchSequence;
    private void SwitchPage(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= GuidePages.Count) return;

        currentTween?.Kill();

        var oldPage = GuidePages[currentIndex];
        var newPage = GuidePages[targetIndex];

        var oldCg = GetOrAddCanvasGroup(oldPage);
        var newCg = GetOrAddCanvasGroup(newPage);

        // 准备新页面
        newPage.SetActive(true);
        newCg.alpha = 0f;
        oldCg.blocksRaycasts = false;
        newCg.blocksRaycasts = true;

        // 执行淡出淡入序列
        currentTween = DOTween.Sequence()
            .Append(oldCg.DOFade(0f, 0.2f))
            .AppendCallback(() => oldPage.SetActive(false))
            .Append(newCg.DOFade(1f, 0.2f))
            .OnComplete(() =>
            {
                currentIndex = targetIndex;
                UpdateButtonState();
            });
    }

    private void UpdateButtonState()
    {
        if (Left != null) Left.interactable = (currentIndex > 0);
        if (Right != null) Right.interactable = (currentIndex < GuidePages.Count - 1);
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }
        return cg;
    }

    private void OnQuitClick()
    {
        AudioManager.Instance.PlaySFX("点击音效");
        PlayCloseTween(() => { CloseSelf(); });
    }
}
