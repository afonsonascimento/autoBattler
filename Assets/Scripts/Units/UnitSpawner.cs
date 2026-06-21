using NarutoAutoBattle.Board;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Units
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] GameObject unitPrefab;

        public GameObject UnitPrefab => unitPrefab;

        public Unit Spawn(UnitData data, GridSlot slot, UnitOwner owner, IGridContainer container)
        {
            var go = Instantiate(unitPrefab, slot.WorldPosition, Quaternion.identity);
            var unit = go.GetComponent<Unit>();
            unit.Initialize(data, owner);
            container.TryPlaceUnit(unit, slot, swap: false);
            return unit;
        }
    }
}
