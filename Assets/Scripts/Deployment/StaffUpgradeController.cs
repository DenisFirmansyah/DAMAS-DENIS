using DamasDenis.Core;
using UnityEngine;

namespace DamasDenis.Deployment
{
    public static class StaffUpgradeStore
    {
        private const string Prefix = "DamasDenis.StaffUpgrade.";
        public static int GetLevel(string unlockId) => PlayerPrefs.GetInt(Prefix + unlockId, 0);
        public static void SetLevel(string unlockId, int level) { PlayerPrefs.SetInt(Prefix + unlockId, level); PlayerPrefs.Save(); }
    }

    // 2) Logic: plain class, no MonoBehaviour. The Lobby upgrade screen owns one.
    public sealed class StaffUpgradeService
    {
        private readonly PlayerState player;
        public StaffUpgradeService(PlayerState player) { this.player = player; }

        public bool TryPurchase(StaffUpgradeDefinition def)
        {
            if (def == null || def.TargetStaff == null || player == null) return false;
            string id = def.TargetStaff.UnlockId;
            int level = StaffUpgradeStore.GetLevel(id);
            if (level >= def.MaximumLevel || !player.TrySpendResearchCredits(def.ResourceCost)) return false;
            StaffUpgradeStore.SetLevel(id, level + 1);
            return true;
        }
    }
}
