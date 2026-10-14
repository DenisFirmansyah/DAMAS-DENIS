using UnityEngine;

namespace DamasDenis.Deployment
{
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

        public string DisplayName { get { return displayName; } }
        public string UnlockId { get { return unlockId; } }
        public GameObject UnitPrefab { get { return unitPrefab; } }
        public int DeploymentCost { get { return deploymentCost; } }
        public bool CanFace { get { return canFace; } }
        public bool IsMedic { get { return isMedic; } }
        public bool IsUnlocked { get { return unlockedByDefault || StaffUnlocks.IsUnlocked(unlockId); } }

        public bool CanDeployOn(CellTerrain terrain)
        {
            return (terrain == CellTerrain.Ground && deployOnGround) ||
                   (terrain == CellTerrain.Elevated && deployOnElevated);
        }
    }
}
