using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>Runtime ownership link between an instantiated unit and its map cell.</summary>
    public sealed class DeployedUnit : MonoBehaviour
    {
        public MapCell Cell { get; private set; }
        public UnitDeploymentData Data { get; private set; }
        public Vector2Int Facing { get; private set; }
        public Vector2Int EffectiveFacing => (Data != null && !Data.CanFace) ? Vector2Int.up : Facing;

        public void Initialize(MapCell cell, UnitDeploymentData data, Vector2Int facing)
        {
            Cell = cell;
            Data = data;
            SetFacing(facing);

            if (data != null && data.BlockCount > 0)
            {
                StaffBlocker blocker = GetComponent<StaffBlocker>();
                if (blocker == null) blocker = gameObject.AddComponent<StaffBlocker>();
                blocker.Initialize(data.BlockCount);
            }

            StaffMember staffMember = GetComponent<StaffMember>();
            if (staffMember != null)
                staffMember.Initialize(data);

            StaffCombat staffCombat = GetComponent<StaffCombat>();
            if (staffCombat != null)
                staffCombat.Initialize(data);
        }

        public void SetFacing(Vector2Int facing)
        {
            if (facing == Vector2Int.zero)
                facing = Vector2Int.up;

            Facing = facing;
            // Hybrid 3D Environment: Root rotates on XZ plane to define gameplay direction.
            // 2D Character Sprite child will handle visual billboarding.
            transform.rotation = Quaternion.LookRotation(new Vector3(facing.x, 0f, facing.y), Vector3.up);
        }

        private void OnDestroy()
        {
            if (Cell != null)
                Cell.ClearOccupant(this);
        }
    }
}
