using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerController for a Roll-a-Ball style game using the Unity Input System.
/// - Reads Vector2 move input from an InputActionReference (Move)
/// - Reads jump input from an InputActionReference (Jump)
/// - Reads look input from an InputActionReference (Look) and applies a simple camera pivot rotation
/// - Applies movement in FixedUpdate using Rigidbody (AddForce by default)
/// Attach this to the player (ball) GameObject and assign the Rigidbody and InputActionReferences in the inspector.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Rigidbody attached to the player (required)")]
    public Rigidbody rb;

    [Header("Movement")]
    [Tooltip("Movement speed multiplier")]
    public float speed = 10f;

    [Tooltip("Jump impulse force")]
    public float jumpForce = 5f;

    public enum MovementMode { AddForce, SetVelocity }
    [Tooltip("Choose AddForce for natural ball physics or SetVelocity for more direct control")]
    public MovementMode movementMode = MovementMode.AddForce;

    [Header("Input Actions (assign from InputSystem_Actions)")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference lookAction;

    [Header("Camera")]
    [Tooltip("Optional pivot transform for camera rotation. If set, movement will be relative to this pivot's forward/right.")]
    public Transform cameraPivot;

    [Tooltip("Lock cursor on enable")]
    public bool lockCursor = true;

    // Internal state
    private Vector2 moveInput = Vector2.zero;
    private bool jumpRequested = false;
    private Vector2 lookInput = Vector2.zero;

    void OnValidate()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    void OnEnable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.performed += OnMovePerformed;
            moveAction.action.canceled += OnMoveCanceled;
            moveAction.action.Enable();
        }

        if (jumpAction != null && jumpAction.action != null)
        {
            jumpAction.action.performed += OnJumpPerformed;
            jumpAction.action.Enable();
        }

        if (lookAction != null && lookAction.action != null)
        {
            lookAction.action.performed += OnLookPerformed;
            lookAction.action.canceled += OnLookCanceled;
            lookAction.action.Enable();
        }

        if (lockCursor)
            Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.performed -= OnMovePerformed;
            moveAction.action.canceled -= OnMoveCanceled;
            moveAction.action.Disable();
        }

        if (jumpAction != null && jumpAction.action != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
            jumpAction.action.Disable();
        }

        if (lookAction != null && lookAction.action != null)
        {
            lookAction.action.performed -= OnLookPerformed;
            lookAction.action.canceled -= OnLookCanceled;
            lookAction.action.Disable();
        }

        if (lockCursor)
            Cursor.lockState = CursorLockMode.None;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        // Only request a jump; the physics step will actually apply it
        if (ctx.performed)
            jumpRequested = true;
    }

    private void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext ctx)
    {
        lookInput = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        // Convert 2D input to world-space movement vector (XZ plane)
        Vector3 input3 = new Vector3(moveInput.x, 0f, moveInput.y);

        Vector3 worldMove;
        if (cameraPivot != null)
        {
            Vector3 forward = cameraPivot.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = cameraPivot.right;
            right.y = 0f;
            right.Normalize();
            worldMove = forward * input3.z + right * input3.x;
        }
        else
        {
            worldMove = input3;
        }

        ApplyMovement(worldMove);

        if (jumpRequested)
        {
            TryJump();
            jumpRequested = false;
        }
    }

    private void ApplyMovement(Vector3 worldMove)
    {
        if (movementMode == MovementMode.AddForce)
        {
            rb.AddForce(worldMove * speed, ForceMode.Force);
        }
        else // SetVelocity
        {
            Vector3 vel = rb.linearVelocity;
            Vector3 desired = worldMove * speed;
            vel.x = desired.x;
            vel.z = desired.z;
            rb.linearVelocity = vel;
        }
    }

    private void TryJump()
    {
        if (IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Basic grounded check using a short downward sphere cast.
    /// Tweak radius and distance to match your player scale.
    /// </summary>
    /// <returns>true if a hit is detected beneath the player</returns>
    private bool IsGrounded()
    {
        float radius = 0.5f;
        float maxDistance = 1.1f;
        return Physics.SphereCast(transform.position, radius, Vector3.down, out RaycastHit hit, maxDistance);
    }

    void Update()
    {
        // Apply look input to rotate player/camera. This is intentionally simple — tune sensitivities in the inspector or by multiplying lookInput.
        if (cameraPivot != null && lookInput != Vector2.zero)
        {
            float lookSpeed = 1f;
            // Yaw rotate the player
            transform.Rotate(0f, lookInput.x * lookSpeed, 0f, Space.World);
            // Pitch rotate the camera pivot (clamping may be desirable in a full implementation)
            cameraPivot.Rotate(-lookInput.y * lookSpeed, 0f, 0f, Space.Self);
            // Do not zero lookInput here; the input system will send canceled/performed events as appropriate
        }
    }
}

