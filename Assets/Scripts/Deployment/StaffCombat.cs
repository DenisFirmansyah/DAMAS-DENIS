using Enemies;
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Handles combat targeting and attacks for deployed staff units (both Melee and Ranged).
    /// Replaces legacy TowerInstance.
    /// </summary>
    public class StaffCombat : MonoBehaviour
    {
        [Header("Data Source")]
        [SerializeField] private UnitDeploymentData unitData;

        [Header("Combat Configuration")]
        [SerializeField] private UnitAttackType attackType = UnitAttackType.Ranged;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float range = 3f;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Visuals")]
        [SerializeField] private GameObject rangeIndicator;

        [Header("Health & Vitality")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        private float fireCooldown;
        private float damageMultiplier = 1f;
        private float rangeMultiplier = 1f;
        private float attackRateMultiplier = 1f;
        private Transform currentTarget;

        public UnitAttackType AttackType => attackType;
        public float Damage => damage * damageMultiplier;
        public float Range => range * rangeMultiplier;
        public float AttackSpeed => attackSpeed * attackRateMultiplier;
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;

        public float HealthFraction
        {
            get
            {
                StaffVitality vitality = GetComponent<StaffVitality>();
                if (vitality != null) return vitality.HealthFraction;
                return maxHealth <= 0f ? 1f : currentHealth / maxHealth;
            }
        }

        public void Initialize(UnitDeploymentData data)
        {
            if (data == null) return;

            unitData = data;
            attackType = data.AttackType;
            damage = data.Damage;
            range = data.Range;
            attackSpeed = data.AttackSpeed;
            projectileSpeed = data.ProjectileSpeed;
            projectilePrefab = data.ProjectilePrefab;
            maxHealth = data.MaxHealth;
            currentHealth = maxHealth;
        }

        private void Start()
        {
            if (unitData != null)
            {
                Initialize(unitData);
            }

            if (rangeIndicator != null)
                rangeIndicator.SetActive(false);
        }

        private void Update()
        {
            fireCooldown -= Time.deltaTime;

            if (currentTarget == null || !IsTargetInRange())
            {
                EnemyInstance found = FindTarget();
                currentTarget = found != null ? found.transform : null;
            }

            if (fireCooldown <= 0f && currentTarget != null)
            {
                Attack();
                float effectiveRate = Mathf.Max(0.01f, attackSpeed * attackRateMultiplier);
                fireCooldown = 1f / effectiveRate;
            }

            if (rangeIndicator != null && rangeIndicator.activeSelf)
            {
                float diameter = Range * 2f;
                rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
            }
        }

        public EnemyInstance FindTarget()
        {
            float effectiveRange = Range;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effectiveRange, enemyLayer);

            float shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;

            for (int i = 0; i < hits.Length; i++)
            {
                float distance = Vector2.Distance(transform.position, hits[i].transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestEnemy = hits[i].transform;
                }
            }

            return nearestEnemy != null ? nearestEnemy.GetComponent<EnemyInstance>() : null;
        }

        public bool IsTargetInRange()
        {
            if (currentTarget == null) return false;
            return Vector2.Distance(transform.position, currentTarget.position) <= Range;
        }

        private void Attack()
        {
            EnemyInstance target = FindTarget();
            if (target == null) return;

            float effectiveDamage = Damage;

            if (attackType == UnitAttackType.Melee)
            {
                target.TakeDamage(effectiveDamage);
            }
            else // Ranged
            {
                if (projectilePrefab != null)
                {
                    GameObject projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                    Projectile projScript = projGO.GetComponent<Projectile>();
                    if (projScript != null)
                    {
                        projScript.Setup(target.transform, projectileSpeed, effectiveDamage);
                    }
                    else
                    {
                        target.TakeDamage(effectiveDamage);
                    }
                }
                else
                {
                    target.TakeDamage(effectiveDamage);
                }
            }
        }

        public void TakeDamage(float amount)
        {
            StaffVitality vitality = GetComponent<StaffVitality>();
            if (vitality != null)
            {
                vitality.TakeDamage(amount);
            }

            currentHealth -= amount;
            if (currentHealth <= 0f)
            {
                Destroy(gameObject);
            }
        }

        public void ApplyStageUpgrade(float dmg, float rng, float attackRate)
        {
            damageMultiplier *= dmg;
            rangeMultiplier *= rng;
            attackRateMultiplier *= attackRate;
        }

        private void OnMouseEnter()
        {
            if (rangeIndicator != null)
                rangeIndicator.SetActive(true);
        }

        private void OnMouseExit()
        {
            if (rangeIndicator != null)
                rangeIndicator.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = attackType == UnitAttackType.Melee ? Color.red : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, Range);
        }
    }
}
