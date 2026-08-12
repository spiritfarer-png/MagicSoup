using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using TMPro;
using UnityEngine;

public class CardEntity : MonoBehaviour
{
    public CardInfo cardInfo;
    [SerializeField] private SpriteRenderer soup;
    [SerializeField] private SpriteRenderer[] materialIcons;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI nameText;
    private void Start()
    {
        cardInfo.Initialize();
        InitializeVisual();
    }

    public void InitializeVisual()
    {
        var materials = cardInfo.materialInfoArray;
        if (materials == null || materials.Length == 0) return;

        Color soupColor = Color.clear;
        for (int i = 0; i < materials.Length; i++)
        {
            soupColor += materials[i].Color;
            materialIcons[i].sprite = materials[i].Icon;
        }

        soupColor /= materials.Length;
        soupColor.a = 1f;
        soup.color = soupColor;

        healthText.text = cardInfo.currentHealth.ToString();
        attackText.text = cardInfo.currentAttack.ToString();
        nameText.text = cardInfo.CardName;
    }

    public void UpdateVisual()
    {
        healthText.text = cardInfo.currentHealth.ToString();
        attackText.text = cardInfo.currentAttack.ToString();
    }
}

