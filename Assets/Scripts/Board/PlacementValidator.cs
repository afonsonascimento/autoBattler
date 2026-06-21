using NarutoAutoBattle.Core;
using NarutoAutoBattle.Units;

namespace NarutoAutoBattle.Board
{
    public static class PlacementValidator
    {
        public static bool CanPlayerDrag(Unit unit)
        {
            return unit != null && unit.Owner == UnitOwner.Player && !unit.IsInCombat;
        }

        public static bool CanDropOnSlot(Unit unit, GridSlot slot)
        {
            if (unit == null || slot == null)
                return false;

            if (slot.Zone == GridZone.Bench)
                return unit.Owner == UnitOwner.Player;

            if (slot.Zone == GridZone.PlayerBoard)
                return unit.Owner == UnitOwner.Player;

            return false;
        }
    }
}
