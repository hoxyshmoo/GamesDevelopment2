using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    private Transform orientation;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float slideForce;
    private float slideTime = 1.433f; // Based on the slide animation length
    private float remainingTime;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        orientation = transform;
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }

    public void StartSlide()
    {
        pm.sliding = true;

        // playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        remainingTime = slideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // sliding normal
        if(!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            remainingTime -= Time.deltaTime;
        }

        // sliding down a slope
        else 
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);

        if (remainingTime <= 0)
            StopSlide();
    }

    public void StopSlide()
    {
        pm.sliding = false;
        pm.changeCapsuleColliderToPlayerSize();
    }
}
