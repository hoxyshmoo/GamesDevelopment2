using UnityEngine;

// Peter's ToDo: Fix when the player is crouching the object gets squeezed  
// Peter's ToDo: Add animations for wall sliding, sliding, do turns, wall jump, double jump etc
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    
    private void Awake()
    {
        // Get the Animator and PlayerMovement components
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }
    
    private void Update()
    {
        // Update movement parameters of the player
        animator.SetBool("IsMoving", playerMovement.isMoving);
    }
    
}
