using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Tiles a unit can attack, written as if the unit faces UP.
    /// X = tiles to its right, Y = tiles in front, (0,0) = its own tile.
    /// The pattern is rotated to the unit's real facing at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "RangePattern", menuName = "DAMAS DENIS/Attack Range Pattern")]
    public sealed class AttackRangePattern : ScriptableObject
    {
        [SerializeField] private List<Vector2Int> offsets = new List<Vector2Int> { new Vector2Int(0, 1) };

        /// <summary>The authored offsets (facing up).</summary>
        public IReadOnlyList<Vector2Int> Offsets => offsets;

        /// <summary>How many tiles the pattern covers.</summary>
        public int CellCount => offsets.Count;

        /// <summary>Rotates an "authored facing up" offset to the given cardinal facing.</summary>
        public static Vector2Int Rotate(Vector2Int offset, Vector2Int facing)
        {
            if (facing == Vector2Int.right) return new Vector2Int(offset.y, -offset.x);
            if (facing == Vector2Int.down)  return new Vector2Int(-offset.x, -offset.y);
            if (facing == Vector2Int.left)  return new Vector2Int(-offset.y, offset.x);
            return offset; // up, or any unexpected value
        }

        /// <summary>Fills results with the board coordinates covered from an origin tile and facing.</summary>
        public void GetCoordinates(Vector2Int origin, Vector2Int facing, List<Vector2Int> results)
        {
            results.Clear();
            for (int i = 0; i < offsets.Count; i++)
                results.Add(origin + Rotate(offsets[i], facing));
        }
    }
}