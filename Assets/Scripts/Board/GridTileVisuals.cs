using System.Collections.Generic;
using NarutoAutoBattle.Core;
using UnityEngine;

namespace NarutoAutoBattle.Board
{
    public static class GridTileVisuals
    {
        public static Transform BuildTiles(
            Transform parent,
            IEnumerable<GridSlot> slots,
            float cellSize,
            Color color,
            string containerName)
        {
            var container = new GameObject(containerName);
            container.transform.SetParent(parent, false);

            var sharedMaterial = CreateTileMaterial(color);

            foreach (var slot in slots)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = $"Tile_{slot.Coordinates.x}_{slot.Coordinates.y}";
                tile.transform.SetParent(container.transform, false);
                tile.transform.position = new Vector3(slot.WorldPosition.x, 0.02f, slot.WorldPosition.z);
                tile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                tile.transform.localScale = new Vector3(cellSize * 0.92f, cellSize * 0.92f, 1f);

                Object.Destroy(tile.GetComponent<Collider>());
                tile.GetComponent<Renderer>().sharedMaterial = sharedMaterial;
            }

            return container.transform;
        }

        static Material CreateTileMaterial(Color color)
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null)
                shader = Shader.Find("Standard");

            var material = new Material(shader);
            material.color = color;
            return material;
        }

        public static Color ColorForZone(GridZone zone)
        {
            return zone switch
            {
                GridZone.PlayerBoard => new Color(0.15f, 0.45f, 0.85f),
                GridZone.EnemyBoard => new Color(0.85f, 0.25f, 0.25f),
                GridZone.Bench => new Color(0.25f, 0.75f, 0.35f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }
    }
}
