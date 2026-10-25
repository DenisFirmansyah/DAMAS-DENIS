using System;
using UnityEngine;

namespace DamasDenis.Roaming
{
    /// <summary>Anything the roaming character can interact with by pressing the interact key.</summary>
    public interface IInteractable
    {
        /// <summary>Short text for a prompt, e.g. "Talk" or "Read terminal".</summary>
        string Prompt { get; }

        /// <summary>Called once when the player presses the interact key while this is the nearest target.</summary>
        void Interact(CharacterControl interactor);
    }

    /// <summary>
    /// Roaming-mode player: WASD movement on the board's XZ plane and F to interact.
    /// Put this on the unit ROOT (with a CharacterController). The 2D sprite is a child
    /// that carries the Billboard script, same as deployed units.
    /// Uses the legacy Input class, like DeploymentController and DialogueController.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterControl : MonoBehaviour
    {
        [Header("Movement")]
        [Min(0f)] [SerializeField] private float moveSpeed = 4f;
        [Tooltip("Movement is relative to this camera. Falls back to Camera.main.")]
        [SerializeField] private Camera worldCamera;

        [Header("Keys")]
        [SerializeField] private KeyCode upKey = KeyCode.W;
        [SerializeField] private KeyCode downKey = KeyCode.S;
        [SerializeField] private KeyCode leftKey = KeyCode.A;
        [SerializeField] private KeyCode rightKey = KeyCode.D;
        [SerializeField] private KeyCode interactKey = KeyCode.F;

        [Header("Interaction")]
        [Min(0.1f)] [SerializeField] private float interactRadius = 1.2f;
        [Tooltip("Only colliders on these layers can be interacted with.")]
        [SerializeField] private LayerMask interactableLayer;

        [Header("Visual (optional)")]
        [Tooltip("Animator on the sprite child. Needs a float parameter named 'Speed'.")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private readonly Collider[] hitBuffer = new Collider[16];
        private CharacterController controller;
        private IInteractable current;

        /// <summary>When false the character ignores all input (cutscenes, menus).</summary>
        public bool InputEnabled { get; set; } = true;

        /// <summary>The nearest interactable in reach, or null. Use it to show an "F" prompt.</summary>
        public IInteractable CurrentInteractable => current;

        /// <summary>Raised when the nearest interactable changes (null = nothing in reach).</summary>
        public event Action<IInteractable> InteractableChanged;

        /// <summary>Raised after the player interacts with something.</summary>
        public event Action<IInteractable> Interacted;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (worldCamera == null) worldCamera = Camera.main;
        }

        private void Update()
        {
            // Dialogue pauses gameplay with timeScale 0; don't let F or WASD act underneath it.
            bool blocked = !InputEnabled || Time.timeScale <= 0f;

            Vector2 input = blocked ? Vector2.zero : ReadMoveInput();
            Move(input);
            UpdateVisual(input);

            RefreshInteractable();
            if (!blocked && current != null && Input.GetKeyDown(interactKey))
            {
                IInteractable target = current;
                target.Interact(this);
                Interacted?.Invoke(target);
            }
        }

        private Vector2 ReadMoveInput()
        {
            Vector2 v = Vector2.zero;
            if (Input.GetKey(upKey)) v.y += 1f;
            if (Input.GetKey(downKey)) v.y -= 1f;
            if (Input.GetKey(rightKey)) v.x += 1f;
            if (Input.GetKey(leftKey)) v.x -= 1f;
            return Vector2.ClampMagnitude(v, 1f); // stops diagonals being faster
        }

        private void Move(Vector2 input)
        {
            GetGroundAxes(out Vector3 forward, out Vector3 right);
            Vector3 direction = forward * input.y + right * input.x;

            // SimpleMove applies gravity and Time.deltaTime for us, and takes metres per second.
            controller.SimpleMove(direction * moveSpeed);
        }

        /// <summary>
        /// "Up" and "right" on screen, flattened onto the ground. Works for a tilted camera
        /// and for one looking straight down (same trick as Billboard).
        /// </summary>
        private void GetGroundAxes(out Vector3 forward, out Vector3 right)
        {
            if (worldCamera == null) worldCamera = Camera.main;

            if (worldCamera == null)
            {
                forward = Vector3.forward;
                right = Vector3.right;
                return;
            }

            Transform cam = worldCamera.transform;
            forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.ProjectOnPlane(cam.up, Vector3.up);

            forward.Normalize();
            right = Vector3.Cross(Vector3.up, forward);
        }

        private void UpdateVisual(Vector2 input)
        {
            if (animator != null)
                animator.SetFloat(SpeedHash, input.sqrMagnitude > 0.0001f ? 1f : 0f);

            // Sprites face screen left/right, so flip on the screen-space X input.
            if (spriteRenderer != null)
            {
                if (input.x < -0.1f) spriteRenderer.flipX = true;
                else if (input.x > 0.1f) spriteRenderer.flipX = false;
            }
        }

        private void RefreshInteractable()
        {
            Vector3 center = transform.position + controller.center;
            int count = Physics.OverlapSphereNonAlloc(center, interactRadius, hitBuffer,
                interactableLayer, QueryTriggerInteraction.Collide);

            IInteractable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                // The collider may sit on a child, so search upward (like MapCell lookup).
                IInteractable candidate = hitBuffer[i].GetComponentInParent<IInteractable>();
                if (candidate == null) continue;

                float distance = (hitBuffer[i].ClosestPoint(center) - center).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            if (best == current) return;
            current = best;
            InteractableChanged?.Invoke(current);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}