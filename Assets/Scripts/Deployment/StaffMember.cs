using UnityEngine;
using DamasDenis.Stage;

namespace DamasDenis.Deployment
{
    /// <summary>Runtime staff state. Attach alongside StaffCombat when this staff attacks.</summary>
    public sealed class StaffMember : MonoBehaviour
    {
        public UnitDeploymentData Data { get; private set; }
        public int UpgradeLevel { get; private set; }

        public void Initialize(UnitDeploymentData data)
        {
            Data = data;
        }

        public bool TryApplyUpgrade(StaffUpgradeDefinition upgrade)
        {
            if (upgrade == null || Data != upgrade.TargetStaff || UpgradeLevel >= upgrade.MaximumLevel)
                return false;

            StaffCombat combat = GetComponent<StaffCombat>();
            if (combat != null)
                combat.ApplyStageUpgrade(upgrade.DamageMultiplier, upgrade.AttackRateMultiplier);

            UpgradeLevel++;
            StageEventLog.Record("staff_upgraded", Data.DisplayName + " level " + UpgradeLevel);
            return true;
        }
    }
}
