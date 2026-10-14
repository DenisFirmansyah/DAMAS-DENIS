using UnityEngine;
using System.Collections;
using Core.PlayerState;
using DamasDenis.Stage;

public class TowerRotator : MonoBehaviour
{
    private bool _isAiming = false; // Start false to prevent input bleed
    private BuildSpot _linkedSpot;
    private int _buildCost;
    private StageState _stageState;

    // Call this immediately after instantiating
    public void Initialize(BuildSpot spot, int cost)
    {
        _linkedSpot = spot;
        _buildCost = cost;
        _stageState = StageState.Instance ?? FindObjectOfType<StageState>();
        
        // Start aiming after a brief delay to avoid "Instant Click" issues
        StartCoroutine(EnableAimingRoutine());
    }

    private IEnumerator EnableAimingRoutine()
    {
        // Wait one frame ensures the click that spawned this doesn't trigger the confirm
        yield return null; 
        _isAiming = true;
    }

    void Update()
    {
        if (!_isAiming) return;

        HandleRotation();
        HandleInput();
    }

    void HandleRotation()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;

        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // -90 offset assumes the sprite visually points UP. 
        // If sprite points RIGHT, remove the -90.
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }

    void HandleInput()
    {
        // CONFIRM
        if (Input.GetMouseButtonDown(0))
        {
            _isAiming = false;
            // Enable firing script here, e.g.:
            // GetComponent<TowerAttack>().enabled = true;
            
            // Clean up this script since we are done rotating
            Destroy(this); 
        } 
        // CANCEL
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuild();
        }
    }

    void CancelBuild()
    {
        // 1. Refund resources
        if (_stageState != null) 
        {
            _stageState.AddStageResources(_buildCost);
        }

        // 2. Free the spot
        if (_linkedSpot != null)
        {
            _linkedSpot.IsOccupied = false;
        }

        // 3. Destroy the tower
        Destroy(gameObject);
    }
}