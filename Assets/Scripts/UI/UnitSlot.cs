using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DamasDenis.Deployment;

namespace DamasDenis.UI
{
    public class UnitSlot : MonoBehaviour
    {
        [Header("References")]
        public Image portraitImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI costText;
        public Button selectButton;
        // public Button upgradeButton;

        [Header("Visuals")]
        // public Image lockOverlay;
        public Image cooldownOverlay;
        // public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public Color cooldownColor = new Color(1f, 0.5f, 0f, 0.3f);
        public Color selectedColor = new Color(0.8f, 0.8f, 1f, 1f);
        public Color unaffordablePortraitColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        public Color normalCostColor = Color.white;
        public Color unaffordableCostColor = new Color(1f, 0.3f, 0.3f, 1f);

        [Header("Events")]
        public System.Action<UnitDeploymentData> onSelect;

        private UnitDisplayData _displayData;
        public UnitDisplayData displayData
        {
            get => _displayData;
            set
            {
                _displayData = value;
                if (_displayData != null) _displayData.Refresh();
                UpdateVisuals();
            }
        }

        private bool CanSelect =>
            displayData != null && displayData.isUnlocked &&
            !displayData.isOnCooldown && displayData.isAffordable;

        private void Awake()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(OnSelectClick);
        }

        public void Initialize(UnitDeploymentData unit)
        {
            displayData = new UnitDisplayData();
            displayData.unitData = unit;
            displayData.Refresh();
            UpdateVisuals();
        }

        public void SetAffordable(bool affordable)
        {
            if (displayData == null || displayData.isAffordable == affordable) return;
            displayData.isAffordable = affordable;
            UpdateVisuals();
        }

        private void Update()
        {
            if (displayData == null || displayData.unitData == null) return;

            bool cooling = StaffCooldowns.IsCoolingDown(displayData.unlockId);
            if (cooling == displayData.isOnCooldown) return;

            displayData.isOnCooldown = cooling;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (displayData == null) return;

            bool affordable = displayData.isAffordable;

            // Portrait
            if (portraitImage != null && displayData.unitData != null && displayData.unitData.UnitPrefab != null)
            {
                Sprite sprite = GetPortraitSprite();
                portraitImage.sprite = sprite;
                portraitImage.enabled = sprite != null;
                portraitImage.color = displayData.isSelected ? selectedColor
                                    : affordable ? Color.white
                                    : unaffordablePortraitColor;
            }
            else
            {
                portraitImage.enabled = false;
            }

            // Name
            nameText.text = displayData.displayName;

            // Cost
            costText.text = displayData.cost.ToString();

            // Cooldown overlay
            cooldownOverlay.color = displayData.isOnCooldown ? cooldownColor : Color.clear;

            // Button interactivity
            bool canInteract = displayData.isUnlocked && !displayData.isOnCooldown;
            selectButton.interactable = canInteract;

            // Highlight selected
            if (displayData.isSelected)
            {
                portraitImage.color = selectedColor;
            }
            else
            {
                portraitImage.color = displayData.isAffordable ? normalCostColor : unaffordablePortraitColor;
            }
        }

        private Sprite GetPortraitSprite()
        {
            if (displayData.unitData == null || displayData.unitData.UnitPrefab == null)
                return null;

            // Explicit null check: "?." skips Unity's destroyed-object check.
            SpriteRenderer sr = displayData.unitData.UnitPrefab.GetComponentInChildren<SpriteRenderer>();
            return sr != null ? sr.sprite : null;
        }

        public void OnSelectClick()
        {
            Debug.Log("slot clicked");
            if (!CanSelect) return;
            onSelect?.Invoke(displayData.unitData);

            Debug.Log("slot Chose");
        }

        public void SetSelected(bool selected)
        {
            displayData.isSelected = selected;
            UpdateVisuals();
        }
    }
}
