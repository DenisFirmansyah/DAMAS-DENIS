using DamasDenis.Deployment;
using UnityEngine;

namespace DamasDenis.Stage
{
    /// <summary>
    /// Positions and zooms the camera so the whole board sits centered in the part of the
    /// screen not covered by UI. Camera rotation is left alone; only position and size change.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class StageCameraFramer : MonoBehaviour
    {
        [Header("Screen area covered by UI (fractions of the screen, 0-0.5)")]
        [Range(0f, 0.5f)] [SerializeField] private float leftMargin;
        [Range(0f, 0.5f)] [SerializeField] private float rightMargin;
        [Range(0f, 0.5f)] [SerializeField] private float topMargin = 0.05f;
        [Range(0f, 0.5f)] [SerializeField] private float bottomMargin = 0.2f;

        [Header("Framing")]
        [Tooltip("Extra world units around the board edges.")]
        [Min(0f)] [SerializeField] private float worldPadding = 0.5f;
        [Tooltip("Orthographic only: how far back along the view direction the camera sits.")]
        [Min(1f)] [SerializeField] private float orthoDistance = 30f;
        [Min(0.5f)] [SerializeField] private float minOrthoSize = 3f;

        private Camera cam;
        private Bounds bounds;
        private bool hasBounds;
        private int lastWidth, lastHeight;

        private void Awake() => cam = GetComponent<Camera>();

        /// <summary>Reads the board from BoardGrid and frames it. Call after the stage scene loads.</summary>
        public void Frame()
        {
            hasBounds = BoardGrid.TryGetBounds(out bounds);
            if (!hasBounds)
            {
                Debug.LogWarning("StageCameraFramer: no MapCells registered, nothing to frame.", this);
                return;
            }
            Apply();
        }

        private void LateUpdate()
        {
            // Re-frame only when the window or aspect ratio changes.
            if (hasBounds && (Screen.width != lastWidth || Screen.height != lastHeight))
                Apply();
        }

        private void Apply()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            Transform t = cam.transform;
            Vector3 center = bounds.center;
            Vector3 ext = bounds.extents + new Vector3(worldPadding, 0f, worldPadding);

            // Half-size of the board as seen along the camera's own axes.
            float ex = 0f, ey = 0f, ez = 0f;
            for (int sx = -1; sx <= 1; sx += 2)
            for (int sy = -1; sy <= 1; sy += 2)
            for (int sz = -1; sz <= 1; sz += 2)
            {
                Vector3 v = new Vector3(sx * ext.x, sy * ext.y, sz * ext.z);
                ex = Mathf.Max(ex, Mathf.Abs(Vector3.Dot(v, t.right)));
                ey = Mathf.Max(ey, Mathf.Abs(Vector3.Dot(v, t.up)));
                ez = Mathf.Max(ez, Mathf.Abs(Vector3.Dot(v, t.forward)));
            }

            float freeW = Mathf.Max(0.1f, 1f - leftMargin - rightMargin);
            float freeH = Mathf.Max(0.1f, 1f - topMargin - bottomMargin);

            float halfH, halfW, distance;
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Max(minOrthoSize, ey / freeH, ex / (cam.aspect * freeW));
                halfH = cam.orthographicSize;
                halfW = halfH * cam.aspect;
                distance = orthoDistance;
            }
            else
            {
                float tanV = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                // Conservative: add the board's depth so near corners still fit.
                distance = Mathf.Max(ey / (tanV * freeH), ex / (tanV * cam.aspect * freeW)) + ez;
                halfH = distance * tanV;
                halfW = halfH * cam.aspect;
            }

            // Where the middle of the free area sits on screen (0-1), and how far that is from screen center.
            float cx = (leftMargin + (1f - rightMargin)) * 0.5f;
            float cy = (bottomMargin + (1f - topMargin)) * 0.5f;
            float dx = (cx - 0.5f) * 2f * halfW;
            float dy = (cy - 0.5f) * 2f * halfH;

            // Move the camera opposite, so the board lands in the middle of the free area.
            t.position = center - t.forward * distance - t.right * dx - t.up * dy;
        }
    }
}