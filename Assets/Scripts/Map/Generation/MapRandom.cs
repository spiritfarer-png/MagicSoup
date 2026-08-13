using System;
using System.Collections.Generic;

/// <summary>
/// 地图生成专用的确定性随机数工具。
/// 相同的种子和相同的调用顺序会得到相同结果。
/// </summary>
public sealed class MapRandom
{
    private uint state;
    public MapRandom(int seed)
    {
        state = unchecked((uint)seed);
        if (state == 0)
        {
            state = 0x6D2B79F5u;
        }
    }

    /// <summary>
    /// 返回 [minInclusive, maxInclusive] 范围内的整数
    /// </summary>
    public int NextIntInclusive(int minInclusive, int maxInclusive)
    {
        if (maxInclusive < minInclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maxInclusive), "最大值不能小于最小值。");
        }

        if (minInclusive == maxInclusive)
        {
            return minInclusive;
        }

        uint range = (uint)(maxInclusive - minInclusive + 1);

        return minInclusive + (int)(NextUInt() % range);
    }

    /// <summary>
    /// 返回 [0, 1) 范围内的浮点数。
    /// </summary>
    public float NextFloat()
    {
        return (NextUInt() >> 8) * (1f / 16777216f);
    }

    /// <summary>
    /// 根据权重列表返回被抽中的下标，权重允许为 0，但权重总和必须大于 0
    /// </summary>
    public int WeightedIndex(IReadOnlyList<float> weights)
    {
        if (weights == null)
        {
            throw new ArgumentNullException(nameof(weights));
        }

        if (weights.Count == 0)
        {
            throw new ArgumentException("权重列表不能为空。", nameof(weights));
        }

        float totalWeight = 0f;

        for (int i = 0; i < weights.Count; i++)
        {
            if (weights[i] < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(weights), "权重不能小于 0。");
            }

            totalWeight += weights[i];
        }

        if (totalWeight <= 0f)
        {
            throw new ArgumentException("权重总和必须大于 0。", nameof(weights));
        }

        float value = NextFloat() * totalWeight;
        float cumulativeWeight = 0f;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulativeWeight += weights[i];

            if (value < cumulativeWeight)
            {
                return i;
            }
        }

        // 处理浮点数累计误差。
        return weights.Count - 1;
    }

    /// <summary>
    /// 使用当前随机源原地打乱列表。
    /// </summary>
    public void Shuffle<T>(IList<T> list)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        // Fisher-Yates 洗牌。
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = NextIntInclusive(0, i);

            T value = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = value;
        }
    }

    /// <summary>
    /// 生成下一个无符号整数。
    /// 使用 Xorshift32 算法。
    /// </summary>
    private uint NextUInt()
    {
        uint value = state;

        value ^= value << 13;
        value ^= value >> 17;
        value ^= value << 5;

        state = value;
        return value;
    }
}