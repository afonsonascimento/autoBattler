using System.Collections.Generic;
using System.Linq;
using NarutoAutoBattle.Board;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Units
{
    public class UnitMergeService : MonoBehaviour
    {
        public static UnitMergeService Instance { get; private set; }

        [SerializeField] BoardGrid boardGrid;
        [SerializeField] BenchGrid benchGrid;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void ProcessMerges()
        {
            bool merged;
            do
            {
                merged = TryMergeOnce();
            } while (merged);
        }

        bool TryMergeOnce()
        {
            var units = CollectPlayerUnits();
            var groups = units
                .Where(u => u.StarLevel < 3)
                .GroupBy(u => (u.Data, u.StarLevel))
                .Where(g => g.Count() >= 3)
                .ToList();

            if (groups.Count == 0)
                return false;

            var group = groups[0].ToList();
            var keeper = group[0];
            var toRemove = group.Skip(1).Take(2).ToList();

            foreach (var unit in toRemove)
            {
                if (unit.CurrentSlot != null)
                    unit.CurrentSlot.Occupant = null;
                Destroy(unit.gameObject);
            }

            keeper.SetStarLevel(keeper.StarLevel + 1);
            Debug.Log($"Merged into {keeper.Data.displayName} ★{keeper.StarLevel}!");
            return true;
        }

        List<Unit> CollectPlayerUnits()
        {
            var units = new List<Unit>();
            units.AddRange(boardGrid.GetAllUnits().Where(u => u.Owner == Core.UnitOwner.Player));
            units.AddRange(benchGrid.GetAllUnits());
            return units;
        }
    }
}
