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
        [SerializeField] private Vector2Int coordinates;
        [SerializeField] private CellTerrain terrain = CellTerrain.Ground;
        [SerializeField] private bool allowDeployment = true;

        public Vector2Int Coordinates { get { return coordinates; } }
        public CellTerrain Terrain { get { return terrain; } }
        public bool AllowDeployment { get { return allowDeployment; } }
        public DeployedUnit Occupant { get; private set; }
        public bool IsOccupied { get { return Occupant != null; } }

        private void OnValidate()
        {
            if (terrain == CellTerrain.Blocked || terrain == CellTerrain.Route)
                allowDeployment = false;
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
    }
}
