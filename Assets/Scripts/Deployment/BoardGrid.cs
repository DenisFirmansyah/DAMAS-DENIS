using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Lookup from board coordinates to MapCell. Cells register themselves when they load,
    /// which works across scenes (tiles in the stage scene, units in the gameplay scene).
    /// </summary>
    /// 
    
    public static class BoardGrid
    {
        private static readonly Dictionary<Vector2Int, MapCell> cells = new Dictionary<Vector2Int, MapCell>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => cells.Clear();

        /// <summary>Distance between neighboring cell centers, in world units.</summary>
        public static float CellSize { get; set; } = 1f;

        /// <summary>Converts a world position to board coordinates (X and Z only).</summary>
        public static Vector2Int WorldToCoordinates(Vector3 world)
        {
            return new Vector2Int(
                Mathf.RoundToInt(world.x / CellSize),
                Mathf.RoundToInt(world.z / CellSize));
        }

        /// <summary>Grid coordinates to world position on the XZ plane. Height (Y) is supplied by the caller.</summary>
        public static Vector3 CoordinatesToWorld(Vector2Int c, float height = 0f)
        {
            return new Vector3(c.x * CellSize, height, c.y * CellSize);
        }

        internal static void Register(MapCell cell, Vector2Int key)
        {
            if (cells.TryGetValue(key, out MapCell existing) && existing != null && existing != cell)
            {
                Debug.LogWarning($"Two MapCells share coordinates {key}: '{existing.name}' and '{cell.name}'.", cell);
            }
            cells[key] = cell;
        }

        internal static void Unregister(MapCell cell, Vector2Int key)
        {
            if (cells.TryGetValue(key, out MapCell existing) && existing == cell)
                cells.Remove(key);
        }

        /// <summary>Finds the cell at the given coordinates. False if the board has no tile there.</summary>
        public static bool TryGetCell(Vector2Int coordinates, out MapCell cell)
        {
            if (cells.TryGetValue(coordinates, out cell) && cell != null)
                return true;

            cell = null;
            return false;
        }
    }
}