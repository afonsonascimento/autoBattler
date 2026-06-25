using System;
using UnityEngine;

namespace NarutoAutoBattle.Economy
{
    public class PlayerEconomy : MonoBehaviour
    {
        [SerializeField] PlayerGold playerGold;
        [SerializeField] EconomyRules rules;

        int xp;
        int level = 1;
        int winStreak;
        int lossStreak;
        RoundIncomeBreakdown lastIncome = RoundIncomeBreakdown.Empty;

        public EconomyRules Rules => rules;
        public int Level => level;
        public int Xp => xp;
        public int WinStreak => winStreak;
        public int LossStreak => lossStreak;
        public int PredictedInterest => rules != null
            ? rules.CalculateInterest(playerGold != null ? playerGold.Gold : 0)
            : 0;
        public RoundIncomeBreakdown LastIncome => lastIncome;

        public string GetLevelProgressText()
        {
            if (rules == null)
                return $"Lv.{level}";

            if (level >= rules.MaxLevel)
                return $"Lv.{level} MAX";

            return $"Lv.{level} ({xp}/{rules.GetXpForNextLevel(level)} XP)";
        }

        public string GetStreakText()
        {
            if (rules == null)
                return string.Empty;

            if (winStreak > 0)
                return $"Win streak {winStreak} (+{rules.CalculateStreakBonus(winStreak, 0)}g)";
            if (lossStreak > 0)
                return $"Loss streak {lossStreak} (+{rules.CalculateStreakBonus(0, lossStreak)}g)";
            return string.Empty;
        }

        public event Action OnStateChanged;

        public void ResetRun()
        {
            xp = 0;
            level = 1;
            winStreak = 0;
            lossStreak = 0;
            lastIncome = RoundIncomeBreakdown.Empty;
            OnStateChanged?.Invoke();
        }

        public void RegisterCombatResult(bool playerWon)
        {
            if (playerWon)
            {
                winStreak++;
                lossStreak = 0;
            }
            else
            {
                lossStreak++;
                winStreak = 0;
            }

            if (rules != null)
                AddXp(rules.xpPerRound + (playerWon ? rules.xpWinBonus : 0));
        }

        public RoundIncomeBreakdown GrantRoundIncome(int currentGold)
        {
            if (rules == null)
            {
                lastIncome = RoundIncomeBreakdown.Empty;
                return lastIncome;
            }

            int interest = rules.CalculateInterest(currentGold);
            int streak = rules.CalculateStreakBonus(winStreak, lossStreak);

            lastIncome = new RoundIncomeBreakdown
            {
                BaseGold = rules.baseRoundGold,
                InterestGold = interest,
                StreakGold = streak,
                TotalGold = rules.baseRoundGold + interest + streak
            };

            OnStateChanged?.Invoke();
            return lastIncome;
        }

        void AddXp(int amount)
        {
            if (rules == null || level >= rules.MaxLevel)
                return;

            xp += amount;
            int newLevel = rules.GetLevelFromXp(xp);
            if (newLevel != level)
            {
                level = newLevel;
                Debug.Log($"Player level up! Now level {level}.");
            }

            OnStateChanged?.Invoke();
        }
    }
}
