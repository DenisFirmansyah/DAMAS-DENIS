using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Draws a flat marker on every tile an attack pattern covers. Each owner (placement,
    /// selected unit) uses its own instance, so they never overwrite each other.
    /// </summary>
    public sealed class RangePreview : MonoBehaviour
    {
        [Tooltip("Flat marker with no collider, e.g. a Quad rotated (90, 0, 0).")]
        [SerializeField] private GameObject tilePrefab;
        [Tooltip("Lift above the tile's top face so the marker doesn't flicker (z-fighting).")]
        [Min(0f)] [SerializeField] private float lift = 0.02f;

        private readonly List<GameObject> tiles = new List<GameObject>();
        private readonly List<Vector2Int> coordinates = new List<Vector2Int>();
        private int visible;

        /// <summary>Marks the tiles a pattern covers from an origin tile and facing. Tiles off the board are skipped.</summary>
        public void Show(AttackRangePattern pattern, Vector2Int origin, Vector2Int facing)
        {
            int used = 0;

            if (pattern != null && tilePrefab != null)
            {
                pattern.GetCoordinates(origin, facing, coordinates);
                for (int i = 0; i < coordinates.Count; i++)
                {
                    if (!BoardGrid.TryGetCell(coordinates[i], out MapCell cell)) continue;

                    if (used == tiles.Count)
                        tiles.Add(Instantiate(tilePrefab, transform));

                    GameObject tile = tiles[used++];
                    tile.transform.position = cell.SurfacePoint + Vector3.up * lift;
                    if (!tile.activeSelf) tile.SetActive(true);
                }
            }

            HideFrom(used);
        }

        /// <summary>Hides every marker. Cheap to call every frame.</summary>
        public void Hide() => HideFrom(0);

        private void HideFrom(int first)
        {
            for (int i = first; i < visible; i++)
                tiles[i].SetActive(false);
            visible = first;
        }
    }
}