namespace NarutoAutoBattle.Economy
{
    public struct RoundIncomeBreakdown
    {
        public int BaseGold;
        public int InterestGold;
        public int StreakGold;
        public int TotalGold;

        public static RoundIncomeBreakdown Empty => default;

        public override string ToString()
        {
            if (TotalGold <= 0)
                return string.Empty;

            return $"+{TotalGold}g ({BaseGold} base + {InterestGold} interest + {StreakGold} streak)";
        }
    }
}
