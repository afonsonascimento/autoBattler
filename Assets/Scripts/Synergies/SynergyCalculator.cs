using System.Collections.Generic;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Synergies
{
    public struct SynergyBonuses
    {
        public float AttackMultiplier;
        public float HealthMultiplier;

        public static SynergyBonuses Identity => new() { AttackMultiplier = 1f, HealthMultiplier = 1f };

        public static SynergyBonuses Combine(SynergyBonuses a, SynergyBonuses b)
        {
            return new SynergyBonuses
            {
                AttackMultiplier = a.AttackMultiplier + (b.AttackMultiplier - 1f),
                HealthMultiplier = a.HealthMultiplier + (b.HealthMultiplier - 1f)
            };
        }
    }

    public struct ActiveSynergy
    {
        public ClanData Clan;
        public int Count;
        public int ActiveThreshold;
        public float AttackBonus;
        public float HealthBonus;
    }

    public static class SynergyCalculator
    {
        public static SynergyBonuses CalculateBonuses(IEnumerable<Unit> units)
        {
            var active = CalculateActiveSynergies(units);
            var bonuses = SynergyBonuses.Identity;

            foreach (var synergy in active)
            {
                bonuses = SynergyBonuses.Combine(bonuses, new SynergyBonuses
                {
                    AttackMultiplier = 1f + synergy.AttackBonus,
                    HealthMultiplier = 1f + synergy.HealthBonus
                });
            }

            return bonuses;
        }

        public static List<ActiveSynergy> CalculateActiveSynergies(IEnumerable<Unit> units)
        {
            var clanUniqueUnits = new Dictionary<ClanData, HashSet<UnitData>>();

            foreach (var unit in units)
            {
                if (unit?.Data?.clans == null)
                    continue;

                foreach (var clan in unit.Data.clans)
                {
                    if (clan == null)
                        continue;

                    if (!clanUniqueUnits.TryGetValue(clan, out var uniqueUnits))
                    {
                        uniqueUnits = new HashSet<UnitData>();
                        clanUniqueUnits[clan] = uniqueUnits;
                    }

                    uniqueUnits.Add(unit.Data);
                }
            }

            var active = new List<ActiveSynergy>();

            foreach (var pair in clanUniqueUnits)
            {
                var clan = pair.Key;
                int count = pair.Value.Count;

                if (clan.thresholds == null || clan.thresholds.Length == 0)
                    continue;

                ClanData.Threshold best = default;
                bool found = false;

                foreach (var threshold in clan.thresholds)
                {
                    if (count >= threshold.unitCount && threshold.unitCount >= best.unitCount)
                    {
                        best = threshold;
                        found = true;
                    }
                }

                if (!found)
                    continue;

                active.Add(new ActiveSynergy
                {
                    Clan = clan,
                    Count = count,
                    ActiveThreshold = best.unitCount,
                    AttackBonus = best.attackBonus,
                    HealthBonus = best.healthBonus
                });
            }

            active.Sort((a, b) => string.Compare(a.Clan.displayName, b.Clan.displayName, System.StringComparison.Ordinal));
            return active;
        }
    }
}
