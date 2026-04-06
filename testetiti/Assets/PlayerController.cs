using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Drag the 'Player/Move' action from your Input Action Asset here (InputActionReference)")]
    public InputActionReference moveAction;

    [Header("Movement")]
    [Tooltip("Force multiplier applied every FixedUpdate")]
    public float speed = 10f;
    [Tooltip("Maximum horizontal speed (m/s)")]
    public float maxSpeed = 5f;

    [Header("References")]
    [Tooltip("Camera used to orient movement. If none set, Camera.main will be used.")]
    public Camera mainCamera;

    Rigidbody rb;
    Vector2 moveInput = Vector2.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void OnEnable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMove;
            moveAction.action.canceled += OnMove;
        }
    }

    void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.performed -= OnMove;
            moveAction.action.canceled -= OnMove;
            moveAction.action.Disable();
        }
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // If action reference wasn't assigned or is null, try to read safely
        if (moveAction == null || moveAction.action == null)
        {
            // nothing to read
            return;
        }

        // Read current value (in case callback wasn't invoked yet)
        moveInput = moveAction.action.ReadValue<Vector2>();

        // Convert 2D input to world-space direction relative to the camera
        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;
        if (mainCamera != null)
        {
            camForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
            camRight = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
        }

        Vector3 desiredDir = camRight * moveInput.x + camForward * moveInput.y;
        if (desiredDir.sqrMagnitude > 1f)
            desiredDir.Normalize();

        // Apply force
        rb.AddForce(desiredDir * speed, ForceMode.Force);

        // Limit horizontal speed while preserving vertical velocity
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float hSpeed = horizontalVel.magnitude;
        if (hSpeed > maxSpeed)
        {
            Vector3 limited = horizontalVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
        }
    }

    // Optional: quick editor helper to ensure user sees missing assignment
    void Reset()
    {
        // Try to auto-assign main camera when adding component
        if (mainCamera == null)
            mainCamera = Camera.main;
    }
}