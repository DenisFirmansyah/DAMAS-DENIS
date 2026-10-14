using Core.PlayerState;
using UnityEngine;

namespace DamasDenis.Deployment
{
    public sealed class StaffUpgradeController : MonoBehaviour
    {
        [SerializeField] private PlayerState playerState;

        private void Awake()
        {
            if (playerState == null) playerState = PlayerState.Instance;
        }

        public bool TryUpgrade(StaffMember staff, StaffUpgradeDefinition upgrade)
        {
            if (playerState == null) playerState = PlayerState.Instance;

            if (staff == null || upgrade == null || playerState == null ||
                !playerState.TrySpendResearchCredits(upgrade.ResourceCost))
                return false;

            if (staff.TryApplyUpgrade(upgrade))
                return true;

            playerState.AddResearchCredits(upgrade.ResourceCost);
            return false;
        }
    }
}
