using System.Collections.Generic;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Board
{
    public class BenchGrid : MonoBehaviour, IGridContainer
    {
        [SerializeField] float cellSize = 1.2f;
        [SerializeField] float benchZ = -2.4f;
        [SerializeField] int slotCount = 8;
        [SerializeField] bool showTiles = true;

        readonly List<GridSlot> slots = new();

        public float CellSize => cellSize;
        public IReadOnlyList<GridSlot> Slots => slots;

        void Awake()
        {
            BuildBench();

            if (showTiles)
            {
                GridTileVisuals.BuildTiles(transform, slots, cellSize,
                    GridTileVisuals.ColorForZone(GridZone.Bench), "BenchTiles");
            }
        }

        void BuildBench()
        {
            slots.Clear();
            float startX = -(slotCount - 1) * cellSize * 0.5f;

            for (int i = 0; i < slotCount; i++)
            {
                var coords = new Vector2Int(i, 0);
                var worldPos = new Vector3(startX + i * cellSize, 0.8f, benchZ);
                slots.Add(new GridSlot(coords, worldPos, GridZone.Bench));
            }
        }

        public bool TryPlaceUnit(Unit unit, GridSlot slot, bool swap = true)
        {
            if (slot == null || slot.Zone != GridZone.Bench)
                return false;

            if (unit.Owner != UnitOwner.Player)
                return false;

            if (unit.CurrentSlot != null)
                unit.CurrentSlot.Occupant = null;

            if (!slot.IsEmpty && swap)
            {
                var other = slot.Occupant;
                slot.Occupant = unit;
                unit.SetSlot(slot);

                if (unit.PreviousSlot != null)
                {
                    unit.PreviousSlot.Occupant = other;
                    other?.SetSlot(unit.PreviousSlot);
                }

                return true;
            }

            if (!slot.IsEmpty)
                return false;

            slot.Occupant = unit;
            unit.SetSlot(slot);
            unit.transform.position = slot.WorldPosition;
            return true;
        }

        public bool TryRemoveUnit(Unit unit)
        {
            if (unit.CurrentSlot == null)
                return false;

            unit.CurrentSlot.Occupant = null;
            unit.SetSlot(null);
            return true;
        }

        public GridSlot GetSlotAtWorldPosition(Vector3 worldPosition)
        {
            GridSlot closest = null;
            float closestDist = float.MaxValue;

            foreach (var slot in slots)
            {
                float dist = Vector3.SqrMagnitude(slot.WorldPosition - worldPosition);
                if (dist < closestDist && dist < cellSize * cellSize * 0.5f)
                {
                    closestDist = dist;
                    closest = slot;
                }
            }

            return closest;
        }

        public GridSlot GetFirstEmptySlot()
        {
            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                    return slot;
            }

            return null;
        }

        public IEnumerable<Unit> GetAllUnits()
        {
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty)
                    yield return slot.Occupant;
            }
        }

        public IEnumerable<GridSlot> GetAllSlots()
        {
            foreach (var slot in slots)
                yield return slot;
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                BuildBench();

            Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.3f);
            foreach (var slot in slots)
                Gizmos.DrawCube(slot.WorldPosition, new Vector3(cellSize * 0.9f, 0.05f, cellSize * 0.9f));
        }
    }
}
