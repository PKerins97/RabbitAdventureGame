using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{

    public float moveSpeed = 5f; 
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float doubleJumpForce = 7f;
    public float rotationSpeed = 120f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.1f;
    public Transform cameraTransform;

    public int maxJumps = 2;




    private Rigidbody rb;
    
    private int jumpCount;

    public Animator animator;
    public float runSpeed = 5f;

    private Vector3 lastCameraForward;

    bool isGrounded = true;
    bool isSprinting;
    bool isFailing;
    

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
        bool jumpPressed = Input.GetButtonDown("Jump");

        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundMask);

        if (isGrounded)
        {
            jumpCount = 0;
            animator.SetBool("IsFalling", false);
        }

        if (jumpPressed && jumpCount < maxJumps)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // reset vertical velocity

            if (jumpCount == 0)
            {
                // First jump
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                animator.SetTrigger("Jump");
            }
            else if (jumpCount == 1)
            {
                // Second jump (double jump)
                rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
                animator.SetTrigger("DoubleJump"); // <-- Different animation trigger
            }

            jumpCount++;
        }

        // Falling animation toggle
        animator.SetBool("IsFalling", !isGrounded && rb.velocity.y < 0);
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
            jumpCount = 0;
            maxJumps = 2;
            animator.ResetTrigger("Jump");
            animator.ResetTrigger("DoubleJump");
            Debug.LogError("Grounded = True");
        }
    }

}
