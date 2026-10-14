using UnityEngine;
using Enemies;

public class TowerInstance : MonoBehaviour {
    [Header("Data Source")]
    public TowerData towerData;

    [Header("Runtime Attributes")]
    private float currentHealth;
    private float fireCooldown = 1f;
    private float damageMultiplier = 1f;
    private float rangeMultiplier = 1f;
    private float attackRateMultiplier = 1f;
    private Transform currentTarget;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] public GameObject rangeIndicator;

    void Start() {
        if (towerData == null) return;

        currentHealth = towerData.maxHealth;

        if (transform.childCount == 1 && towerData.towerPrefab != null) {
            GameObject visual = Instantiate(towerData.towerPrefab, transform);
            
            visual.transform.localPosition = Vector3.zero; 
        }
        
        rangeIndicator?.SetActive(false);
    }

    void Update() {
        fireCooldown -= Time.deltaTime;
        
        if (currentTarget == null || !IsTargetInRange()) {
            currentTarget = FindTarget()?.transform;
        }

        if (fireCooldown <= 0f && currentTarget != null) {
            Attack();
            fireCooldown = 1f / (towerData.attackSpeed * attackRateMultiplier);
        }

        if (rangeIndicator != null && rangeIndicator.activeSelf)
        {
            // Calculate diameter. Assumes the sprite is 1x1 Unity units by default.
            float diameter = towerData.range * 2f; 
            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

    EnemyInstance FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, towerData.range * rangeMultiplier, enemyLayer);

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider2D hit in hits) {
            float distance = Vector2.Distance(transform.position, hit.transform.position);
            
            if (distance < shortestDistance) {
                shortestDistance = distance;
                nearestEnemy = hit.transform;
            }
        }

        if (nearestEnemy != null)
        {
            return nearestEnemy.GetComponent<EnemyInstance>();
        }

        return null;
    }

    bool IsTargetInRange() {
        if (currentTarget == null) return false;
        return Vector2.Distance(transform.position, currentTarget.position) <= towerData.range * rangeMultiplier;
    }

    void Attack() {
        EnemyInstance target = FindTarget();
        if (target != null)
        {
            GameObject projGO = Instantiate(towerData.projectilePrefab, transform.position, Quaternion.identity);
            
            Projectile projScript = projGO.GetComponent<Projectile>();
            
            projScript?.Setup(target.transform, towerData.projectileSpeed, towerData.damage * damageMultiplier);
        }
    }

    public void TakeDamage(float amount) {
        currentHealth -= amount;
        if (currentHealth <= 0) Destroy(gameObject);
    }

    public void ApplyStageUpgrade(float damage, float range, float attackRate)
    {
        damageMultiplier *= damage;
        rangeMultiplier *= range;
        attackRateMultiplier *= attackRate;
    }

    public float HealthFraction
    {
        get { return towerData == null || towerData.maxHealth <= 0f ? 1f : currentHealth / towerData.maxHealth; }
    }

    private void OnDrawGizmosSelected() {
        if (towerData != null) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, towerData.range);
        }
    }

    void OnMouseEnter()
    {
        rangeIndicator?.SetActive(true);
    }

    void OnMouseExit()
    {
        rangeIndicator?.SetActive(false);
    }
}
