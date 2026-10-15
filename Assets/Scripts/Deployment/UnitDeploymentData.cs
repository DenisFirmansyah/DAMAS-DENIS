using UnityEngine;

namespace DamasDenis.Deployment
{
    public enum UnitAttackType
    {
        Melee,
        Ranged
    }

    [CreateAssetMenu(fileName = "UnitDeploymentData", menuName = "DAMAS DENIS/Deployment Unit")]
    public sealed class UnitDeploymentData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "New Unit";
        [Tooltip("A stable, unique PlayerPrefs key segment. Set this once before shipping.")]
        [SerializeField] private string unlockId = "staff.new_unit";
        [SerializeField] private bool unlockedByDefault = true;
        [SerializeField] private GameObject unitPrefab;

        [Header("Deployment")]
        [Min(0)] [SerializeField] private int deploymentCost = 10;
        [SerializeField] private bool deployOnGround = true;
        [SerializeField] private bool deployOnElevated;
        [SerializeField] private bool canFace = true;
        [SerializeField] private bool isMedic;

        [Header("Combat Type & Attributes")]
        [SerializeField] private UnitAttackType attackType = UnitAttackType.Ranged;
        [Min(0f)] [SerializeField] private float damage = 10f;
        [Min(0.1f)] [SerializeField] private float range = 3f;
        [Min(0.01f)] [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private GameObject projectilePrefab;
        [Min(1f)] [SerializeField] private float maxHealth = 100f;

        public string DisplayName => displayName;
        public string UnlockId => unlockId;
        public GameObject UnitPrefab => unitPrefab;
        public int DeploymentCost => deploymentCost;
        public bool CanFace => canFace;
        public bool IsMedic => isMedic;
        public bool IsUnlocked => unlockedByDefault || StaffUnlocks.IsUnlocked(unlockId);

        public UnitAttackType AttackType => attackType;
        public UnitAttackType Type => attackType;
        public float Damage => damage;
        public float Range => range;
        public float AttackSpeed => attackSpeed;
        public float ProjectileSpeed => projectileSpeed;
        public GameObject ProjectilePrefab => projectilePrefab;
        public float MaxHealth => maxHealth;

        public bool CanDeployOn(CellTerrain terrain)
        {
            return (terrain == CellTerrain.Ground && deployOnGround) ||
                   (terrain == CellTerrain.Elevated && deployOnElevated);
        }
    }
}
