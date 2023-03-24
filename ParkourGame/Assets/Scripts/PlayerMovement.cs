using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

[Header("Movement")]
public float moveSpeed;

public float groundDrag;

public float jumpForce;
public float jumpCooldown;
public float airMultiplier;
bool readyToJump;

[Header("Keybinds")]
public KeyCode jumpKey=KeyCode.Space;

[Header("Ground Check")]
public float playerHeight;
public LayerMask whatIsGround;
bool grounded;

public Transform orientation;
float horizontalInput, verticalInput;

Vector3 moveDirection;
Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        rb.freezeRotation=true;
        readyToJump=true; 
    }

    // Update is called once per frame
    void Update()
    {
        //RayCast for Ground Check
        grounded=Physics.Raycast(transform.position,Vector3.down,playerHeight*0.5f+0.2f,whatIsGround);

        MyInput();
        SpeedControl(); // controls speed of player

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
    }

    private void MovePlayer(){
        
        moveDirection=orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        if(grounded){
            rb.AddForce(moveDirection.normalized *  moveSpeed * 10f, ForceMode.Force);
        }
        else if(!grounded){
            rb.AddForce(moveDirection.normalized *  moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

    }

    private void SpeedControl(){
        Vector3 flatVel = new Vector3(rb.velocity.x,0f,rb.velocity.z);

        //limit velocity if needed
        if(flatVel.magnitude>moveSpeed){
            Vector3 limitedVel=flatVel.normalized * moveSpeed; 
            rb.velocity = new Vector3(limitedVel.x,rb.velocity.y,limitedVel.z);
        }
    }

    private void Jump(){
        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x,0f,rb.velocity.z);

        rb.AddForce(transform.up * jumpForce,ForceMode.Impulse);

    }

    private void ResetJump(){
        readyToJump=true;
    }
}
