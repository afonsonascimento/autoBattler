using NarutoAutoBattle.Core;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Board
{
    public class GridSlot
    {
        public Vector2Int Coordinates { get; }
        public Vector3 WorldPosition { get; }
        public GridZone Zone { get; }
        public Unit Occupant { get; set; }

        public bool IsEmpty => Occupant == null;

        public GridSlot(Vector2Int coordinates, Vector3 worldPosition, GridZone zone)
        {
            Coordinates = coordinates;
            WorldPosition = worldPosition;
            Zone = zone;
        }
    }
}
