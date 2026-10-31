
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Applies a depth-aware URP material to every
    /// SpriteRenderer inside the visual hierarchy.
    /// </summary>
    public class SpriteDepthRenderer : MonoBehaviour
    {
        [Header("Material Settings")]
        [SerializeField]
        private Material depthMaterial;

        [Header("Options")]
        [SerializeField]
        private bool includeInactive = true;

        private SpriteRenderer[] spriteRenderers;

        private void Awake()
        {
            ApplyMaterial();
        }

        private void ApplyMaterial()
        {
            if (depthMaterial == null)
            {
                Debug.LogError(
                    "[SpriteDepthRenderer] Depth Material is missing!",
                    this
                );

                return;
            }

            spriteRenderers =
                GetComponentsInChildren<SpriteRenderer>(
                    includeInactive
                );

            foreach (SpriteRenderer sr in spriteRenderers)
            {
                sr.sharedMaterial = depthMaterial;
            }

            Debug.Log(
                $"[SpriteDepthRenderer] Applied depth material " +
                $"to {spriteRenderers.Length} SpriteRenderers.",
                this
            );
        }
    }
}
