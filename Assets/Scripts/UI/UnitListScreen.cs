using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DamasDenis.Deployment;

namespace DamasDenis.UI
{
    /// <summary>
    /// Shows every unit in the StaffRoster, locked and unlocked, in roster order.
    /// The roster is the master list; UnitDisplayData supplies the values to draw.
    /// </summary>
    public sealed class UnitListScreen : UIScreen
    {
        [Header("Data")]
        [Tooltip("Master list of all units. Locked units are shown too.")]
        [SerializeField] private StaffRoster roster;

        [Header("UI")]
        [SerializeField] private Transform entryContainer;
        [SerializeField] private UnitListEntry entryPrefab;
        [SerializeField] private TextMeshProUGUI unlockedCountText;
        [SerializeField] private GameObject emptyStateText;

        private readonly List<UnitDisplayData> displayData = new List<UnitDisplayData>();

        protected override void OnEnable()
        {
            base.OnEnable();
            Rebuild();
        }

        private void Rebuild()
        {
            foreach (Transform child in entryContainer)
                Destroy(child.gameObject);
            displayData.Clear();

            if (roster == null || entryContainer == null || entryPrefab == null)
            {
                Debug.LogWarning("UnitListScreen needs a roster, entry container and entry prefab.", this);
                return;
            }

            int unlocked = 0;
            foreach (UnitDeploymentData unit in roster.Units)
            {
                if (unit == null) continue;

                UnitDisplayData data = new UnitDisplayData { unitData = unit };
                data.Refresh();
                displayData.Add(data);
                if (data.isUnlocked) unlocked++;

                UnitListEntry entry = Instantiate(entryPrefab, entryContainer);
                entry.Bind(data);
            }

            if (unlockedCountText != null)
                unlockedCountText.text = $"{unlocked}/{displayData.Count}";
            if (emptyStateText != null)
                emptyStateText.SetActive(displayData.Count == 0);
        }
    }
}