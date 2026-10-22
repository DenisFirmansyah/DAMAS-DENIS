using UnityEngine;
using UnityEngine.EventSystems;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Click a deployed unit to select it and show its attack range. Click empty ground,
    /// right-click or press Escape to deselect. Idle while a unit is being placed.
    /// </summary>
    public sealed class UnitSelector : MonoBehaviour
    {
        [SerializeField] private DeploymentController deployment;
        [SerializeField] private RangePreview rangePreview;

        private bool hasSelection;
        private int ignoreClickFrame = -1;

        /// <summary>The selected unit, or null.</summary>
        public DeployedUnit Selected { get; private set; }

        /// <summary>Raised with the newly selected unit, or null when the selection closes.</summary>
        public event System.Action<DeployedUnit> SelectionChanged;

        private void OnEnable()  => deployment.PlacingChanged += OnPlacingChanged;
        private void OnDisable() => deployment.PlacingChanged -= OnPlacingChanged;

        private void Update()
        {
            // A selected unit that died or retreated is destroyed: close the panel.
            if (hasSelection && Selected == null)
            {
                Clear();
                return;
            }

            if (deployment.IsPlacing || Time.frameCount == ignoreClickFrame) return;

            if (hasSelection && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
            {
                Clear();
                return;
            }

            if (!Input.GetMouseButtonDown(0)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            MapCell cell = deployment.PickCellUnderCursor();
            Select(cell != null ? cell.Occupant : null);   // empty tile or empty space deselects
        }

        private void OnPlacingChanged(UnitDeploymentData unit)
        {
            // The click that ends placement must not also select the unit it just placed.
            ignoreClickFrame = Time.frameCount;
            if (unit != null) Clear();   // starting a placement closes the panel
        }

        private void Select(DeployedUnit unit)
        {
            if (unit == null)
            {
                if (hasSelection) Clear();
                return;
            }
            if (unit == Selected) return;

            Selected = unit;
            hasSelection = true;
            if (unit.Cell != null && unit.Data != null)
                rangePreview.Show(unit.Data.RangePattern, unit.Cell.Coordinates, unit.EffectiveFacing);
            else
                rangePreview.Hide();

            SelectionChanged?.Invoke(unit);
        }

        private void Clear()
        {
            Selected = null;
            hasSelection = false;
            rangePreview.Hide();
            SelectionChanged?.Invoke(null);
        }
    }
}