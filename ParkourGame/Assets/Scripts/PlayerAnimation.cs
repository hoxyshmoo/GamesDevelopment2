using UnityEngine;

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
        animator.SetBool("IsWallRunning", playerMovement.state == PlayerMovement.MovementState.wallrunning);
        animator.SetBool("IsClimbing", playerMovement.climbing);
        animator.SetBool("IsSliding", playerMovement.state == PlayerMovement.MovementState.sliding);
        animator.SetBool("IsCrouching", playerMovement.state == PlayerMovement.MovementState.crouching);
        animator.SetBool("MirrorAnimation", wallRunning.wallRight);
    }

    public void JumpTrigger()
    {
        animator.SetTrigger("JumpTrigger");
    }
    
    public void ClimbTrigger()
    {
        animator.SetTrigger("ClimbTrigger");
    }
}
