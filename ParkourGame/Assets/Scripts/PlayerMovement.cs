using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using TMPro;
using TMPro.Examples;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed = 4f; // Was 7 before
    public float sprintSpeed = 10f;
    public float slideSpeed = 15f; // Was 30 before
    public float wallrunSpeed = 8.5f;
    public float climbSpeed = 3f;
    public float playerHeight = 2f;
    public float speedIncreaseMultiplier = 1.5f;
    public float slopeIncreaseMultiplier = 2.5f;
    public float groundDrag = 4f;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private Vector3 moveDirection;

    [Header("Jumping")] public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    bool readyToJump;

    [Header("Crouching")] public float crouchSpeed = 1.5f; // Was 3.5 before
    public float heightWhenCrouching = 0.5f;
    private CapsuleCollider capsuleCollider;
    private float yOffSetWhenCrouching = -0.5f;

    [Header("Keybinds")] public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.Q;
    public KeyCode slideKey = KeyCode.Q;

    [Header("Ground Check")] public LayerMask whatIsGround;
    public bool grounded { get; private set; }

    [Header("Slope Handling")] public float maxSlopeAngle = 40f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Camera Settings")]
    public float rotationPower = 5f;
    public float rotationLerp = 0.5f;
    private Transform camera;
    private GameObject freeLookCam;
    private GameObject lockedLookCam;

    [Header("References")] private PlayerAnimation animator;
    private WallClimbing climbScript;
    public bool isMoving { get; private set; }
    private Rigidbody rb;
    private Sliding slidingMovement;
    private GameObject cameraFollowTarget;

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
        cameraFollowTarget = GameObject.FindWithTag("FollowTarget");

    }
    

    private void Start()
    {
        rb.freezeRotation = true;
        isMoving = false;
        readyToJump = true;
        freeLookCam.SetActive(true);
        lockedLookCam.SetActive(false);
    }
    

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
        float mouseInputX = Input.GetAxis ("Mouse X");
        float mouseInputY = Input.GetAxis ("Mouse Y");
        Vector3 direction = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        
        
        #region Camera Horizontal Rotation
        cameraFollowTarget.transform.rotation *= Quaternion.AngleAxis(mouseInputX * rotationPower, Vector3.up);
        #endregion
        
        #region Camera Vertical Rotation
        cameraFollowTarget.transform.rotation *= Quaternion.AngleAxis(mouseInputY * rotationPower, Vector3.right);

        var angles = cameraFollowTarget.transform.localEulerAngles;
        angles.z = 0;

        var angleTemp = cameraFollowTarget.transform.localEulerAngles.x;

        //Clamp the Up/Down rotation
        if (angleTemp > 180 && angleTemp < 340)
        {
            angles.x = 340;
        }
        else if(angleTemp < 180 && angleTemp > 40)
        {
            angles.x = 40;
        }
        cameraFollowTarget.transform.localEulerAngles = angles;
        #endregion
        isMoving = direction.magnitude >= 0.1f;
        
        //Set the player rotation based on the camera
        transform.rotation = Quaternion.Euler(0, cameraFollowTarget.transform.rotation.eulerAngles.y, 0);
        if (isMoving)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            //reset the y rotation of the look transform
            cameraFollowTarget.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
        }
        else
        {
            //reset the y rotation of the look transform
            cameraFollowTarget.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
            moveDirection = Vector3.zero;
        }
    }

    private void MyInput()
    {
        // when to jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded && isMoving)
        {
            readyToJump = false;
            animator.JumpTrigger();
            
            // When we jump while sprinting the jump is done instantly and it's called from here
            if (state == MovementState.sprinting) Jump();
            
            // When we jump while walking we delay the start of the jump motion. For that the jump is called
            // from the WalkingJumpAnimationBehaviour
            Invoke(nameof(ResetJump), jumpCooldown);

        }

        // start crouch if the player is not moving and presses the crouching key
        if (Input.GetKeyDown(crouchKey) && grounded && !Input.GetKey(sprintKey))
        {
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            crouching = true;
            changeCapsuleColliderToCrouchSize();
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey) && crouching)
        {
            crouching = false;
            changeCapsuleColliderToPlayerSize();
        }

        // Start sliding if the player is sprinting and presses the button
        if (Input.GetKeyDown(slideKey) && Input.GetKey(sprintKey) && isMoving && grounded && !sliding)
        {
            slidingMovement.StartSlide();
            changeCapsuleColliderToCrouchSize();

        }
    }

    private void changeCapsuleColliderToCrouchSize()
    {
        // Move the capsule collider to the new temporary position
        capsuleCollider.center = new Vector3(0f, yOffSetWhenCrouching, 0f);

        // Make it smaller
        capsuleCollider.height = heightWhenCrouching;
    }
    
    public void changeCapsuleColliderToJumpSize()
    {
        // Move the capsule collider to the new temporary position
        capsuleCollider.center = new Vector3(0f, -0.25f, 0f);
        
        // Make it smaller
        capsuleCollider.height = 1.5f;
    }

    
    public void changeCapsuleColliderToPlayerSize()
    {
        // Move the capsule back to it's original position
        capsuleCollider.center = new Vector3(0f, 0, 0f);

        // Change the height back to original
        capsuleCollider.height = playerHeight;
    }

    public void StopPlayerMovement()
    {
        rb.isKinematic = true;
    }
    
    public void ResumePlayerMovement()
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

    public void Jump()
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
