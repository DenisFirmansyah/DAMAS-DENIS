using DamasDenis.Deployment;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DamasDenis.UI
{
    /// <summary>Shows the selected unit's name and health, with the Medevac button and its charges.</summary>
    public sealed class SelectedUnitPanel : MonoBehaviour
    {
        [SerializeField] private UnitSelector selector;
        [SerializeField] private Medevac medevac;
        [Tooltip("The child object that is shown and hidden. NOT the object this script is on.")]
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image healthFill;
        [FormerlySerializedAs("retreatButton")]
        [SerializeField] private Button medevacButton;
        [SerializeField] private TextMeshProUGUI chargesText;
        // [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image retreatFill;

        private DeployedUnit unit;
        private StaffVitality vitality;
        private StaffRetreatController retreat;

        private void Awake()
        {
            medevacButton.onClick.AddListener(OnMedevacClicked);
            root.SetActive(false);
        }

        private void OnEnable()  => selector.SelectionChanged += OnSelectionChanged;
        private void OnDisable() => selector.SelectionChanged -= OnSelectionChanged;

        private void OnSelectionChanged(DeployedUnit selected)
        {
            unit = selected;
            root.SetActive(selected != null);
            if (selected == null) return;

            nameText.text = selected.Data != null ? selected.Data.DisplayName : "Unit";
            vitality = selected.GetComponent<StaffVitality>();
            retreat = selected.GetComponent<StaffRetreatController>();
            medevacButton.gameObject.SetActive(retreat != null);
        }

        private void Update()
        {
            if (unit == null) return;

            if (vitality != null) healthFill.fillAmount = vitality.HealthFraction;
            if (retreat == null) return;

            MedevacBlock block = medevac.Check(retreat);
            medevacButton.interactable = block == MedevacBlock.None;
            retreatFill.fillAmount = retreat.Progress01;
            // statusText.text = Describe(block);

            string recharge = medevac.Charges < medevac.MaxCharges
                ? $"  (+1 in {Mathf.CeilToInt(medevac.SecondsToNextCharge)}s)" : "";
            chargesText.text = $"{medevac.Charges}/{medevac.MaxCharges}{recharge}";
        }

        private static string Describe(MedevacBlock block)
        {
            switch (block)
            {
                case MedevacBlock.None:              return "Ready";
                case MedevacBlock.AlreadyEvacuating: return "Evacuating...";
                case MedevacBlock.NoCharges:         return "No charges";
                case MedevacBlock.AtCapacity:        return "Medevac busy";
                default:                             return "";
            }
        }

        // The only place a retreat is requested. Medevac decides whether it happens.
        private void OnMedevacClicked() => medevac.TryEvacuate(retreat);
    }
}