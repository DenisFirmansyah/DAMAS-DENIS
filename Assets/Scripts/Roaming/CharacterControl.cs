using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterControl : MonoBehaviour
{
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction interactAction;
    [SerializeField] private float speed = 5f;
    private Vector2 moveInput;

    private void OnEnable()
    {
        moveAction.Enable();
        interactAction.Enable();
        interactAction.performed += OnInteract;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        interactAction.Disable();
        interactAction.performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        // Handle interaction logic here
        Debug.Log("Interact action performed");
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        // Handle movement based on moveInput
        transform.Translate(new Vector3(moveInput.x, moveInput.y, 0) * speed * Time.deltaTime);
    }


}
