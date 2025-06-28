using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform groundCheck;
    public LayerMask groundMask;
    public LayerMask wallMask;
    public LayerMask vaultMask;
    public Camera playerCamera;
    private CharacterController controller;
    private AudioSource audioSource;

    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;
    public float slideSpeed = 10f;
    public float acceleration = 10f;
    public float deceleration = 15f;

    private float targetSpeed;
    private float currentSpeed;

    [Header("Crouch & Slide")]
    public float crouchHeight = 1f;
    private float originalHeight;
    public float slideDuration = 0.75f;
    private bool isSliding;
    private Vector3 slideDirection;
    private Vector3 slideMomentum;
    public float slideMomentumDecay = 1.5f;

    [Header("Jumping & Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;
    public float groundDistance = 0.4f;
    private Vector3 velocity;
    private bool isGrounded;

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Air Control")]
    public float airControlPercent = 0.5f;

    [Header("Step Climbing")]
    public float stepHeight = 0.3f;

    [Header("Camera Effects")]
    public float bobFrequency = 10f;
    public float bobHeight = 0.05f;
    private float bobTimer;

    public float cameraTiltAmount = 5f;
    private float currentTilt;
    private float tiltVelocity;
    public float tiltSmoothTime = 0.1f;

    public float fovNormal = 60f;
    public float fovSprint = 70f;
    public float fovChangeSpeed = 5f;

    [Header("Keybinds")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode slideKey = KeyCode.LeftControl;
    public KeyCode dashKey = KeyCode.E;

    [Header("Dash")]
    public bool dashUnlocked = false;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing = false;

    [Header("Wall Running")]
    public float wallRunDuration = 1.2f;
    public float wallRunGravity = -2f;
    private bool isWallRunning = false;
    private Vector3 wallNormal;

    [Header("Vaulting")]
    public float vaultDistance = 1.5f;
    public float vaultHeight = 1.2f;
    public float vaultDuration = 0.5f;
    private bool isVaulting = false;

    [Header("Fall Damage")]
    public float fallDamageThreshold = 15f;  // velocity threshold
    public float maxFallDamage = 50f;

    [Header("Surface Physics Materials (Example)")]
    public PhysicsMaterial slipperyMaterial;
    public PhysicsMaterial normalMaterial;
    private Collider playerCollider;

    private float horizontal;
    private float vertical;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        playerCollider = GetComponent<Collider>();

        originalHeight = controller.height;
        currentSpeed = walkSpeed;

        if (playerCamera != null)
            playerCamera.fieldOfView = fovNormal;
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleInput();

        if (!isVaulting && !isDashing)
        {
            HandleMovement();
            ApplyGravity();
        }

        HandleCameraEffects();
        HandleSurfacePhysics();

        if (dashUnlocked && Input.GetKeyDown(dashKey) && canDash && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    private bool IsNextToWall()
    {
        return Physics.Raycast(transform.position, orientation.right, 1f, wallMask) ||
            Physics.Raycast(transform.position, -orientation.right, 1f, wallMask);
    }


    private void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            velocity.y = -2f;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (Input.GetKeyDown(slideKey) && !isSliding && isGrounded && new Vector2(horizontal, vertical).magnitude > 0.1f)
        {
            StartCoroutine(StartSlide());
        }

        if (isGrounded && !isVaulting && !isWallRunning && !isDashing && !isSliding)
        {
            if (TryVault())
                return;

            if (TryWallRun())
                return;
        }

        if (isWallRunning)
        {
            if (!Input.GetKey(sprintKey) || !IsNextToWall())
            {
                StopWallRun();
            }
        }
    }

    private void HandleMovement()
    {
        if (isSliding)
        {
            SlideMove();
            return;
        }
        if (isWallRunning)
        {
            WallRunMove();
            return;
        }
        if (isVaulting)
        {
            // Vault handled in coroutine
            return;
        }

        Vector3 inputDirection = orientation.forward * vertical + orientation.right * horizontal;
        inputDirection.Normalize();

        if (Input.GetKey(crouchKey))
            targetSpeed = crouchSpeed;
        else if (Input.GetKey(sprintKey))
            targetSpeed = sprintSpeed;
        else
            targetSpeed = walkSpeed;

        float speedDiff = targetSpeed - currentSpeed;
        float accelRate = (Mathf.Abs(speedDiff) > 0.1f) ? (speedDiff > 0 ? acceleration : deceleration) : 0f;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.deltaTime);

        float moveSpeed = isGrounded ? currentSpeed : currentSpeed * airControlPercent;
        slideMomentum = Vector3.Lerp(slideMomentum, Vector3.zero, slideMomentumDecay * Time.deltaTime);

        Vector3 move = inputDirection * moveSpeed + slideMomentum;
        float maxSpeed = sprintSpeed + slideSpeed;
        if (move.magnitude > maxSpeed)
            move = move.normalized * maxSpeed;

        StepClimb();
        controller.Move(move * Time.deltaTime);

        // Jumping
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            PlayJumpSound();
        }
    }

    private IEnumerator StartSlide()
    {
        isSliding = true;
        controller.height = crouchHeight;

        Vector3 inputDirection = orientation.forward * vertical + orientation.right * horizontal;
        slideDirection = inputDirection.normalized;
        if (slideDirection.magnitude == 0)
            slideDirection = orientation.forward;

        float elapsed = 0f;
        while (elapsed < slideDuration && isSliding)
        {
            controller.Move(slideDirection * slideSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        controller.height = originalHeight;
        isSliding = false;
        slideMomentum = slideDirection * slideSpeed;
    }

    private void SlideMove()
    {
        controller.Move(slideDirection * slideSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (isWallRunning)
            velocity.y = Mathf.Max(velocity.y + wallRunGravity * Time.deltaTime, wallRunGravity);
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (!isGrounded && velocity.y < -fallDamageThreshold)
        {
            float damage = Mathf.InverseLerp(fallDamageThreshold, fallDamageThreshold * 2, -velocity.y) * maxFallDamage;
            if (damage > 0)
            {
                // TODO: Apply fall damage to player health here
                Debug.Log($"Fall Damage Taken: {damage:F1}");
            }
        }
    }

    private void StepClimb()
    {
        if (!isGrounded) return;

        Vector3 origin = transform.position + Vector3.up * (stepHeight + 0.1f);
        Vector3 direction = controller.velocity.normalized;
        float rayLength = controller.radius + 0.1f;

        if (direction.magnitude < 0.1f) return;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, rayLength))
        {
            float stepHeightCheck = hit.point.y - transform.position.y;
            if (stepHeightCheck > 0 && stepHeightCheck <= stepHeight)
            {
                controller.Move(Vector3.up * stepHeightCheck);
            }
        }
    }

    private void HandleCameraEffects()
    {
        if (playerCamera == null) return;

        // Head bob
        if (isGrounded && controller.velocity.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float bobAmount = Mathf.Sin(bobTimer) * bobHeight;
            playerCamera.transform.localPosition = new Vector3(0, bobAmount, 0);
        }
        else
        {
            bobTimer = 0f;
            playerCamera.transform.localPosition = Vector3.zero;
        }

        // Camera tilt on strafing
        float targetTilt = -horizontal * cameraTiltAmount;
        currentTilt = Mathf.SmoothDamp(currentTilt, targetTilt, ref tiltVelocity, tiltSmoothTime);
        playerCamera.transform.localRotation = Quaternion.Euler(currentTilt, 0, 0);

        // FOV kick sprinting
        float targetFOV = Input.GetKey(sprintKey) ? fovSprint : fovNormal;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
    }

    private void HandleSurfacePhysics()
    {
        // Example: Swap physics material based on ground type
        if (!isGrounded) return;

        RaycastHit hit;
        if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, groundDistance + 0.1f))
        {
            string tag = hit.collider.tag;
            if (tag == "Slippery")
                playerCollider.material = slipperyMaterial;
            else
                playerCollider.material = normalMaterial;
        }
    }

    private bool TryWallRun()
    {
        if (isWallRunning) return false;

        if (Physics.Raycast(transform.position, orientation.right, out RaycastHit rightHit, 1f, wallMask))
        {
            StartCoroutine(WallRunRoutine(rightHit.normal));
            return true;
        }
        else if (Physics.Raycast(transform.position, -orientation.right, out RaycastHit leftHit, 1f, wallMask))
        {
            StartCoroutine(WallRunRoutine(leftHit.normal));
            return true;
        }
        return false;
    }

    private IEnumerator WallRunRoutine(Vector3 wallNormalParam)
    {
        isWallRunning = true;
        wallNormal = wallNormalParam;
        float timer = 0f;

        while (timer < wallRunDuration && isWallRunning)
        {
            // Push player slightly against wall and forward
            Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
            float inputDot = Vector3.Dot(orientation.forward, wallForward);
            if (inputDot < 0)
                wallForward = -wallForward;

            Vector3 moveDir = wallForward * currentSpeed;

            controller.Move(moveDir * Time.deltaTime);

            // Allow jump off wall
            if (Input.GetKeyDown(jumpKey))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                velocity += wallNormal * jumpHeight * 2f; // Push off wall
                isWallRunning = false;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isWallRunning = false;
    }

    private void WallRunMove()
    {
        // Keep moving along wall (handled in coroutine)
    }

    private void StopWallRun()
    {
        isWallRunning = false;
    }

    private bool TryVault()
    {
        if (isVaulting) return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 forwardDir = orientation.forward;

        if (Physics.Raycast(origin, forwardDir, out RaycastHit hit, vaultDistance, vaultMask))
        {
            float heightDifference = hit.point.y - transform.position.y;
            if (heightDifference > 0.2f && heightDifference < vaultHeight)
            {
                StartCoroutine(VaultRoutine(hit.point));
                return true;
            }
        }
        return false;
    }

    private IEnumerator VaultRoutine(Vector3 targetPoint)
    {
        isVaulting = true;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 vaultPos = new Vector3(targetPoint.x, targetPoint.y + controller.height / 2, targetPoint.z);

        while (elapsed < vaultDuration)
        {
            controller.enabled = false;
            transform.position = Vector3.Lerp(startPos, vaultPos, elapsed / vaultDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = vaultPos;
        controller.enabled = true;
        isVaulting = false;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        Vector3 dashDir = orientation.forward * vertical + orientation.right * horizontal;
        if (dashDir.magnitude == 0)
            dashDir = orientation.forward;

        float time = 0f;
        while (time < dashDuration)
        {
            controller.Move(dashDir.normalized * dashSpeed * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void PlayJumpSound()
    {
        // Placeholder: play jump sound here
        if(audioSource != null)
        {
            // audioSource.PlayOneShot(jumpClip);
        }
    }

    // You can add other sounds for landing, footsteps, etc. similarly
}
