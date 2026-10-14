using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>Runtime ownership link between an instantiated unit and its map cell.</summary>
    public sealed class DeployedUnit : MonoBehaviour
    {
        public MapCell Cell { get; private set; }
        public UnitDeploymentData Data { get; private set; }
        public Vector2Int Facing { get; private set; }

        public void Initialize(MapCell cell, UnitDeploymentData data, Vector2Int facing)
        {
            Cell = cell;
            Data = data;
            SetFacing(facing);

            StaffMember staffMember = GetComponent<StaffMember>();
            if (staffMember != null)
                staffMember.Initialize(data);
        }

        public void SetFacing(Vector2Int facing)
        {
            if (facing == Vector2Int.zero)
                facing = Vector2Int.up;

            Facing = facing;
            transform.up = new Vector3(facing.x, facing.y, 0f);
        }

        private void OnDestroy()
        {
            if (Cell != null)
                Cell.ClearOccupant(this);
        }
    }
}
