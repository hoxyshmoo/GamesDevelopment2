using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

[Header("Movement")]
private float moveSpeed;
public float walkSpeed;
public float sprintSpeed;

public float groundDrag;

[Header("Jumping")]
public float jumpForce;
public float jumpCooldown;
public float airMultiplier;
bool readyToJump;

[Header("Crouching")]
public float crouchSpeed;
public float crouchYScale;
private float startYScale;

[Header("Keybinds")]
public KeyCode jumpKey=KeyCode.Space;
public KeyCode sprintKey=KeyCode.LeftShift;
public KeyCode crouchKey=KeyCode.LeftControl;

[Header("Ground Check")]
public float playerHeight;
public LayerMask whatIsGround;
bool grounded;

[Header("Slope Handling")]
public float maxSlopeAngle;
private RaycastHit slopeHit;
private bool exitingSlope; 

public Transform orientation;
float horizontalInput, verticalInput;

Vector3 moveDirection;
Rigidbody rb;

public MovementState state;

public enum MovementState{
    walking,
    sprinting,
    crouching,
    air
}

    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        rb.freezeRotation=true;
        readyToJump=true; 

        startYScale=transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        //RayCast for Ground Check
        grounded=Physics.Raycast(transform.position,Vector3.down,playerHeight*0.5f+0.2f,whatIsGround);

        MyInput();
        SpeedControl(); // controls speed of player
        StateHandler();

        //handle the movement drag;
        if(grounded){
            rb.drag=groundDrag;
        }
        else{
            rb.drag=0;
        }
    }

    void FixedUpdate(){
        MovePlayer();
    }

    private void MyInput(){
        horizontalInput=Input.GetAxisRaw("Horizontal");
        verticalInput=Input.GetAxisRaw("Vertical");

        //Jump Key is Pressed 
        if(Input.GetKey(jumpKey)&& readyToJump && grounded){
            readyToJump=false;
            Jump();
            Invoke(nameof(ResetJump),jumpCooldown);
        }

        //Crouch Key is Pressed
        if(Input.GetKeyDown(crouchKey)){
            transform.localScale=new Vector3(transform.localScale.x,crouchYScale,transform.localScale.z);
            rb.AddForce(Vector3.down * 5f,ForceMode.Impulse);
        }

        if(Input.GetKeyUp(crouchKey)){
            transform.localScale=new Vector3(transform.localScale.x,startYScale,transform.localScale.z);
        }
    }

private void StateHandler(){

    //crouch 
    if(Input.GetKey(crouchKey)){
        state=MovementState.crouching;
        moveSpeed=crouchSpeed;
    }

    //Mode Sprint
    if(grounded && Input.GetKey(sprintKey)){
        state = MovementState.sprinting;
        moveSpeed=sprintSpeed;
    }
    else if(grounded){
        state=MovementState.walking;
        moveSpeed=walkSpeed;
    }
    else{
        state = MovementState.air;
    }
}

    private void MovePlayer(){
        
        //Calculate movement direction
        moveDirection=orientation.forward * verticalInput + orientation.right * horizontalInput;
        // rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        //slope
        if(OnSlope() && !exitingSlope){
            rb.AddForce(GetSlopeMoveDirection()*moveSpeed*20f,ForceMode.Force);

            if(rb.velocity.y>0){
                rb.AddForce(Vector3.down*80f,ForceMode.Force);
            }
        }

        //on ground
        if(grounded){
            rb.AddForce(moveDirection.normalized *  moveSpeed * 10f, ForceMode.Force);
        }
        //not on ground
        else if(!grounded){
            rb.AddForce(moveDirection.normalized *  moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        //turn gravity off while on slope
        rb.useGravity=!OnSlope();

    }

    private void SpeedControl(){

        //limit speed on slope
        if(OnSlope()&& !exitingSlope){
            if(rb.velocity.magnitude>moveSpeed){
                rb.velocity =rb.velocity.normalized * moveSpeed;
            }
        }
        //limit speed on ground or air
        else{
  Vector3 flatVel = new Vector3(rb.velocity.x,0f,rb.velocity.z);
        //limit velocity if needed
        if(flatVel.magnitude>moveSpeed){
            Vector3 limitedVel=flatVel.normalized * moveSpeed; 
            rb.velocity = new Vector3(limitedVel.x,rb.velocity.y,limitedVel.z);
            }
        }
      
    }

    private void Jump(){

        exitingSlope=true;

        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x,0f,rb.velocity.z);

        rb.AddForce(transform.up * jumpForce,ForceMode.Impulse);

    }

    private void ResetJump(){
        readyToJump=true;
        exitingSlope=false;
    }

    private bool OnSlope(){
        if(Physics.Raycast(transform.position,Vector3.down,out slopeHit,playerHeight*0.5f+0.3f)){
            float angle=Vector3.Angle(Vector3.up,slopeHit.normal);
            return angle < maxSlopeAngle && angle !=0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection(){
        return Vector3.ProjectOnPlane(moveDirection,slopeHit.normal).normalized;
    }
}
