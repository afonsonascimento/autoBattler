using System.Collections.Generic;
using NarutoAutoBattle.Board;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Synergies
{
    public class SynergyService : MonoBehaviour
    {
        [SerializeField] BoardGrid boardGrid;
        [SerializeField] BenchGrid benchGrid;

        public List<ActiveSynergy> GetPlayerActiveSynergies(bool includeBench = false)
        {
            return SynergyCalculator.CalculateActiveSynergies(GetPlayerUnits(includeBench));
        }

        public SynergyBonuses GetPlayerBonuses(bool includeBench = false)
        {
            return SynergyCalculator.CalculateBonuses(GetPlayerUnits(includeBench));
        }

        public SynergyBonuses GetBonusesForOwner(UnitOwner owner)
        {
            if (owner == UnitOwner.Player)
                return GetPlayerBonuses(includeBench: false);

            return SynergyCalculator.CalculateBonuses(boardGrid.GetEnemyBoardUnits());
        }

        IEnumerable<Unit> GetPlayerUnits(bool includeBench)
        {
            foreach (var unit in boardGrid.GetPlayerBoardUnits())
                yield return unit;

            if (includeBench)
            {
                foreach (var unit in benchGrid.GetAllUnits())
                    yield return unit;
            }
        }
    }
}
