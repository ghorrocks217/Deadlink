using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float jumpForce = 7f;
    public float groundCheckDistance = 0.6f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevent rotation from collisions
    }

    private void FixedUpdate()
    {
        UpdateGroundStatus();

        Vector3 inputDir = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) inputDir += transform.forward;
        if (Keyboard.current.sKey.isPressed) inputDir -= transform.forward;
        if (Keyboard.current.aKey.isPressed) inputDir -= transform.right;
        if (Keyboard.current.dKey.isPressed) inputDir += transform.right;

        inputDir = inputDir.normalized;

        float currentSpeed = moveSpeed;
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            currentSpeed *= sprintMultiplier;
        }

        // Project movement on slope
        Vector3 moveDir = Vector3.ProjectOnPlane(inputDir, groundNormal).normalized;
        Vector3 targetVelocity = moveDir * currentSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;

        rb.linearVelocity = velocity;
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            isGrounded = false;
        }
    }

    private void UpdateGroundStatus()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }
}
