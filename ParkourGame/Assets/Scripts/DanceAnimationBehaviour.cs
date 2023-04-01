using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceAnimationBehaviour : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Randomly picks a dance animation from the blend tree
        animator.SetFloat("DanceAnimationRandom", Random.value);
    }
    
}
