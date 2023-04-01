using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed = 4f;  // Was 7 before
    public float sprintSpeed = 10f;
    public float slideSpeed = 15f;  // Was 30 before
    public float wallrunSpeed = 8.5f;
    public float climbSpeed = 3f;
    public float playerHeight = 2f;
    public float speedIncreaseMultiplier = 1.5f;
    public float slopeIncreaseMultiplier = 2.5f;
    public float groundDrag = 4f;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private Vector3 moveDirection;

    [Header("Jumping")]
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed = 1.5f; // Was 3.5 before
    public float heightWhenCrouching = 0.5f;
    private float startYScale;
    private float heightWhenNotCrouching;
    private CapsuleCollider capsuleCollider;
    private float yOffSetWhenCrouching = -0.5f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftAlt;
    public KeyCode slideKey = KeyCode.Q;

    [Header("Ground Check")]
    public LayerMask whatIsGround;
    public bool grounded { get; private set; }

    [Header("Slope Handling")]
    public float maxSlopeAngle = 40f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Camera Settings")]
    private Transform camera;
    private GameObject freeLookCam;
    private GameObject lockedLookCam;
    
    [Header("References")]
    private PlayerAnimation animator;
    private WallClimbing climbScript;
    public bool isMoving { get; private set; }
    private Rigidbody rb;
    private Sliding slidingMovement;

    float horizontalInput;
    float verticalInput;


    public MovementState state { get; private set; }
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        air,
        climbing,
        freeze,
        unlimited
    }

    public bool sliding;
    public bool crouching;
    public bool wallrunning;
    public bool climbing;
    public bool freeze;
    public bool unlimited;
    public bool restricted; 
    private bool keepMomentum;
    // public TextMeshProUGUI text_speed;
    // public TextMeshProUGUI text_mode;

    private void Awake()
    {
        climbScript = GetComponent<WallClimbing>();
        rb = GetComponent<Rigidbody>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        freeLookCam = GameObject.FindWithTag("CameraFreeLook");
        lockedLookCam = GameObject.FindWithTag("CameraLockedLook");
        animator = GetComponentInChildren<PlayerAnimation>();
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        slidingMovement = GetComponentInChildren<Sliding>();

    }

    private void Start()
    {
        rb.freezeRotation = true;
        isMoving = false;
        readyToJump = true;
        freeLookCam.SetActive(true);
        lockedLookCam.SetActive(false);
        heightWhenNotCrouching = playerHeight;
    }
    
    // ToDo: Make the capsule smaller when the player is crouching

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
       // TextStuff();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        CalculateMoveDirection();
        MovePlayer();
    }

    private void CalculateMoveDirection()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        isMoving = direction.magnitude >= 0.1f;
        if (isMoving)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f); 
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }

    private void MyInput()
    {
        // when to jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded && isMoving)
        {
            if (state == MovementState.sprinting)
            {
                readyToJump = false;
                animator.TriggerJump();

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }
            
            // When we jump while walking we delay the start of the jump motion to sync with the animation
            // The movement is also stopped for a short period of time to make it realistic
            if (state == MovementState.walking)
            {
                float delayBeforeMovementStop = 0.1f;
                float delayBeforeJump = 0.4f;
                readyToJump = false;
                animator.TriggerJump();
                
                // Stop the player movement right before jumping
                Invoke(nameof(StopPlayerMovement), delayBeforeMovementStop);

                // Resume the player movement and do the jumping
                Invoke(nameof(ResumePlayerMovement), delayBeforeMovementStop);
                Invoke(nameof(Jump), delayBeforeJump);

                Invoke(nameof(ResetJump), jumpCooldown);
            }

        }

        // start crouch if the player is not moving and presses the crouching key
        if (Input.GetKeyDown(crouchKey) && !isMoving && grounded)
        {
            // Move the capsule collider to the new temporary position
            capsuleCollider.center = new Vector3(0f, yOffSetWhenCrouching, 0f);
            
            // Make it smaller
            capsuleCollider.height = heightWhenCrouching;
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            crouching = true;
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey) && crouching)
        {
            // Move the capsule back to it's original position
            capsuleCollider.center = new Vector3(0f, 0, 0f);
            
            // Change the height back to original
            capsuleCollider.height = heightWhenNotCrouching;
            crouching = false;
        }
        
        // Start sliding if the player is sprinting and presses the button
        if (Input.GetKeyDown(slideKey) && Input.GetKey(sprintKey) && isMoving && grounded && !sliding)
            slidingMovement.StartSlide();
        
        // Stop sliding
        if (Input.GetKeyUp(slideKey) && sliding)
            slidingMovement.StopSlide();
    }

    private void StopPlayerMovement()
    {
        rb.isKinematic = true;
    }
    
    private void ResumePlayerMovement()
    {
        rb.isKinematic = false;
    }
    
    private void StateHandler()
    {
        if(freeze){
            state=MovementState.freeze;
            rb.velocity=Vector3.zero;
            desiredMoveSpeed=5f;
        }
        if(unlimited){
            state=MovementState.unlimited;
            //moveSpeed=999f;
            desiredMoveSpeed=999f;
            return;
        }
        if(climbing){
            state=MovementState.climbing;
            desiredMoveSpeed=climbSpeed;
        }
        // Mode - Wallrunning
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            // increase speed by one every second
            if (OnSlope() && rb.velocity.y < 0.1f){
                desiredMoveSpeed = slideSpeed;
                keepMomentum=true;
            }
            else{
                desiredMoveSpeed = sprintSpeed;
            }
              
        }

        // Mode - Crouching
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey) && isMoving && !sliding)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
            
            // Disable the free look camera and enable the sprint locked camera
            freeLookCam.SetActive(false);
            lockedLookCam.SetActive(true);
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
            
            // Enable the free look camera and disable the locked camera which is for sprinting
            freeLookCam.SetActive(true);
            lockedLookCam.SetActive(false);
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        bool desiredMoveSpeedHasChanged=desiredMoveSpeed != lastDesiredMoveSpeed;

        if(desiredMoveSpeedHasChanged){
            if(keepMomentum){
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else{
                moveSpeed=desiredMoveSpeed;
            }
        }

        // check if desired move speed has changed drastically
        // if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        // {
        //     StopAllCoroutines();
        //     StartCoroutine(SmoothlyLerpMoveSpeed());

        //     print("Lerp Started!");
        // }
        // else
        // {
        //     moveSpeed = desiredMoveSpeed;
        // }

        //disable momentum when slowing done

        lastDesiredMoveSpeed = desiredMoveSpeed;

        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f){
            keepMomentum=false;
        }

    
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if(restricted || climbScript.exitingWall){
            return; 
        }

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * (moveSpeed * 20f), ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);

        // turn gravity off while on slope
        if(!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    // private void TextStuff()
    // {
    //     Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

    //     if (OnSlope())
    //         text_speed.SetText("Speed: " + Round(rb.velocity.magnitude, 1));

    //     else
    //         text_speed.SetText("Speed: " + Round(flatVel.magnitude, 1));

    //     text_mode.SetText(state.ToString());
    // }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }
}
