using UnityEngine;

// Peter's ToDo: Fix when the player is crouching the object gets squeezed  
// Peter's ToDo: Add animations for wall sliding, sliding, do turns, wall jump, double jump etc
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private WallRunning wallRunning;

    private void Awake()
    {
        // Get the Animator and PlayerMovement components
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        wallRunning = GameObject.FindGameObjectWithTag("Player").GetComponent<WallRunning>();

    }
    
    private void Update()
    {
        animator.SetBool("IsMoving", playerMovement.isMoving);
        animator.SetBool("IsRunning", playerMovement.state == PlayerMovement.MovementState.sprinting);
        animator.SetBool("IsOnGround", playerMovement.grounded);
        animator.SetBool("Crouching", playerMovement.state == PlayerMovement.MovementState.crouching);
        animator.SetBool("Sliding", playerMovement.state == PlayerMovement.MovementState.sliding);
        animator.SetBool("IsWallRunning", playerMovement.state == PlayerMovement.MovementState.wallrunning);
        animator.SetBool("MirrorAnimation", wallRunning.wallRight);
    }

    public void TriggerJump()
    {
        animator.SetTrigger("Jump");
    }
}
