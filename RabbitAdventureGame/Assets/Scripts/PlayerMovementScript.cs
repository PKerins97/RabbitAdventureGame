using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 3f;
    public float doubleJumpForce = 5f;
    public float chargeJumpForce = 15f;
    public float chargeTime = 1f;
    public float rotationSpeed = 120f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.1f;
    public Transform cameraTransform;

    public int maxJumps = 2;

    private Rigidbody rb;
    private int jumpCount;
    private float jumpCharge;
    private bool isChargingJump = false;
    private float chargeStartTime;
    private bool isGrounded = true;
    private bool isFalling;

    public Animator animator;

    private Vector3 lastCameraForward;

    // Start
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        lastCameraForward = cameraTransform.forward;
        lastCameraForward.y = 0f;
        lastCameraForward.Normalize();
    }

    // Update
    void Update()
    {
        HandleRotation();
        HandleJump();
        HandleAnimator();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // Movement Logic
    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Camera-relative movement
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = camForward * moveZ + camRight * moveX;
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) && moveZ > 0.1f ? sprintSpeed : moveSpeed;

            // Move player
            Vector3 move = moveDirection.normalized * currentSpeed;
            rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    // Camera-based Rotation
    void HandleRotation()
    {
        Vector3 currentCameraForward = cameraTransform.forward;
        currentCameraForward.y = 0;
        currentCameraForward.Normalize();

        if (Vector3.Angle(lastCameraForward, currentCameraForward) > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentCameraForward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            lastCameraForward = currentCameraForward;
        }
    }

    void HandleJump()
    {
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool jumpHeld = Input.GetButton("Jump");
        bool jumpReleased = Input.GetButtonUp("Jump");
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundMask);

        if (isGrounded)
        {
            jumpCount = 0;
            animator.SetBool("IsFalling", false);
        }

        // START charging jump
        if (shiftHeld && jumpPressed && isGrounded)
        {
            isChargingJump = true;
            chargeStartTime = Time.time;
            animator.SetBool("IsChargingJump", true); // Optional animation
            Debug.LogError("Charging");
        }

        // UPDATE charging jump
        if (isChargingJump && jumpHeld)
        {
            jumpCharge = Mathf.Clamp01((Time.time - chargeStartTime) / chargeTime);
        }

        // RELEASE charged jump
        if (isChargingJump && jumpReleased)
        {
            isChargingJump = false;
            float chargePower = Mathf.Lerp(jumpForce, chargeJumpForce, jumpCharge);
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset Y
            rb.AddForce(Vector3.up * chargePower, ForceMode.Impulse);
            animator.SetTrigger("ChargeJump"); // Optional animation
            animator.SetBool("IsChargingJump", false);
            jumpCount = 2;
            return; // Skip normal/double jump logic
        }

        // NORMAL and DOUBLE jump
        if (jumpPressed && jumpCount < maxJumps && !isChargingJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (jumpCount == 0 && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                animator.SetTrigger("Jump");
            }
            else if (jumpCount == 1)
            {
                rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
                animator.SetTrigger("DoubleJump");
            }

            jumpCount++;
        }

        // Falling logic
        animator.SetBool("IsFalling", !isGrounded && rb.velocity.y < -1);
    }


    // Animator Parameter Logic
    void HandleAnimator()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Camera-relative direction
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * moveZ + camRight * moveX;

        // Convert world move to local
        Vector3 localMove = transform.InverseTransformDirection(moveDirection);
        animator.SetFloat("Horizontal", localMove.x);
        animator.SetFloat("Vertical", localMove.z);

        bool isSprinting = moveZ > 0.1f && Input.GetKey(KeyCode.LeftShift);
        animator.SetBool("IsSprinting", isSprinting);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
            animator.ResetTrigger("Jump");
            animator.ResetTrigger("DoubleJump");
        }
    }
}
