using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    public class AnimStartRootMovement : StateMachineBehaviour
    {
        ClimbEvents ce;

        public float timer = 0.2f;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (ce == null)
            {
                ce = animator.transform.GetComponent<ClimbEvents>();
            }

            if (ce == null)
            {
                return;
            }

            ce.EnableRootMovement(timer);
        }
    }
}