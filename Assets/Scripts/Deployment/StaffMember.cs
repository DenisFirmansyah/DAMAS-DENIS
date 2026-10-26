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
            UpgradeLevel = StaffUpgradeStore.GetLevel(data.UnlockId);
            StaffUpgradeDefinition up = data.Upgrade;
            if (up != null && UpgradeLevel > 0)
                GetComponent<StaffCombat>()?.ApplyStageUpgrade(
                    Mathf.Pow(up.DamageMultiplier, UpgradeLevel),
                    Mathf.Pow(up.AttackRateMultiplier, UpgradeLevel));
        }
    }
}
