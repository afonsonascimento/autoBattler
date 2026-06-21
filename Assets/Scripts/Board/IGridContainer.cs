using System.Collections.Generic;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Board
{
    public interface IGridContainer
    {
        bool TryPlaceUnit(Unit unit, GridSlot slot, bool swap = true);
        bool TryRemoveUnit(Unit unit);
        GridSlot GetSlotAtWorldPosition(Vector3 worldPosition);
        GridSlot GetFirstEmptySlot();
        IEnumerable<Unit> GetAllUnits();
        IEnumerable<GridSlot> GetAllSlots();
    }
}
