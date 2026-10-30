using System.Collections.Generic;
using DamasDenis.Enemies;
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Attacks for deployed staff. A unit attacks the enemy furthest along the route
    /// among those standing on the tiles of its range pattern, rotated to its facing.
    /// </summary>
    public class StaffCombat : MonoBehaviour
    {
        [Header("Data Source")]
        [SerializeField] private UnitDeploymentData unitData;

        [Header("Combat (overwritten from data at deploy)")]
        [SerializeField] private UnitAttackType attackType = UnitAttackType.Ranged;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private GameObject projectilePrefab;

        private AttackRangePattern pattern;
        private StaffVitality vitality;
        private DeployedUnit deployedUnit;
        private bool initialized;
        private float fireCooldown;
        private float damageMultiplier = 1f;
        private float attackRateMultiplier = 1f;
        private StaffBlocker blocker;
        private WeaponVisualController weaponVisual;

        private readonly List<Vector2Int> coordinateBuffer = new List<Vector2Int>();
        private readonly List<MapCell> coveredCells = new List<MapCell>();

        public UnitAttackType AttackType => attackType;
        public float Damage => damage * damageMultiplier;
        public float AttackSpeed => attackSpeed * attackRateMultiplier;
        public float MaxHealth => vitality.MaxHealth;
        public float CurrentHealth => vitality.CurrentHealth;
        public float HealthFraction => vitality.HealthFraction;

        private void Awake()
        {
            // Prefabs made before StaffVitality existed won't have it.
            vitality = GetComponent<StaffVitality>();
            if (vitality == null) vitality = gameObject.AddComponent<StaffVitality>();
        }

        /// <summary>Copies stats from the unit asset. Called by DeployedUnit when placed.</summary>
        public void Initialize(UnitDeploymentData data)
        {
            if (data == null) return;

            unitData = data;
            attackType = data.AttackType;
            damage = data.Damage;
            attackSpeed = data.AttackSpeed;
            projectileSpeed = data.ProjectileSpeed;
            projectilePrefab = data.ProjectilePrefab;
            weaponVisual = WeaponVisualController.Configure(gameObject, data.Weapon, data.WeaponMountPath);
            if (data.Weapon != null)
            {
                damage = data.Weapon.damage;
                attackSpeed = 1f / Mathf.Max(0.01f, data.Weapon.fireInterval);
                projectileSpeed = data.Weapon.projectileSpeed;
                projectilePrefab = data.Weapon.projectilePrefab;
            }
            pattern = data.RangePattern;
            vitality.Initialize(data.MaxHealth);
            initialized = true;

            if (pattern == null && data.Damage > 0f)
                Debug.LogWarning($"{data.DisplayName} deals damage but has no Range Pattern, so it will never attack.", this);
        }

        private void Start()
        {
            if (!initialized && unitData != null) Initialize(unitData);
        }

        private void Update()
        {
            fireCooldown = Mathf.Max(0f, fireCooldown - Time.deltaTime);
            if (fireCooldown > 0f) return;

            EnemyInstance target = FindTarget();
            if (target == null)
            {
                weaponVisual?.SetMoving(false);
                return;
            }

            Attack(target);
            fireCooldown = 1f / Mathf.Max(0.01f, AttackSpeed);
        }

        /// <summary>The tiles this unit currently covers, rotated to its facing. Empty if unplaced or no pattern.</summary>
        public IReadOnlyList<MapCell> GetCoveredCells()
        {
            coveredCells.Clear();
            if (pattern == null) return coveredCells;

            if (deployedUnit == null) deployedUnit = GetComponent<DeployedUnit>();
            if (deployedUnit == null || deployedUnit.Cell == null) return coveredCells;

            // Units that can't face (e.g. a medic) always use the authored direction.
            Vector2Int facing = (unitData != null && !unitData.CanFace) ? Vector2Int.up : deployedUnit.Facing;

            pattern.GetCoordinates(deployedUnit.Cell.Coordinates, facing, coordinateBuffer);
            for (int i = 0; i < coordinateBuffer.Count; i++)
            {
                if (BoardGrid.TryGetCell(coordinateBuffer[i], out MapCell cell))
                    coveredCells.Add(cell);
            }
            return coveredCells;
        }

        /// <summary>The enemy furthest along the route among those on covered tiles, or null.</summary>
        public EnemyInstance FindTarget()
        {
            IReadOnlyList<MapCell> covered = GetCoveredCells();
            if (covered.Count == 0) return null;

            EnemyInstance best = null;
            float bestRemaining = float.MaxValue;

            IReadOnlyList<EnemyInstance> enemies = EnemyInstance.Active;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyInstance enemy = enemies[i];
                if (enemy == null || enemy.IsDead) continue;

                MapCell enemyCell = enemy.CurrentCell;
                if (enemyCell == null || !ContainsCell(covered, enemyCell)) continue;

                float remaining = enemy.RemainingSteps;
                if (remaining < bestRemaining)
                {
                    bestRemaining = remaining;
                    best = enemy;
                }
            }
            return best;
        }

        private static bool ContainsCell(IReadOnlyList<MapCell> cells, MapCell cell)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] == cell) return true;
            }
            return false;
        }

        private void Attack(EnemyInstance target)
        {
            weaponVisual?.PlayAttack();
            float effectiveDamage = Damage;

            if (attackType == UnitAttackType.Ranged && projectilePrefab != null)
            {
                GameObject projectileObject = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                Projectile projectile = projectileObject.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Setup(target.transform, projectileSpeed, effectiveDamage);
                    return;
                }
                Destroy(projectileObject); // no Projectile script: fall back to a direct hit
            }

            target.TakeDamage(effectiveDamage);
        }

        /// <summary>Forwarded to StaffVitality, which owns health and death.</summary>
        public void TakeDamage(float amount) => vitality.TakeDamage(amount);

        /// <summary>
        /// Called by the upgrade system. Range is ignored on purpose: ranges are tile patterns,
        /// and range upgrades are out of scope for the prototype.
        /// </summary>
        public void ApplyStageUpgrade(float dmg, float attackRate)
        {
            damageMultiplier *= dmg;
            attackRateMultiplier *= attackRate;
        }

        // Scene-view debugging: select a placed unit in Play mode to see its covered tiles.
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = attackType == UnitAttackType.Melee ? Color.red : Color.cyan;
            IReadOnlyList<MapCell> covered = GetCoveredCells();
            for (int i = 0; i < covered.Count; i++)
                Gizmos.DrawWireCube(covered[i].transform.position, Vector3.one * 0.9f);
        }
    }
}