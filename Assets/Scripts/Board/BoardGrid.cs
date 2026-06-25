using System.Collections.Generic;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Board
{
    public class BoardGrid : MonoBehaviour, IGridContainer
    {
        [SerializeField] float cellSize = 1.2f;
        [SerializeField] float playerBoardZ = 0f;
        [SerializeField] float enemyBoardZ = 4.8f;
        [SerializeField] int columns = 4;
        [SerializeField] int rows = 3;
        [SerializeField] bool showTiles = true;

        readonly List<GridSlot> playerSlots = new();
        readonly List<GridSlot> enemySlots = new();

        public float CellSize => cellSize;
        public IReadOnlyList<GridSlot> PlayerSlots => playerSlots;
        public IReadOnlyList<GridSlot> EnemySlots => enemySlots;

        void Awake()
        {
            BuildBoard(playerBoardZ, GridZone.PlayerBoard, playerSlots);
            BuildBoard(enemyBoardZ, GridZone.EnemyBoard, enemySlots);

            if (showTiles)
            {
                GridTileVisuals.BuildTiles(transform, playerSlots, cellSize,
                    GridTileVisuals.ColorForZone(GridZone.PlayerBoard), "PlayerBoardTiles");
                GridTileVisuals.BuildTiles(transform, enemySlots, cellSize,
                    GridTileVisuals.ColorForZone(GridZone.EnemyBoard), "EnemyBoardTiles");
            }
        }

        void BuildBoard(float originZ, GridZone zone, List<GridSlot> slots)
        {
            slots.Clear();
            float startX = -(columns - 1) * cellSize * 0.5f;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    var coords = new Vector2Int(col, row);
                    var worldPos = new Vector3(startX + col * cellSize, 0.8f, originZ + row * cellSize);
                    slots.Add(new GridSlot(coords, worldPos, zone));
                }
            }
        }

        public bool TryPlaceUnit(Unit unit, GridSlot slot, bool swap = true)
        {
            if (slot == null || slot.Zone == GridZone.Bench)
                return false;

            if (unit.Owner == UnitOwner.Player && slot.Zone != GridZone.PlayerBoard)
                return false;

            if (unit.Owner == UnitOwner.Enemy && slot.Zone != GridZone.EnemyBoard)
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

            foreach (var slot in GetAllSlots())
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
            foreach (var slot in playerSlots)
            {
                if (slot.IsEmpty)
                    return slot;
            }

            return null;
        }

        public GridSlot GetFirstEmptyEnemySlot()
        {
            foreach (var slot in enemySlots)
            {
                if (slot.IsEmpty)
                    return slot;
            }

            return null;
        }

        public IEnumerable<Unit> GetAllUnits()
        {
            foreach (var slot in GetAllSlots())
            {
                if (!slot.IsEmpty)
                    yield return slot.Occupant;
            }
        }

        public IEnumerable<GridSlot> GetAllSlots()
        {
            foreach (var slot in playerSlots)
                yield return slot;
            foreach (var slot in enemySlots)
                yield return slot;
        }

        public IEnumerable<Unit> GetPlayerBoardUnits()
        {
            foreach (var slot in playerSlots)
            {
                if (!slot.IsEmpty)
                    yield return slot.Occupant;
            }
        }

        public IEnumerable<Unit> GetEnemyBoardUnits()
        {
            foreach (var slot in enemySlots)
            {
                if (!slot.IsEmpty)
                    yield return slot.Occupant;
            }
        }

        public void ClearZone(GridZone zone)
        {
            foreach (var slot in GetAllSlots())
            {
                if (slot.Zone != zone || slot.IsEmpty)
                    continue;

                Destroy(slot.Occupant.gameObject);
                slot.Occupant = null;
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                BuildBoard(playerBoardZ, GridZone.PlayerBoard, playerSlots);
                BuildBoard(enemyBoardZ, GridZone.EnemyBoard, enemySlots);
            }

            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);
            foreach (var slot in playerSlots)
                Gizmos.DrawCube(slot.WorldPosition, new Vector3(cellSize * 0.9f, 0.05f, cellSize * 0.9f));

            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
            foreach (var slot in enemySlots)
                Gizmos.DrawCube(slot.WorldPosition, new Vector3(cellSize * 0.9f, 0.05f, cellSize * 0.9f));
        }
    }
}
