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
        private WeaponVisualController weaponVisual;
        private readonly HashSet<int> floatAnimatorParameters = new HashSet<int>();

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
        private const float RetargetInterval = 0.2f;
        private static readonly List<DeployedUnit> unitBuffer = new List<DeployedUnit>();
        private DeployedUnit rangedTarget;
        private float rangedTimer;
        private float retargetTimer;

        public bool IsDead { get; private set; }

        public void Initialize(EnemyPath route)
        {
            path = route;
        }

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

        /// <summary>Waypoints left before the base. Smaller means more dangerous. Comparable across different routes.</summary>
        public float RemainingSteps => path == null ? float.MaxValue : (path.PointCount - 1) - RouteProgress;

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
                weaponVisual = WeaponVisualController.Configure(visualObj, enemyData.weapon, enemyData.weaponMountPath);
                CacheAnimatorParameters();
            }

            nextIndex = 1; // point 0 is where the enemy spawned
            StartCoroutine(StartMovementAfterDelay(startDelay));
        }

        private void Update()
        {
            if (!canMove || IsDead) return;

            if (blockedBy == null)
                TryGetBlocked();

            if (blockedBy != null)
            {
                FightBlocker();
                return;
            }

            if (AttackElevatedUnit()) return;

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
                SetAnimatorFloatIfPresent("Horizontal", direction.x);
                SetAnimatorFloatIfPresent("Vertical", direction.z);
                SetAnimatorFloatIfPresent("Speed", 1f);
            }
            weaponVisual?.SetMoving(true);

            if (sr != null)
            {
                if (direction.x < -0.1f)
                {
                    sr.flipX = true;
                    weaponVisual?.SetFacing(true);
                }
                else if (direction.x > 0.1f)
                {
                    sr.flipX = false;
                    weaponVisual?.SetFacing(false);
                }
            }
        }

        private void SetAnimatorFloatIfPresent(string parameterName, float value)
        {
            if (anim == null) return;
            int parameterHash = Animator.StringToHash(parameterName);
            if (floatAnimatorParameters.Contains(parameterHash))
                anim.SetFloat(parameterHash, value);
        }

        private void CacheAnimatorParameters()
        {
            if (anim == null) return;

            AnimatorControllerParameter[] parameters = anim.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].type == AnimatorControllerParameterType.Float)
                    floatAnimatorParameters.Add(parameters[i].nameHash);
            }
        }

        private void ReachEndOfPath()
        {
            IsDead = true;
            StageState stageState = StageState.Instance;
            if (stageState == null) stageState = FindObjectOfType<StageState>();
            if (stageState != null) stageState.TakeDamage(enemyData.damage);
            StageEventLog.Record(StageEvents.EnemyLeaked, enemyData.enemyName);
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
            SetAnimatorFloatIfPresent("Speed", 0f);
            weaponVisual?.SetMoving(false);

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f) return;

            attackTimer = enemyData.attackInterval;
            weaponVisual?.PlayAttack();
            blockedBy.TakeDamage(enemyData.attackDamage);
        }

        public void ReleaseFromBlocker()
        {
            blockedBy = null;
        }

        private bool AttackElevatedUnit()
        {
            if (enemyData.attackRange <= 0f) return false;

            // A destroyed target is null (Unity null check); also drop it if it left range.
            if (rangedTarget != null && FlatDistance(transform.position, rangedTarget.transform.position) > enemyData.attackRange)
                rangedTarget = null;

            if (rangedTarget == null)
            {
                retargetTimer -= Time.deltaTime;
                if (retargetTimer > 0f) return false;

                retargetTimer = RetargetInterval;
                rangedTarget = FindElevatedTarget();
                if (rangedTarget == null) return false;

                rangedTimer = 0f; // first shot lands immediately, like the melee fight
            }

            SetAnimatorFloatIfPresent("Speed", 0f);
            weaponVisual?.SetMoving(false);

            // Face the target (same screen-X flip rule as Move).
            float dx = rangedTarget.transform.position.x - transform.position.x;
            if (sr != null && Mathf.Abs(dx) > 0.1f)
            {
                sr.flipX = dx < 0f;
                weaponVisual?.SetFacing(dx < 0f);
            }

            rangedTimer -= Time.deltaTime;
            if (rangedTimer > 0f) return true;

            rangedTimer = enemyData.rangedAttackInterval;
            weaponVisual?.PlayAttack();
            DamageUnit(rangedTarget, enemyData.attackDamage);
            return true;
        }

        private DeployedUnit FindElevatedTarget()
        {
            StageMapContext map = StageMapContext.Current;
            if (map == null) return null;

            map.GetOrCreateUnitContainer().GetComponentsInChildren(unitBuffer);

            DeployedUnit best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                DeployedUnit unit = unitBuffer[i];
                if (!IsElevated(unit)) continue;

                float distance = FlatDistance(transform.position, unit.transform.position);
                if (distance > enemyData.attackRange || distance >= bestDistance) continue;

                best = unit;
                bestDistance = distance;
            }

            unitBuffer.Clear(); // don't keep references alive between calls
            return best;
        }


        private static bool IsElevated(DeployedUnit unit) => unit.Cell != null && unit.Cell.Terrain == CellTerrain.Elevated;

        private static void DamageUnit(DeployedUnit unit, float amount)
        {
            StaffVitality vitality = unit.GetComponent<StaffVitality>();
            if (vitality != null) vitality.TakeDamage(amount);
        }

            private void OnDrawGizmosSelected()
        {
            if (enemyData == null || enemyData.attackRange <= 0f) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
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
            StageEventLog.Record(StageEvents.EnemyKilled, enemyData.enemyName);
            Destroy(gameObject);
        }
    }
}
