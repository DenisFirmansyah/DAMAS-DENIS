using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DamasDenis.UI
{
    public sealed class UnitListEntry : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI typeText;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Locked State")]
        [Tooltip("Shown only while the unit is locked (e.g. a padlock icon).")]
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private Color unlockedPortraitColor = Color.white;
        [SerializeField] private Color lockedPortraitColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        /// <summary>Draws the given display data. Call once after instantiating.</summary>
        public void Bind(UnitDisplayData data)
        {
            if (data == null) return;

            SetText(nameText, $"Name: {data.displayName}");
            SetText(typeText, $"Type: {data.typeLabel}");
            SetText(costText, $"Cost: {data.cost.ToString()}");
            SetText(damageText, $"Damage: {data.damage.ToString("F0")}");
            SetText(speedText, $"Speed: {data.attackSpeed.ToString("F1")}");
            SetText(healthText, $"Health: {data.maxHealth.ToString("F0")}");

            if (lockOverlay != null)
                lockOverlay.SetActive(!data.isUnlocked);

            if (portraitImage != null)
            {
                Sprite sprite = GetPortraitSprite(data);
                portraitImage.sprite = sprite;
                portraitImage.enabled = sprite != null;
                portraitImage.color = data.isUnlocked ? unlockedPortraitColor : lockedPortraitColor;
            }
        }

        private static void SetText(TextMeshProUGUI label, string value)
        {
            if (label != null) label.text = value;
        }

        private static Sprite GetPortraitSprite(UnitDisplayData data)
        {
            if (data.unitData == null || data.unitData.UnitPrefab == null) return null;
            SpriteRenderer sr = data.unitData.UnitPrefab.GetComponentInChildren<SpriteRenderer>();
            return sr != null ? sr.sprite : null;
        }
    }
}