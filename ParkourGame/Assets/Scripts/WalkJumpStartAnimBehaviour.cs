using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkJumpStartAnimBehaviour : StateMachineBehaviour
{
    private PlayerMovement playerMovement;
    private void Awake()
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Stop the player movement while landing animation is played
        playerMovement.StopPlayerMovement();
        
    }
    
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Allow the player movement when  animation is over
        playerMovement.ResumePlayerMovement();
        playerMovement.Jump();
    }

}
