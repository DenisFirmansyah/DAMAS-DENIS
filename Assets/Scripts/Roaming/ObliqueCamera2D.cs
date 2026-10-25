using UnityEngine;

namespace DamasDenis.Roaming
{
    /// <summary>
    /// Orthographic camera with an oblique (sheared) projection, plus a smooth follow.
    /// Heights in the 3D map are pushed sideways/upwards on screen, which gives the
    /// "2D game with depth" look while keeping sprites pixel-sharp.
    /// Runs in the editor too, so the angle sliders update the Game view live.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public sealed class ObliqueCamera2D : MonoBehaviour
    {

        [Header("Follow")]
        [SerializeField] private Transform target;
        [Tooltip("Camera position relative to the target, in world units.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 10f, 0f);
        [Tooltip("Seconds to catch up. 0 = locked to the target.")]
        [Min(0f)] [SerializeField] private float smoothTime = 0.15f;

        private Camera cam;
        private Vector3 velocity;

        /// <summary>The transform the camera follows. Can be swapped at runtime.</summary>
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void OnEnable()
        {
            cam = GetComponent<Camera>();
        }

        private void OnDisable()
        {
            // Hand the projection back to Unity so other scenes' cameras aren't left sheared.
            if (cam != null) cam.ResetProjectionMatrix();
        }

        private void LateUpdate()
        {
            // LateUpdate so we move after the character has moved this frame (no jitter).
            if (Application.isPlaying) Follow();
        }

        private void Follow()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            transform.position = smoothTime <= 0f
                ? desired
                : Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }
    }
}