using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Mirrors a unit's whole visual (rig, IK bones, weapons) left/right to match a board facing.
    /// Up/down facings keep the last side. Uses scale.x because Billboard owns rotation.
    /// Applied in LateUpdate so it wins over anything the Animator writes.
    /// </summary>
    public sealed class UnitVisualFlip : MonoBehaviour
    {
        [Tooltip("The object to mirror. Empty = this object. Use the parent of the rig, not the animated bones.")]
        [SerializeField] private Transform flipTarget;
        [SerializeField] private Camera targetCamera;
        [Tooltip("Tick if the original art looks to the right.")]
        [SerializeField] private bool artFacesRight = true;
        [Tooltip("How sideways a facing must be (0-1) to count as left/right. Below this, the side is kept.")]
        [Range(0.05f, 0.9f)] [SerializeField] private float sideThreshold = 0.3f;

        private bool faceLeft;

        private void Awake()
        {
            if (flipTarget == null) flipTarget = transform;
        }

        /// <summary>Board facing: x = world X, y = world Z.</summary>
        public void SetFacing(Vector2Int facing)
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;

            // Same ground flattening as Billboard / CharacterControl.
            Transform cam = targetCamera.transform;
            Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.ProjectOnPlane(cam.up, Vector3.up);
            Vector3 right = Vector3.Cross(Vector3.up, forward.normalized);

            Vector3 direction = new Vector3(facing.x, 0f, facing.y).normalized;
            float side = Vector3.Dot(direction, right);

            if (side <= -sideThreshold) faceLeft = true;
            else if (side >= sideThreshold) faceLeft = false;
            // else: facing up/down, keep the previous side
        }

        private void LateUpdate()
        {
            if (flipTarget == null) return;

            bool mirrored = artFacesRight ? faceLeft : !faceLeft;
            Vector3 s = flipTarget.localScale;
            // Abs keeps the prefab's scale size and only changes the sign.
            s.x = Mathf.Abs(s.x) * (mirrored ? -1f : 1f);
            flipTarget.localScale = s;
        }
    }
}