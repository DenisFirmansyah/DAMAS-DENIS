using UnityEngine;

namespace DamasDenis.Deployment
{
    [CreateAssetMenu(fileName = "StaffUpgrade", menuName = "DAMAS DENIS/Staff Upgrade")]
    public sealed class StaffUpgradeDefinition : ScriptableObject
    {
        [SerializeField] private UnitDeploymentData targetStaff;
        [Min(0)] [SerializeField] private int resourceCost = 10;
        [Min(1)] [SerializeField] private int maximumLevel = 1;
        [Min(0.01f)] [SerializeField] private float damageMultiplier = 1.2f;
        [Min(0.01f)] [SerializeField] private float rangeMultiplier = 1f;
        [Min(0.01f)] [SerializeField] private float attackRateMultiplier = 1f;

        public UnitDeploymentData TargetStaff { get { return targetStaff; } }
        public int ResourceCost { get { return resourceCost; } }
        public int MaximumLevel { get { return maximumLevel; } }
        public float DamageMultiplier { get { return damageMultiplier; } }
        public float RangeMultiplier { get { return rangeMultiplier; } }
        public float AttackRateMultiplier { get { return attackRateMultiplier; } }
    }
}
