using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float slideSpeed = 10f;
    public float jumpForce = 7f;

    [Header("Slide Settings")]
    public float slideDuration = 1f;
    public float slideEndSpeedThreshold = 0.5f;
    public float slideHeight = 1f;
    public float slideTiltAngle = 15f;

    [Header("References")]
    public Transform cameraTransform;
    public ParticleSystem slideParticles;
    public AudioSource slideSound;

    private Rigidbody rb;
    private CapsuleCollider col;

    private bool isGrounded;
    private bool isSliding;
    private float slideTimer;

    private float originalHeight;
    private Vector3 originalCenter;
    private Quaternion originalCamRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        originalHeight = col.height;
        originalCenter = col.center;

        if (cameraTransform != null)
            originalCamRotation = cameraTransform.localRotation;
    }

    void FixedUpdate()
    {
        Vector3 move = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) move += transform.forward;
        if (Keyboard.current.sKey.isPressed) move -= transform.forward;
        if (Keyboard.current.aKey.isPressed) move -= transform.right;
        if (Keyboard.current.dKey.isPressed) move += transform.right;

        move.Normalize();

        float currentSpeed = isSliding ? slideSpeed :
                             (Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed);

        Vector3 velocity = new Vector3(move.x * currentSpeed, rb.linearVelocity.y, move.z * currentSpeed);
        rb.linearVelocity = velocity;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isSliding)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            isGrounded = false;
        }

        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame && isGrounded && rb.linearVelocity.magnitude > 3f)
        {
            StartSlide();
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f || rb.linearVelocity.magnitude < slideEndSpeedThreshold)
            {
                StopSlide();
            }

            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(slideTiltAngle, 0f, 0f);
            }
        }
    }

    void StartSlide()
    {
        if (isSliding) return;

        isSliding = true;
        slideTimer = slideDuration;

        col.height = slideHeight;
        col.center = new Vector3(originalCenter.x, slideHeight / 2f, originalCenter.z);

        if (slideParticles != null) slideParticles.Play();
        if (slideSound != null) slideSound.Play();
    }

    void StopSlide()
    {
        if (!isSliding) return;

        isSliding = false;

        col.height = originalHeight;
        col.center = originalCenter;

        if (cameraTransform != null)
            cameraTransform.localRotation = originalCamRotation;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
}
