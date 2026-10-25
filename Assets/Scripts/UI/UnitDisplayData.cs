using UnityEngine;
using DamasDenis.Deployment;

namespace DamasDenis.UI
{
    /// <summary>
    /// UI-ready wrapper for UnitDeploymentData.
    /// Holds display state (locked/unlocked, selected, on cooldown, upgrade level)
    /// so the unit bar can render without touching data assets.
    /// </summary>
    [System.Serializable]
    public class UnitDisplayData
    {
        public UnitDeploymentData unitData;
        public string displayName;
        public int cost;
        public float damage;
        public float range;
        public float attackSpeed;
        public float maxHealth;
        public bool isMedic;
        public bool isUnlocked;
        public bool isSelected;
        public bool isOnCooldown;
        public int upgradeLevel;
        public string unlockId;
        public string typeLabel;
        public bool isAffordable = true;

        public void Refresh()
        {
            displayName = unitData != null ? unitData.DisplayName : "Unknown";
            cost = unitData != null ? unitData.DeploymentCost : 0;
            damage = unitData != null ? unitData.Damage : 0f;
            range  = unitData != null && unitData.RangePattern != null ? unitData.RangePattern.CellCount : 0;
            attackSpeed = unitData != null ? unitData.AttackSpeed : 0f;
            maxHealth = unitData != null ? unitData.MaxHealth : 0f;
            isMedic = unitData != null ? unitData.IsMedic : false;
            isUnlocked = unitData != null ? unitData.IsUnlocked : false;
            unlockId = unitData != null ? unitData.UnlockId : "";
            typeLabel = unitData != null ? unitData.AttackType.ToString() : "Unknown";
            isOnCooldown = unitData != null && StaffCooldowns.IsCoolingDown(unitData.UnlockId);

            if (unitData != null)
            {
                // Check upgrade level from deployed staff (optional - for UI showing current level)
                // For now, we just show the base data
            }
        }

        public override string ToString()
        {
            return $"{displayName} ({(isUnlocked ? "Unlocked" : "Locked")})";
        }
    }
}
