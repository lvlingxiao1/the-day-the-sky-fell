using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnExitSetBool : StateMachineBehaviour
{

    public string boolName;
    public bool status;

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(boolName, status);
    }

}

