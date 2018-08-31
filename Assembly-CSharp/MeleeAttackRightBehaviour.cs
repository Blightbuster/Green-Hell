using System;
using UnityEngine;

public class MeleeAttackRightBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo animator_state_info, int layer_index)
	{
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo state_info, int layer_index)
	{
		if (state_info.normalizedTime > 0.9f)
		{
			WeaponMeleeController component = animator.transform.parent.GetComponent<WeaponMeleeController>();
			if (component)
			{
				component.PlayerMeleeRightAttackEnd();
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo state_info, int layer_index)
	{
	}
}
