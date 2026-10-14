using Core.PlayerState;
using UnityEngine;
using UnityEngine.EventSystems;
using DamasDenis.Stage;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Select a UnitDeploymentData from UI, click a valid MapCell, then rotate with Q/E or
    /// the arrow keys and confirm with the left mouse button. Right click/Escape cancels.
    /// </summary>
    public sealed class DeploymentController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private LayerMask mapCellLayer;
        [SerializeField] private StageState stageState;
        [SerializeField] private Transform unitContainer;

        [Header("Limits")]
        [Min(0)] [SerializeField] private int maximumDeployedUnits = 8;

        private UnitDeploymentData selectedUnit;
        private MapCell pendingCell;
        private Vector2Int pendingFacing = Vector2Int.up;
        private GameObject preview;
        public bool IsPlacing { get { return selectedUnit != null; } }
        public bool IsAwaitingDirection { get { return pendingCell != null; } }

        private void Awake()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;
            if (stageState == null)
                stageState = StageState.Instance;
        }

        private void Update()
        {
            if (!IsPlacing)
                return;

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
                return;
            }

            if (pendingCell == null)
                HandleCellSelection();
            else
                HandleDirectionSelection();
        }

        // Call this from a unit-card button's OnClick event.
        public void BeginPlacement(UnitDeploymentData unit)
        {
            CancelPlacement();
            if (unit == null || !unit.IsUnlocked || StaffCooldowns.IsCoolingDown(unit.UnlockId) || unit.UnitPrefab == null ||
                FindObjectsByType<DeployedUnit>(FindObjectsSortMode.None).Length >= maximumDeployedUnits)
                return;

            selectedUnit = unit;
        }

        public void CancelPlacement()
        {
            if (preview != null)
                Destroy(preview);

            preview = null;
            selectedUnit = null;
            pendingCell = null;
            pendingFacing = Vector2Int.up;
        }

        private void HandleCellSelection()
        {
            if (!Input.GetMouseButtonDown(0) || IsPointerOverUi())
                return;

            if (stageState == null)
                stageState = StageState.Instance;

            MapCell cell = GetCellUnderCursor();
            if (cell == null || !cell.CanHost(selectedUnit) || stageState == null ||
                stageState.CurrentStageResources < selectedUnit.DeploymentCost)
                return;

            pendingCell = cell;
            pendingFacing = Vector2Int.up;
            preview = Instantiate(selectedUnit.UnitPrefab, cell.transform.position, Quaternion.identity, unitContainer);
            SetPreviewState(preview, true);
            UpdatePreviewRotation();

            if (!selectedUnit.CanFace)
                ConfirmPlacement();
        }

        private void HandleDirectionSelection()
        {
            Vector2Int requestedFacing = ReadFacingInput();
            if (requestedFacing != Vector2Int.zero)
            {
                pendingFacing = requestedFacing;
                UpdatePreviewRotation();
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
                ConfirmPlacement();
        }

        private void ConfirmPlacement()
        {
            if (stageState == null)
                stageState = StageState.Instance;

            if (pendingCell == null || !pendingCell.CanHost(selectedUnit) ||
                stageState == null || !stageState.TrySpendStageResources(selectedUnit.DeploymentCost))
            {
                CancelPlacement();
                return;
            }

            if (preview != null)
                Destroy(preview);

            GameObject instance = Instantiate(selectedUnit.UnitPrefab, pendingCell.transform.position, Quaternion.identity, unitContainer);
            DeployedUnit deployedUnit = instance.GetComponent<DeployedUnit>();
            if (deployedUnit == null)
                deployedUnit = instance.AddComponent<DeployedUnit>();

            deployedUnit.Initialize(pendingCell, selectedUnit, pendingFacing);
            pendingCell.SetOccupant(deployedUnit);
            StageEventLog.Record("staff_deployed", selectedUnit.DisplayName);
            selectedUnit = null;
            pendingCell = null;
            preview = null;
        }

        private MapCell GetCellUnderCursor()
        {
            if (worldCamera == null)
                return null;

            Vector3 screenPosition = Input.mousePosition;
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, 0f, mapCellLayer);
            return hit.collider == null ? null : hit.collider.GetComponent<MapCell>();
        }

        private static Vector2Int ReadFacingInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) return Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return Vector2Int.right;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) return Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) return Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.Q)) return Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.E)) return Vector2Int.right;
            return Vector2Int.zero;
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void UpdatePreviewRotation()
        {
            if (preview != null)
                preview.transform.up = new Vector3(pendingFacing.x, pendingFacing.y, 0f);
        }

        private static void SetPreviewState(GameObject root, bool isPreview)
        {
            foreach (Collider2D collider in root.GetComponentsInChildren<Collider2D>())
                collider.enabled = !isPreview;

            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>())
            {
                if (!(behaviour is DeployedUnit))
                    behaviour.enabled = !isPreview;
            }
        }
    }
}
