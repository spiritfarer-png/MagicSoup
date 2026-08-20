using System.Text.RegularExpressions;

public static class TooltipText
{
    private static readonly (string text, string color)[] Keywords =
    {
        ("额外行动", "#FFD166"), ("受到伤害", "#FF6B6B"), ("所有敌方", "#FF8A80"), ("所有友方", "#80CBC4"),
        ("伤害", "#FF6B6B"), ("攻击", "#FF6B6B"), ("防御", "#64B5F6"), ("治疗", "#81C784"), ("恢复", "#81C784"),
        ("生命", "#81C784"), ("遗物", "#FFB74D"), ("药水", "#CE93D8"), ("素材", "#FFCC80"), ("卡牌", "#90CAF9"),
        ("意图", "#FFD54F"), ("计时器", "#FFD54F")
    };

    public static string Format(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        text = Regex.Replace(text, @"-?\d+", match => $"<color=#FFF176>{match.Value}</color>");
        foreach (var keyword in Keywords) text = text.Replace(keyword.text, $"<color={keyword.color}>{keyword.text}</color>");
        return text;
    }
}
