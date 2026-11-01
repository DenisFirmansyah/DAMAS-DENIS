using DamasDenis.Roaming;
using UnityEngine;
using UnityEngine.AI;

namespace DamasDenis.UI
{
    /// <summary>
    /// HUD arrow that points along the walkable route to the current objective.
    /// Only the first turn of the route is used, so it is an arrow, not a trail.
    /// Put this on an always-active object, NOT on the arrow itself (the arrow gets hidden).
    /// </summary>
    public sealed class ObjectiveArrow : MonoBehaviour
    {
        [SerializeField] private CharacterControl character;
        [SerializeField] private RoamingTaskSpawner spawner;
        [SerializeField] private RectTransform arrow;
        [Tooltip("Falls back to Camera.main.")]
        [SerializeField] private Camera worldCamera;

        [Header("Tuning")]
        [Min(0.05f)] [SerializeField] private float repathInterval = 0.25f;
        [Tooltip("Arrow hides when the player is this close to the objective.")]
        [Min(0f)] [SerializeField] private float hideDistance = 1.5f;
        [Tooltip("Corners closer than this are skipped, so the arrow doesn't flip while rounding a turn.")]
        [Min(0f)] [SerializeField] private float skipCornerDistance = 0.75f;
        [Tooltip("How far a point may be from the NavMesh and still be snapped onto it.")]
        [Min(0.1f)] [SerializeField] private float snapDistance = 2f;


        private NavMeshPath path;
        private Vector3 aimPoint;
        private float repathTimer;

        private void Awake()
        {
            path = new NavMeshPath();
        }

        private void Update()
        {
            Transform target = spawner != null ? spawner.GetObjectiveTarget() : null;
            if (target == null || character == null)
            {
                SetVisible(false);
                return;
            }

            Vector3 playerPos = character.transform.position;
            if (FlatDistance(playerPos, target.position) <= hideDistance)
            {
                SetVisible(false);
                return;
            }

            // Path queries every 0.25s are plenty. Only the rotation updates every frame.
            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                repathTimer = repathInterval;
                aimPoint = ComputeAimPoint(playerPos, target.position);
            }

            SetVisible(true);
            PointArrowAt(playerPos, aimPoint);
        }

        private Vector3 ComputeAimPoint(Vector3 from, Vector3 to)
        {
            // Snap both ends onto the mesh: the player's pivot and the pickup may sit slightly off it.
            if (NavMesh.SamplePosition(from, out NavMeshHit start, snapDistance, NavMesh.AllAreas) &&
                NavMesh.SamplePosition(to, out NavMeshHit end, snapDistance, NavMesh.AllAreas) &&
                NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                Vector3[] corners = path.corners; // [0] is the start, so look from [1]
                for (int i = 1; i < corners.Length; i++)
                    if (FlatDistance(from, corners[i]) > skipCornerDistance)
                        return corners[i];
            }

            return to; // no route found: point straight at it so the arrow still does something
        }

        private void PointArrowAt(Vector3 from, Vector3 to)
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (worldCamera == null) return;

            // Same ground "up" and "right" as CharacterControl, so the arrow agrees with WASD.
            Transform cam = worldCamera.transform;
            Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.ProjectOnPlane(cam.up, Vector3.up);
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            Vector3 direction = to - from;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            direction.Normalize();

            // Angle from screen-up, positive toward screen-right. UI rotation is counter-clockwise, so negate.
            float angle = Mathf.Atan2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward)) * Mathf.Rad2Deg;
            arrow.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }

        private void SetVisible(bool visible)
        {
            if (arrow != null && arrow.gameObject.activeSelf != visible)
                arrow.gameObject.SetActive(visible);
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f; b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}