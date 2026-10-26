using System.Collections.Generic;
using UnityEngine;
using DamasDenis.Deployment;
using DamasDenis.Stage;
using DamasDenis.Core;
using TMPro;

namespace DamasDenis.UI
{
    /// <summary>
    /// Unit Bar: Loads player staff set (unlocked units) and displays units that can be deployed.
    /// Filters by unlocked status and terrain compatibility.
    /// </summary>
    public class UnitBar : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("Which units this bar offers. Assign in the inspector.")]
        public StaffRoster roster;

        [Header("References")]
        public Transform unitSlotContainer;
        public UnitSlot unitSlotPrefab;

        [Header("Player State")]
        public PlayerState playerState;

        [Header("UI Elements")]
        public GameObject emptyStateText;

        private List<UnitSlot> _activeSlots = new List<UnitSlot>();
        private List<UnitDisplayData> _displayDataList = new List<UnitDisplayData>();
        private UnitDeploymentData _selectedUnit;

        /// <summary>Raised when the player taps a unit card. Subscribe to start placement.</summary>
        public event System.Action<UnitDeploymentData> UnitChosen;

        public UnitDeploymentData SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                _selectedUnit = value;
                foreach (var slot in _activeSlots)
                {
                    slot.SetSelected(slot.displayData.unitData == value);
                }
            }
        }

        private void Awake()
        {
            // Find PlayerState if not assigned
            if (playerState == null)
            {
                playerState = FindObjectOfType<PlayerState>();
            }
        }

        /// <summary>Rebuilds the bar from the roster's unlocked units, cheapest first.</summary>
        public void LoadPlayerStaffSet()
        {
            foreach (Transform child in unitSlotContainer)
                Destroy(child.gameObject);
            _activeSlots.Clear();
            _displayDataList.Clear();

            List<UnitDeploymentData> unlocked = roster != null
                ? roster.GetUnlocked()
                : new List<UnitDeploymentData>();

            // Sort the DATA first, then instantiate in that order.
            // Sibling order in the hierarchy is what the layout group displays.
            unlocked.Sort((a, b) => a.DeploymentCost.CompareTo(b.DeploymentCost));

            if (emptyStateText != null)
                emptyStateText.SetActive(unlocked.Count == 0);

            foreach (UnitDeploymentData unit in unlocked)
            {
                UnitSlot slot = Instantiate(unitSlotPrefab, unitSlotContainer);
                slot.Initialize(unit);
                slot.onSelect += OnSlotSelected;
                _activeSlots.Add(slot);
                _displayDataList.Add(slot.displayData);
            }
        }

        private void OnSlotSelected(UnitDeploymentData unit) => UnitChosen?.Invoke(unit);

        /// <summary>
        /// Get units available for deployment in the current stage.
        /// </summary>
        public List<UnitDeploymentData> GetDeployableUnits(StageDefinition stage)
        {
            List<UnitDeploymentData> deployable = new List<UnitDeploymentData>();

            foreach (var unit in _displayDataList)
            {
                if (!unit.isUnlocked) continue;

                // Check stage requirements (e.g., optional stage unlock)
                if (stage != null && stage.UnlockedStaff != null && unit.unitData.UnlockId == stage.UnlockedStaff.UnlockId)
                {
                    // This unit is the optional stage reward - only deploy if stage is completed
                    continue;
                }

                deployable.Add(unit.unitData);
            }

            return deployable;
        }

        /// <summary>
        /// Update UI with current player resources.
        /// </summary>

        public void UpdateAffordability(int currentResources)
        {
            foreach (UnitSlot slot in _activeSlots)
                slot.SetAffordable(currentResources >= slot.displayData.cost);
        }

        public void ShowEmptyState(string message)
        {
            emptyStateText?.SetActive(true);
            // emptyStateText?.GetComponent<TextMeshProUGUI>().text = message;
        }
    }
}
