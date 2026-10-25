using UnityEngine;

namespace DamasDenis.Deployment
{
    public enum CellTerrain
    {
        Ground,
        Elevated,
        Blocked,
        Route
    }

    /// <summary>One authored board tile. A cell may contain at most one deployed unit.</summary>
    [DisallowMultipleComponent]
    public sealed class MapCell : MonoBehaviour
    {
        [SerializeField] private CellTerrain terrain = CellTerrain.Ground;
        [SerializeField] private bool allowDeployment = true;
        private Collider surfaceCollider;
        private Vector2Int registeredKey;

        private void OnEnable()
        {
            registeredKey = Coordinates;
            BoardGrid.Register(this, registeredKey);
        }
        private void OnDisable() => BoardGrid.Unregister(this, registeredKey);

        public Vector2Int Coordinates => BoardGrid.WorldToCoordinates(transform.position);
        public CellTerrain Terrain { get { return terrain; } }
        public bool AllowDeployment { get { return allowDeployment; } }
        public DeployedUnit Occupant { get; private set; }
        public bool IsOccupied { get { return Occupant != null; } }

        private void OnValidate()
        {
            if (terrain == CellTerrain.Blocked || terrain == CellTerrain.Route)
                allowDeployment = false;

            Vector3 p = transform.position;
            Vector3 snapped = BoardGrid.CoordinatesToWorld(BoardGrid.WorldToCoordinates(p), p.y);
            if ((snapped - p).sqrMagnitude > 0.0001f)
                Debug.LogWarning($"{name}: tile is off the grid (position {p}). It will register at {BoardGrid.WorldToCoordinates(p)}.", this);
        }

        public bool CanHost(UnitDeploymentData unit)
        {
            return unit != null && allowDeployment && !IsOccupied && unit.CanDeployOn(terrain);
        }

        internal void SetOccupant(DeployedUnit unit)
        {
            Occupant = unit;
        }

        internal void ClearOccupant(DeployedUnit unit)
        {
            if (Occupant == unit)
                Occupant = null;
        }

        public Vector3 SurfacePoint
        {
            get
            {
                if (surfaceCollider == null)
                    surfaceCollider = GetComponentInChildren<Collider>();

                Vector3 p = transform.position;
                if (surfaceCollider != null)
                    p.y = surfaceCollider.bounds.max.y;
                return p;
            }
        }
    }
}
