using DamasDenis.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using DamasDenis.Stage;
using System.Collections.Generic;

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

        [Header("Range Preview")]
        [Tooltip("Marks the tiles the pending unit would cover. Don't share this instance with the selected-unit one.")]
        [SerializeField] private RangePreview rangePreview;
        [Tooltip("The mouse must be this far (world units) from the cell centre to change facing.")]
        [Min(0f)] [SerializeField] private float facingDeadZone = 0.3f;
        [Range(0.1f, 1f)] [SerializeField] private float previewAlpha = 0.5f;

        private Vector3 lastMouseScreenPos;

        [Header("Limits")]
        [Min(0)] [SerializeField] private int maximumDeployedUnits = 8;
        [SerializeField, Min(1f)] private float maxRaycastDistance = 1000f;

        private UnitDeploymentData selectedUnit;
        private MapCell pendingCell;
        private Vector2Int pendingFacing = Vector2Int.up;
        private GameObject preview;

        public Transform UnitContainer
        {
            get => unitContainer;
            set => unitContainer = value;
        }

        public void SetUnitContainer(Transform container)
        {
            unitContainer = container;
        }

        public bool IsPlacing { get { return selectedUnit != null; } }
        public bool IsAwaitingDirection { get { return pendingCell != null; } }
        public event System.Action<UnitDeploymentData> PlacingChanged;
        public MapCell PickCellUnderCursor() => GetCellUnderCursor();

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

        private void LateUpdate()
        {
            if (rangePreview == null) return;

            // Derived from state every frame, so it can't go stale however placement ends.
            if (selectedUnit != null && pendingCell != null)
            {
                Vector2Int facing = selectedUnit.CanFace ? pendingFacing : Vector2Int.up;
                rangePreview.Show(selectedUnit.RangePattern, pendingCell.Coordinates, facing);
            }
            else
            {
                rangePreview.Hide();
            }
        }

        // Call this from a unit-card button's OnClick event.
        public void BeginPlacement(UnitDeploymentData unit)
        {
            if (unit != null && unit == selectedUnit)
            {
                CancelPlacement();
                return;
            }

            CancelPlacement();
            if (unit == null || !unit.IsUnlocked || StaffCooldowns.IsCoolingDown(unit.UnlockId) || unit.UnitPrefab == null ||
                FindObjectsByType<DeployedUnit>(FindObjectsSortMode.None).Length >= maximumDeployedUnits)
                return;

            selectedUnit = unit;
            PlacingChanged?.Invoke(selectedUnit);
        }

        public void CancelPlacement()
        {
            if (preview != null)
                Destroy(preview);

            bool wasPlacing = selectedUnit != null;
            preview = null;
            selectedUnit = null;
            pendingCell = null;
            pendingFacing = Vector2Int.up;

            if (wasPlacing) PlacingChanged?.Invoke(null);
        }

        private void HandleCellSelection()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            if (IsPointerOverUi())
            {
                Debug.Log("[Deploy] Blocked: pointer is over a UI element with Raycast Target.");
                return;
            }

            if (stageState == null)
                stageState = StageState.Instance;

            if (stageState == null)
            {
                Debug.Log("[Deploy] Blocked: StageState.Instance is null.");
                return;
            }

            MapCell cell = GetCellUnderCursor();
            if (cell == null)
            {
                Debug.Log("[Deploy] Blocked: no MapCell under cursor (collider, layer, mask, or camera).");
                return;
            }

            if (!cell.CanHost(selectedUnit))
            {
                Debug.Log($"[Deploy] Blocked: cell {cell.name} can't host {selectedUnit.DisplayName} " +
                        $"(terrain={cell.Terrain}, allowDeployment={cell.AllowDeployment}, occupied={cell.IsOccupied}).");
                return;
            }

            if (stageState.CurrentStageResources < selectedUnit.DeploymentCost)
            {
                Debug.Log($"[Deploy] Blocked: resources {stageState.CurrentStageResources} < cost {selectedUnit.DeploymentCost}.");
                return;
            }

            pendingCell = cell;
            pendingFacing = Vector2Int.up;
            lastMouseScreenPos = Input.mousePosition;
            preview = Instantiate(selectedUnit.UnitPrefab, cell.SurfacePoint, Quaternion.identity, unitContainer);
            SetPreviewState(preview, true);
            // AttachFacingIndicator();
            UpdatePreviewRotation();

            if (!selectedUnit.CanFace)
                ConfirmPlacement();
        }

        private void HandleDirectionSelection()
        {
            Vector2Int requested = ReadFacingInput(); // keyboard wins when pressed

            Vector3 mouse = Input.mousePosition;
            bool mouseMoved = mouse != lastMouseScreenPos;
            lastMouseScreenPos = mouse;

            // Only let the mouse drive facing when it actually moved,
            // otherwise it would instantly undo a keyboard choice.
            if (requested == Vector2Int.zero && mouseMoved)
                requested = ReadMouseFacing();

            if (requested != Vector2Int.zero && requested != pendingFacing)
            {
                pendingFacing = requested;
                UpdatePreviewRotation();
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
                ConfirmPlacement();
        }

        /// <summary>Cardinal direction from the pending cell toward the mouse, or zero inside the dead zone.</summary>
        
        private Vector2Int ReadMouseFacing()
        {
            if (worldCamera == null || pendingCell == null)
                return Vector2Int.zero;

            Plane groundPlane = new Plane(Vector3.up, pendingCell.SurfacePoint);

            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);

            if (!groundPlane.Raycast(ray, out float distance))
                return Vector2Int.zero;

            Vector3 mouseWorld = ray.GetPoint(distance);
            Vector3 offset = mouseWorld - pendingCell.SurfacePoint;

            if (offset.magnitude < facingDeadZone)
                return Vector2Int.zero;

            // Logical grid directions: XZ plane, not XY.
            if (Mathf.Abs(offset.x) > Mathf.Abs(offset.z))
                return offset.x > 0f
                    ? Vector2Int.right
                    : Vector2Int.left;

            return offset.z > 0f
                ? Vector2Int.up
                : Vector2Int.down;
        }

        // private void AttachFacingIndicator()
        // {
        //     if (facingIndicatorPrefab == null || preview == null || !selectedUnit.CanFace)
        //         return;

        //     // Parented to the preview, so it rotates with it. Local offset comes from the prefab.
        //     Instantiate(facingIndicatorPrefab, preview.transform);
        // }

        private void SetPreviewState(GameObject root, bool isPreview)
        {
            foreach (Collider col in root.GetComponentsInChildren<Collider>())
                col.enabled = !isPreview;

            foreach (MonoBehaviour behaviour in
                    root.GetComponentsInChildren<MonoBehaviour>())
            {
                if (!(behaviour is DeployedUnit) && !(behaviour is Billboard))
                    behaviour.enabled = !isPreview;
            }

            if (!isPreview)
                return;

            foreach (SpriteRenderer sr in root.GetComponentsInChildren<SpriteRenderer>())
            {
                Color color = sr.color;
                color.a = previewAlpha;
                sr.color = color;
            }
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

            GameObject instance = Instantiate(selectedUnit.UnitPrefab, pendingCell.SurfacePoint, Quaternion.identity, unitContainer);
            DeployedUnit deployedUnit = instance.GetComponent<DeployedUnit>();
            if (deployedUnit == null)
                deployedUnit = instance.AddComponent<DeployedUnit>();

            deployedUnit.Initialize(pendingCell, selectedUnit, pendingFacing);
            pendingCell.SetOccupant(deployedUnit);
            StageEventLog.Record("staff_deployed", selectedUnit.DisplayName);
            selectedUnit = null;
            pendingCell = null;
            preview = null;
            PlacingChanged?.Invoke(null);
        }

        private MapCell GetCellUnderCursor()
        {
            if (worldCamera == null) return null;

            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance,
                    mapCellLayer, QueryTriggerInteraction.Collide))
                return null; // Clicking empty space is normal, so no warning.

            // The collider can sit on a child of the tile, so search upward.
            MapCell cell = hit.collider.GetComponentInParent<MapCell>();
            if (cell == null)
                Debug.LogWarning($"[Deploy] '{hit.collider.name}' is on the Map Cell layer but has no MapCell.");

            return cell;
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
            if (preview == null)
                return;

            Vector3 direction = new Vector3(
                pendingFacing.x,
                0f,
                pendingFacing.y
            );

            if (direction.sqrMagnitude > 0f)
            {
                preview.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

    }
}
