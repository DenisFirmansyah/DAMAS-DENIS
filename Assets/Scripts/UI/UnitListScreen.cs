using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DamasDenis.Deployment;
using DamasDenis.Core;

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

        [Header("Upgrades")]
        [Tooltip("One definition per upgradable unit. Matched to units by TargetStaff.UnlockId.")]
        [SerializeField] private StaffUpgradeDefinition[] upgradeDefinitions;

        [Header("Credits")]
        [SerializeField] private TextMeshProUGUI creditsText;

        private readonly List<UnitListEntry> entries = new List<UnitListEntry>();
        private readonly List<UnitDisplayData> displayData = new List<UnitDisplayData>();
        private PlayerState player;
        private readonly Dictionary<string, StaffUpgradeDefinition> definitionByUnit = new Dictionary<string, StaffUpgradeDefinition>();
        private StaffUpgradeService upgradeService;

        protected override void Awake()
        {
            base.Awake();
            if (upgradeDefinitions == null) return;

            foreach (StaffUpgradeDefinition def in upgradeDefinitions)
                if (def != null && def.TargetStaff != null)
                    definitionByUnit[def.TargetStaff.UnlockId] = def;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            player = PlayerState.Instance;
            upgradeService = new StaffUpgradeService(player);

            Rebuild();

            if (creditsText != null) creditsText.text = player.ResearchCredits.ToString();
            player.OnResearchCreditsChanged -= OnCreditsChanged; // never double-subscribe
            player.OnResearchCreditsChanged += OnCreditsChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (player != null) player.OnResearchCreditsChanged -= OnCreditsChanged;
        }

        private void Rebuild()
        {
            foreach (UnitListEntry old in entries)
            {
                if (old == null) continue;
                old.gameObject.SetActive(false);
                Destroy(old.gameObject);
            }
            entries.Clear();
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
                entry.UpgradeRequested += HandleUpgradeRequested;
                ApplyUpgradeState(entry, data);
                entries.Add(entry);
            }

            if (unlockedCountText != null)
                unlockedCountText.text = $"{unlocked}/{displayData.Count}";
            if (emptyStateText != null)
                emptyStateText.SetActive(displayData.Count == 0);
        }

        private void RefreshEntries()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                displayData[i].Refresh();
                entries[i].Bind(displayData[i]);
                ApplyUpgradeState(entries[i], displayData[i]);
            }
        }

        private void ApplyUpgradeState(UnitListEntry entry, UnitDisplayData data)
        {
            bool hasDefinition = definitionByUnit.TryGetValue(data.unlockId, out StaffUpgradeDefinition def);
            if (!hasDefinition || !data.isUnlocked)
            {
                entry.SetUpgrade(false, data.upgradeLevel, 0, 0, false);
                return;
            }

            bool maxed = data.upgradeLevel >= def.MaximumLevel;
            bool canAfford = player.ResearchCredits >= def.ResourceCost;
            entry.SetUpgrade(true, data.upgradeLevel, def.MaximumLevel, def.ResourceCost, !maxed && canAfford);
        }

        private void HandleUpgradeRequested(UnitDisplayData data)
        {
            if (!definitionByUnit.TryGetValue(data.unlockId, out StaffUpgradeDefinition def)) return;
            if (!upgradeService.TryPurchase(def)) return;

            RefreshEntries();
        }

        private void OnCreditsChanged(int credits)
        {
            if (creditsText != null) creditsText.text = credits.ToString();
            RefreshEntries();
        }
    }
}