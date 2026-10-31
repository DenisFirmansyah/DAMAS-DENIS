using UnityEngine;

namespace DamasDenis.Deployment
{
    public enum BillboardMode
    {
        MatchCamera,
        UprightY
    }

    /// <summary>
    /// Taruh di root unit (Soldier). Sprite menghadap kamera, tapi depth dihitung
    /// seolah sprite berdiri tegak di pivot, sehingga tidak tembus block.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private static readonly int PivotId  = Shader.PropertyToID("_PivotWS");
        private static readonly int NormalId = Shader.PropertyToID("_PlaneNormalWS");

        [SerializeField] private Camera targetCamera;
        [SerializeField] private BillboardMode mode = BillboardMode.MatchCamera;
        [Tooltip("Titik kaki karakter. Kosongkan untuk pakai posisi root.")]
        [SerializeField] private Transform pivot;

        private SpriteRenderer[] renderers;
        private MaterialPropertyBlock block;

        private void Awake()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (pivot == null) pivot = transform;

            block = new MaterialPropertyBlock();
            RefreshRenderers();
        }

        /// <summary>Panggil kalau ada sprite yang ditambah/dihapus saat runtime.</summary>
        public void RefreshRenderers()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) return;
            }

            Transform cam = targetCamera.transform;

            // Normal bidang vertikal: arah horizontal dari scene MENUJU kamera
            Vector3 flatForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
            if (flatForward.sqrMagnitude < 0.0001f)
                flatForward = Vector3.ProjectOnPlane(cam.up, Vector3.up);
            flatForward.Normalize();
            Vector3 planeNormal = -flatForward;

            // Orientasi visual
            transform.rotation = (mode == BillboardMode.MatchCamera)
                ? cam.rotation
                : Quaternion.LookRotation(flatForward, Vector3.up);

            // Kirim data ke shader
            Vector3 pivotPos = pivot.position;
            foreach (var r in renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetVector(PivotId, pivotPos);
                block.SetVector(NormalId, planeNormal);
                r.SetPropertyBlock(block);
            }
        }
    }
}