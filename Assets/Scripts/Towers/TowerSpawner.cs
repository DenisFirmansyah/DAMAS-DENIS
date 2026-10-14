using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Core.PlayerState;
using DamasDenis.Stage;

namespace Towers
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class TowerSpawner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Tower Settings")]
        public GameObject towerPrefab;
        public LayerMask placementLayer;

        [Header("UI References")]
        private RectTransform _rect;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private Vector2 _startPos;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPos = _rect.anchoredPosition;
            
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas == null) return;
            _rect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;

            if (!TrySpawnTowerAtCursor())
            {
                SnapBack(); 
            }
        }

        private bool TrySpawnTowerAtCursor()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f, placementLayer);

            if (hit.collider != null)
            {
                return ProcessSpawnRequest(hit.collider.gameObject, hit.point);
            }

            Debug.Log("Failed: Dropped in void (No valid ground hit).");
            return false;
        }

        private bool ProcessSpawnRequest(GameObject hitObject, Vector3 hitPoint)
        {
            StageState stageState = StageState.Instance ?? FindObjectOfType<StageState>();
            if (stageState == null) return false;

            if (!towerPrefab.TryGetComponent(out TowerInstance towerInstance)) return false;
            int cost = towerInstance.towerData.cost;

            if (!hitObject.TryGetComponent(out BuildSpot spot)) return false;

            if (spot.IsOccupied)
            {
                Debug.Log("Cannot build here: Spot is already taken.");
                return false;
            }

            if (spot.type != towerInstance.towerData.type)
            {
                Debug.Log($"Cannot build here: Spot type {spot.type} incompatible with tower type {towerInstance.towerData.type}.");
                return false;
            }

            if (stageState.TrySpendStageResources(cost))
            {
                GameObject newTower = Instantiate(towerPrefab, hitObject.transform);
                newTower.transform.localPosition = Vector3.zero;

                spot.IsOccupied = true;

                if (towerInstance.towerData.requiresRotation)
                {
                    Debug.Log("tesss");
                    TowerRotator rotator = newTower.GetComponent<TowerRotator>();
                    if (rotator != null)
                    {
                        rotator.enabled = true;
                        // VITAL: Pass the spot and cost so Rotator can undo if cancelled
                        rotator.Initialize(spot, cost); 
                    }
                }
                else
                {
                    // If no rotation needed, enable the attack script immediately here
                    // newTower.GetComponent<TowerAttack>().enabled = true;
                }
                Debug.Log($"Success: Spawned {towerPrefab.name}");
                
                SnapBack(); 
                return true;
            }
            else
            {
                Debug.Log("Failed: Not enough resources.");
                return false;
            }
        }

        private void SnapBack()
        {
            _rect.anchoredPosition = _startPos;
        }
    }
}