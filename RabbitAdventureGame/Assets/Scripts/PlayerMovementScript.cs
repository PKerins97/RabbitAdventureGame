using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{

    public float moveSpeed = 5f; 
    public float sprintSpeed = 8f;
    public float jumpForce = 10f;
    public float rotationSpeed = 120f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.1f;
    public Transform cameraTransform;

    public int maxJumps = 2;
    public float jumpCooldown = 0.01f;

    private Rigidbody rb;
    
    private int jumpsCount;

    public Animator animator;
    public float runSpeed = 5f;

    private Vector3 lastCameraForward;

    bool isGrounded = true;
    bool isSprinting;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.freezeRotation = true;
        lastCameraForward = cameraTransform.forward;
        lastCameraForward.y = 0f;
        lastCameraForward.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleJump();

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Feed movement into Animator
        if (animator != null)
        {
            animator.SetFloat("Horizontal", moveX);
            animator.SetFloat("Vertical", moveZ);
        }

        bool isSprinting = moveZ > 0.1f && Input.GetKey(KeyCode.LeftShift);
        animator.SetBool("IsSprinting", isSprinting);
    }
    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Get camera-relative directions
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = camForward * moveZ + camRight * moveX;

            // Apply movement to rigidbody
            Vector3 move = moveDirection.normalized * moveSpeed;
            rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
        }
        else
        {
            // No movement input: stop horizontal motion
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    void HandleRotation()
    {
        // Only rotate the player when the camera's forward direction has changed
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
        if (Input.GetButtonDown("Jump") && jumpsCount < maxJumps)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // reset Y velocity
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpsCount++;
            isGrounded = false;
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

    //private void HandleMovement()

    //{
    //    float moveX = Input.GetAxis("Horizontal");
    //    float moveZ = Input.GetAxis("Vertical");
    //    Vector3 inputDirection = new Vector3(moveX, 0f, moveZ).normalized;
    //    Vector3 move = new Vector3(moveX, 0f, moveZ).normalized;
    //    Vector3 moveVelocity = move * moveSpeed;

    //    // Apply movement to rigidbody
    //    rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

    //    if (animator != null)
    //    {
    //        animator.SetFloat("Horizontal", moveX);
    //        animator.SetFloat("Vertical", moveZ);
    //    }
    //    if (inputDirection.magnitude >= 0.1f)
    //    {
    //        // Get camera's forward and right directions (flattened to XZ plane)
    //        Vector3 cameraForward = cameraTransform.forward;
    //        Vector3 cameraRight = cameraTransform.right;
    //        cameraForward.y = 0f;
    //        cameraRight.y = 0f;
    //        cameraForward.Normalize();
    //        cameraRight.Normalize();

    //        // Convert input to world space based on camera orientation
    //        Vector3 moveDirection = cameraForward * moveZ + cameraRight * moveX;
    //        moveDirection.Normalize();

    //        // Preserve current vertical velocity (for gravity/jumping)
    //        Vector3 currentVelocity = rb.velocity;
    //        rb.velocity = new Vector3(moveVelocity.x, currentVelocity.y, moveVelocity.z);


    //    }
    //    else
    //    {
    //        // Stop horizontal movement when no input
    //        Vector3 currentVelocity = rb.velocity;
    //        rb.velocity = new Vector3(0f, currentVelocity.y, 0f);
    //    }
    //}
}
