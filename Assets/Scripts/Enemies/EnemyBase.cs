using System.Collections;
using System.Collections.Generic;
using DamasDenis.Deployment;
using DamasDenis.Stage;
using UnityEngine;

namespace DamasDenis.Enemies
{
    public class EnemyInstance : MonoBehaviour {

        private static readonly List<EnemyInstance> active = new List<EnemyInstance>();

        /// <summary>All living enemies. Read-only.</summary>
        public static IReadOnlyList<EnemyInstance> Active => active;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => active.Clear();

        [Header("Data Source")]
        public EnemyData enemyData;

        [Header("References")]
        private Animator anim;
        private SpriteRenderer sr;

        [Header("Movement")]
        [Tooltip("Seconds the enemy waits after spawning before it starts walking.")]
        [Min(0f)] [SerializeField] private float startDelay = 1f;

        [Header("Blocking")]
        [Tooltip("An enemy this close (world units) to a route cell's centre counts as standing in that cell.")]
        [Min(0f)] [SerializeField] private float blockReach = 0.45f;

        [Header("Runtime Attributes")]
        [SerializeField] private float currentHealth;

        private EnemyPath path;
        private int nextIndex;
        // private bool isMoving = true;
        private bool canMove;
        private StaffBlocker blockedBy;
        private float attackTimer;

        public bool IsDead { get; private set; }
        public float RouteProgress
        {
            get
            {
                if (path == null || path.PointCount < 2) return 0f;

                int from = Mathf.Clamp(nextIndex - 1, 0, path.PointCount - 2);
                Vector2 a = path.GetPoint(from);
                Vector2 b = path.GetPoint(from + 1);
                float segment = Vector2.Distance(a, b);
                if (segment <= 0.0001f) return from;

                return from + Mathf.Clamp01(FlatDistance(transform.position, b) / segment);
            }
        }

        public MapCell CurrentCell
        {
            get
            {
                if (path == null) return null;
                return path.GetCell(Mathf.FloorToInt(RouteProgress + 0.5f));
            }
        }

        private void OnDestroy()
        {
            if (blockedBy != null) blockedBy.Release(this);
        }

        private void OnEnable() => active.Add(this);
        private void OnDisable() => active.Remove(this);

        private void Start()
        {
            if (enemyData == null)
            {
                Debug.LogError($"{name}: Enemy Data is not assigned.", this);
                enabled = false;
                return;
            }

            path = EnemyPath.Instance;
            if (path == null || path.PointCount < 2)
            {
                Debug.LogError($"{name}: the scene needs an EnemyPath with at least 2 points.", this);
                enabled = false;
                return;
            }

            currentHealth = enemyData.maxHealth;

            if (enemyData.enemyPrefab != null)
            {
                GameObject visualObj = Instantiate(enemyData.enemyPrefab, transform);
                visualObj.transform.localPosition = new Vector3(0, -0.5f, 0);
                anim = visualObj.GetComponentInChildren<Animator>();
                sr = visualObj.GetComponentInChildren<SpriteRenderer>();
            }

            nextIndex = 1; // point 0 is where the enemy spawned
            StartCoroutine(StartMovementAfterDelay(startDelay));
        }

        private void Update()
        {
            if (!canMove || IsDead) return;

            // blockedBy turns null on its own if the blocker is destroyed (Unity null check).
            if (blockedBy == null)
                TryGetBlocked();

            if (blockedBy != null)
            {
                FightBlocker();
                return;
            }

            Move();
        }

        private IEnumerator StartMovementAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            canMove = true;
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f; b.y = 0f;   // the map is on XZ; ignore height
            return Vector3.Distance(a, b);
        }

        private void Move()
        {
            Vector3 target = path.GetPoint(nextIndex);
            Vector3 flat = target - transform.position;
            flat.y = 0f;
            float step = enemyData.speed * Time.deltaTime;

            if (flat.magnitude <= step)
            {
                transform.position = new Vector3(target.x, transform.position.y, target.z);
                nextIndex++;
                if (nextIndex >= path.PointCount) ReachEndOfPath();
                return;
            }

            Vector3 direction = flat.normalized;
            transform.position += direction * step;

            if (anim != null)
            {
                anim.SetFloat("Horizontal", direction.x);
                anim.SetFloat("Vertical", direction.z);
                anim.SetFloat("Speed", 1f);
            }

            if (sr != null)
            {
                if (direction.x < -0.1f) sr.flipX = true;
                else if (direction.x > 0.1f) sr.flipX = false;
            }
        }

        private void ReachEndOfPath()
        {
            IsDead = true;
            StageState stageState = StageState.Instance;
            if (stageState == null) stageState = FindObjectOfType<StageState>();
            if (stageState != null) stageState.TakeDamage(enemyData.damage);
            Destroy(gameObject);
        }

        private void TryGetBlocked()
        {
            if (!enemyData.canBeBlocked) return;

            // Only the two cells either side of the enemy's current segment matter.
            for (int i = nextIndex - 1; i <= nextIndex; i++)
            {
                MapCell cell = path.GetCell(i);
                if (cell == null || !cell.IsOccupied) continue;
                if (FlatDistance(transform.position, cell.SurfacePoint) > blockReach) continue;

                StaffBlocker blocker = cell.Occupant.GetComponent<StaffBlocker>();
                if (blocker != null && blocker.TryBlock(this))
                {
                    blockedBy = blocker;
                    attackTimer = 0f; // first hit lands immediately
                    return;
                }
            }
        }
        
        private void FightBlocker()
        {
            if (anim != null) anim.SetFloat("Speed", 0f);

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f) return;

            attackTimer = enemyData.attackInterval;
            blockedBy.TakeDamage(enemyData.attackDamage);
        }

        public void ReleaseFromBlocker()
        {
            blockedBy = null;
        }

        public void TakeDamage(float amount) 
        {
            if (IsDead) return;
            
            currentHealth -= amount;
            if (currentHealth <= 0) {
                Die();
            }
        }

        private void Die()
        {
            IsDead = true;
            StageState stageState = StageState.Instance;
            if (stageState == null) stageState = FindObjectOfType<StageState>();
            if (stageState != null) stageState.AddStageResources(enemyData.reward);
            Destroy(gameObject);
        }
    }
}
