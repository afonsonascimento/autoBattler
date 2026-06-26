using System;
using UnityEngine;

namespace NarutoAutoBattle.Economy
{
    [CreateAssetMenu(fileName = "EconomyRules", menuName = "Naruto Auto Battle/Economy Rules")]
    public class EconomyRules : ScriptableObject
    {
        [Serializable]
        public struct StreakTier
        {
            [Min(1)] public int minStreak;
            [Min(0)] public int bonusGold;
        }

        [Serializable]
        public struct ShopOdds
        {
            [Range(0f, 1f)] public float cost1;
            [Range(0f, 1f)] public float cost2;
            [Range(0f, 1f)] public float cost3;
            [Range(0f, 1f)] public float cost4;

            public float[] ToArray() => new[] { cost1, cost2, cost3, cost4 };
        }

        public const int MaxShopCost = 4;

        [Header("Round Income")]
        [Min(0)] public int baseRoundGold = 5;

        [Header("Interest")]
        [Min(1)] public int interestGoldStep = 10;
        [Min(0)] public int interestPerStep = 1;
        [Min(0)] public int maxInterest = 5;
        [Min(0)] public int interestGoldCap = 50;

        [Header("Experience")]
        [Min(0)] public int xpPerRound = 2;
        [Min(0)] public int xpWinBonus = 2;
        [Tooltip("Total XP required for each level. Element 0 = level 1 (0 XP), element 1 = level 2, etc.")]
        public int[] totalXpForLevel = { 0, 2, 6, 10, 20, 36, 56 };

        [Header("Streak Bonus")]
        [Tooltip("Highest matching minStreak tier wins. Sorted automatically in the inspector.")]
        public StreakTier[] streakTiers =
        {
            new() { minStreak = 1, bonusGold = 1 },
            new() { minStreak = 3, bonusGold = 2 },
            new() { minStreak = 5, bonusGold = 3 }
        };

        [Header("Shop Odds By Level")]
        [Tooltip("One entry per player level. Level 1 uses index 0.")]
        public ShopOdds[] shopOddsByLevel =
        {
            new() { cost1 = 1f, cost2 = 0f, cost3 = 0f, cost4 = 0f },
            new() { cost1 = 0.75f, cost2 = 0.25f, cost3 = 0f, cost4 = 0f },
            new() { cost1 = 0.55f, cost2 = 0.30f, cost3 = 0.15f, cost4 = 0f },
            new() { cost1 = 0.45f, cost2 = 0.33f, cost3 = 0.22f, cost4 = 0f },
            new() { cost1 = 0.30f, cost2 = 0.38f, cost3 = 0.27f, cost4 = 0.05f },
            new() { cost1 = 0.22f, cost2 = 0.32f, cost3 = 0.35f, cost4 = 0.11f },
            new() { cost1 = 0.17f, cost2 = 0.27f, cost3 = 0.36f, cost4 = 0.20f }
        };

        public int MaxLevel => shopOddsByLevel != null && shopOddsByLevel.Length > 0
            ? shopOddsByLevel.Length
            : 1;

        public int CalculateInterest(int gold)
        {
            int step = Mathf.Max(1, interestGoldStep);
            int eligibleGold = Mathf.Min(gold, interestGoldCap);
            int interest = eligibleGold / step * interestPerStep;
            return Mathf.Min(maxInterest, interest);
        }

        public int CalculateStreakBonus(int winStreak, int lossStreak)
        {
            int streak = Mathf.Max(winStreak, lossStreak);
            if (streak <= 0 || streakTiers == null || streakTiers.Length == 0)
                return 0;

            int bestBonus = 0;
            foreach (var tier in streakTiers)
            {
                if (streak >= tier.minStreak && tier.bonusGold > bestBonus)
                    bestBonus = tier.bonusGold;
            }

            return bestBonus;
        }

        public int GetLevelFromXp(int xp)
        {
            if (totalXpForLevel == null || totalXpForLevel.Length == 0)
                return 1;

            int result = 1;
            for (int i = 1; i < totalXpForLevel.Length; i++)
            {
                if (xp >= totalXpForLevel[i])
                    result = i + 1;
            }

            return Mathf.Min(result, MaxLevel);
        }

        public int GetXpForNextLevel(int level)
        {
            if (totalXpForLevel == null || totalXpForLevel.Length == 0)
                return 0;

            if (level >= MaxLevel)
                return totalXpForLevel[Mathf.Min(MaxLevel - 1, totalXpForLevel.Length - 1)];

            int index = Mathf.Clamp(level, 0, totalXpForLevel.Length - 1);
            return totalXpForLevel[index];
        }

        public float[] GetShopOdds(int level)
        {
            if (shopOddsByLevel == null || shopOddsByLevel.Length == 0)
                return new[] { 1f, 0f, 0f, 0f };

            int index = Mathf.Clamp(level - 1, 0, shopOddsByLevel.Length - 1);
            return shopOddsByLevel[index].ToArray();
        }

        public int RollShopCost(int playerLevel)
        {
            var odds = GetShopOdds(playerLevel);
            float roll = UnityEngine.Random.value;
            float cumulative = 0f;

            for (int i = 0; i < odds.Length; i++)
            {
                cumulative += odds[i];
                if (roll <= cumulative)
                    return i + 1;
            }

            return 1;
        }

        void OnValidate()
        {
            if (interestGoldStep < 1)
                interestGoldStep = 1;

            if (totalXpForLevel != null && totalXpForLevel.Length > 0)
                totalXpForLevel[0] = 0;

            if (streakTiers != null && streakTiers.Length > 1)
                Array.Sort(streakTiers, (a, b) => a.minStreak.CompareTo(b.minStreak));
        }
    }
}
