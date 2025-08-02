using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{

    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float rotationSpeed = 120f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.1f;
    public Transform cameraTransform;

    public int maxJumps = 2;
    public float jumpCooldown = 0.01f;

    private Rigidbody rb;
    bool isGrounded = true;
    private int jumpsCount;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Ridigbody not found");
        }
        rb.freezeRotation = true;

    }

    // Update is called once per frame
    void Update()
    {

        HandleMovement();
        HandleJump();
        HandleRotation();

    }

    void HandleRotation()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // Get the camera's forward and right directions on the XZ plane
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate movement direction relative to camera
            Vector3 desiredDirection = cameraForward * moveZ + cameraRight * moveX;

            // Smoothly rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }


    private void HandleJump()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundMask);



        if (Input.GetButtonDown("Jump") && jumpsCount < maxJumps)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.LogError("Jumping");
            isGrounded = false;
            jumpsCount++;
        }
    }

    private void HandleMovement()

    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Get camera's forward and right directions (flattened to XZ plane)
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Convert input to world space based on camera orientation
            Vector3 moveDirection = cameraForward * moveZ + cameraRight * moveX;
            moveDirection.Normalize();

            // Preserve current vertical velocity (for gravity/jumping)
            Vector3 currentVelocity = rb.velocity;
            Vector3 moveVelocity = moveDirection * moveSpeed;
            rb.velocity = new Vector3(moveVelocity.x, currentVelocity.y, moveVelocity.z);
        }
        else
        {
            // Stop horizontal movement when no input
            Vector3 currentVelocity = rb.velocity;
            rb.velocity = new Vector3(0f, currentVelocity.y, 0f);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
            jumpsCount = 0;
            maxJumps = 2;
            Debug.LogError("Grounded = True");
        }
    }
}
