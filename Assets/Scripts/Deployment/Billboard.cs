using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>How a sprite orients itself toward the camera.</summary>
    public enum BillboardMode
    {
        /// <summary>Copies the camera rotation exactly. Sprite tilts with camera pitch.</summary>
        MatchCamera,
        /// <summary>Rotates around the world Y axis only. Sprite stays upright.</summary>
        UprightY
    }

    /// <summary>
    /// Keeps a 2D sprite facing the camera in a 3D scene.
    /// Attach to the SpriteRenderer child of a unit, never to the unit root.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private BillboardMode mode = BillboardMode.UprightY;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) return;
            }

            Transform cam = targetCamera.transform;

            if (mode == BillboardMode.MatchCamera)
            {
                transform.rotation = cam.rotation;
                return;
            }

            // Flatten the camera's forward onto the ground plane.
            Vector3 flat = Vector3.ProjectOnPlane(cam.forward, Vector3.up);

            // Looking straight down: forward is ~vertical, so use camera "up" instead.
            if (flat.sqrMagnitude < 0.0001f)
                flat = Vector3.ProjectOnPlane(cam.up, Vector3.up);

            transform.rotation = Quaternion.LookRotation(flat.normalized, Vector3.up);
        }
    }
}