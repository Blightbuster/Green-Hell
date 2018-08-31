using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class AnimationModule : AIModule
	{
		public override void Initialize()
		{
			base.Initialize();
			this.m_WantedBlend = this.DEFAULT_BLEND;
			this.m_WantedAttackBlend = this.DEFAULT_BLEND;
			TextAssetParser textAssetParser = AIManager.Get().m_AnimatorDataParsers[this.GetStatesDataScript()];
			for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
			{
				Key key = textAssetParser.GetKey(i);
				if (key.GetName() == "State")
				{
					StateData stateData = new StateData();
					stateData.m_ClipName = key.GetVariable(1).SValue;
					stateData.m_Duration = key.GetVariable(2).FValue;
					stateData.m_Loop = (key.GetVariable(3).IValue != 0);
					this.m_StatesData.Add(key.GetVariable(0).SValue, stateData);
					this.m_StateNames.Add(key.GetVariable(0).SValue);
					if (!this.m_HasSpecificIdle)
					{
						this.m_HasSpecificIdle = key.GetVariable(0).SValue.Contains("SpecificIdle");
					}
				}
			}
		}

		protected virtual string GetStatesDataScript()
		{
			return this.m_AI.m_ID.ToString() + "AnimatorData";
		}

		private void InitParams()
		{
			if (!this.m_AI.m_Animator.isInitialized)
			{
				return;
			}
			foreach (AnimatorControllerParameter animatorControllerParameter in this.m_AI.m_Animator.parameters)
			{
				if (animatorControllerParameter.nameHash == this.m_BlendHash)
				{
					this.m_HasBlend = true;
					this.m_AI.m_Animator.SetFloat(this.m_BlendHash, this.m_Blend);
					break;
				}
			}
			foreach (AnimatorControllerParameter animatorControllerParameter2 in this.m_AI.m_Animator.parameters)
			{
				if (animatorControllerParameter2.nameHash == this.m_AttackBlendHash)
				{
					this.m_HasAttackBlend = true;
					this.m_AI.m_Animator.SetFloat(this.m_AttackBlendHash, this.m_AttackBlend);
					break;
				}
			}
			this.m_ParamsInited = true;
		}

		public bool ContainsState(string name)
		{
			return this.m_StateNames.Contains(name);
		}

		public void SetWantedAnim(string anim)
		{
			this.m_WantedAnim = anim;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (!this.m_ParamsInited)
			{
				this.InitParams();
			}
			this.UpdateBlend();
			this.UpdateAttackBlend();
			if (this.m_AI.m_Animator.IsInTransition(0))
			{
				return;
			}
			if (this.m_WantedAnim.Length <= 0)
			{
				return;
			}
			if (this.m_CurrentAnim != this.m_WantedAnim)
			{
				this.m_CurrentAnim = this.m_WantedAnim;
				float fixedTimeOffset = 0f;
				if (this.m_StartFromRandomFrame)
				{
					fixedTimeOffset = UnityEngine.Random.Range(0f, this.m_StatesData[this.m_CurrentAnim].m_Duration);
					this.m_StartFromRandomFrame = false;
				}
				if (this.m_PrevAnim == this.m_CurrentAnim && this.m_StatesData[this.m_CurrentAnim].m_Loop)
				{
					this.m_TransitionDuration = AnimationModule.DEFAULT_TRANSITION_DURATION;
					return;
				}
				this.m_AI.m_Animator.CrossFadeInFixedTime(this.m_CurrentAnim, this.m_TransitionDuration, -1, fixedTimeOffset);
				this.m_TransitionDuration = AnimationModule.DEFAULT_TRANSITION_DURATION;
				this.m_PrevAnim = this.m_CurrentAnim;
			}
			AnimatorStateInfo currentAnimatorStateInfo = this.m_AI.m_Animator.GetCurrentAnimatorStateInfo(0);
			if (this.m_ForcedSpeed >= 0f)
			{
				this.m_AI.m_Animator.speed = this.m_ForcedSpeed;
			}
			else if (currentAnimatorStateInfo.shortNameHash == this.m_SneakHash)
			{
				this.m_AI.m_Animator.speed = this.m_AI.m_Params.m_SneakSpeedMul;
			}
			else if (currentAnimatorStateInfo.shortNameHash == this.m_WalkHash)
			{
				this.m_AI.m_Animator.speed = this.m_AI.m_Params.m_WalkSpeedMul;
			}
			else if (currentAnimatorStateInfo.shortNameHash == this.m_TrotHash)
			{
				this.m_AI.m_Animator.speed = this.m_AI.m_Params.m_TrotSpeedMul;
			}
			else if (currentAnimatorStateInfo.shortNameHash == this.m_RunHash)
			{
				this.m_AI.m_Animator.speed = this.m_AI.m_Params.m_RunSpeedMul;
			}
			else
			{
				this.m_AI.m_Animator.speed = 1f;
			}
		}

		public void ForceReset()
		{
			this.m_CurrentAnim = string.Empty;
		}

		public bool IsAnimFinishing(string anim_name)
		{
			AnimatorStateInfo currentAnimatorStateInfo = this.m_AI.m_Animator.GetCurrentAnimatorStateInfo(0);
			return currentAnimatorStateInfo.IsName(anim_name) && currentAnimatorStateInfo.normalizedTime >= 0.9f;
		}

		public void SetWantedBlend(float blend)
		{
			this.m_WantedBlend = blend;
		}

		public void ResetWantedBlend()
		{
			this.m_WantedBlend = this.DEFAULT_BLEND;
			this.m_Blend = this.DEFAULT_BLEND;
			this.m_AI.m_Animator.SetFloat(this.m_BlendHash, this.m_Blend);
		}

		private void UpdateBlend()
		{
			if (!this.m_HasBlend)
			{
				return;
			}
			this.m_Blend += (this.m_WantedBlend - this.m_Blend) * Time.deltaTime * 5f;
			this.m_Blend = Mathf.Clamp01(this.m_Blend);
			this.m_AI.m_Animator.SetFloat(this.m_BlendHash, this.m_Blend);
		}

		public void SetWantedAttackBlend(float blend)
		{
			this.m_WantedAttackBlend = blend;
		}

		private void UpdateAttackBlend()
		{
			if (!this.m_HasAttackBlend)
			{
				return;
			}
			this.m_AttackBlend += (this.m_WantedAttackBlend - this.m_AttackBlend) * Time.deltaTime * 5f;
			this.m_AttackBlend = Mathf.Clamp01(this.m_AttackBlend);
			this.m_AI.m_Animator.SetFloat(this.m_AttackBlendHash, this.m_AttackBlend);
		}

		public void SetForcedSpeed(float speed)
		{
			this.m_ForcedSpeed = speed;
		}

		public void ResetForcedSpeed()
		{
			this.m_ForcedSpeed = -1f;
		}

		protected string m_WantedAnim = string.Empty;

		public string m_CurrentAnim = string.Empty;

		private int m_SneakHash = Animator.StringToHash("Sneak");

		protected int m_WalkHash = Animator.StringToHash("Walk");

		private int m_TrotHash = Animator.StringToHash("Trot");

		protected int m_RunHash = Animator.StringToHash("Run");

		private int m_BlendHash = Animator.StringToHash("Blend");

		private int m_AttackBlendHash = Animator.StringToHash("AttackBlend");

		private const float DEFAULT_BLEND_MUL = 5f;

		[HideInInspector]
		public float DEFAULT_BLEND = 0.5f;

		private float m_WantedBlend;

		private float m_Blend;

		[HideInInspector]
		public bool m_HasBlend;

		private float m_WantedAttackBlend;

		private float m_AttackBlend;

		[HideInInspector]
		public bool m_HasAttackBlend;

		[HideInInspector]
		public bool m_HasSpecificIdle;

		private bool m_ParamsInited;

		[HideInInspector]
		public bool m_StartFromRandomFrame;

		public static float DEFAULT_TRANSITION_DURATION = 0.25f;

		[HideInInspector]
		public float m_TransitionDuration = AnimationModule.DEFAULT_TRANSITION_DURATION;

		protected Dictionary<string, StateData> m_StatesData = new Dictionary<string, StateData>();

		private List<string> m_StateNames = new List<string>();

		protected string m_PrevAnim = string.Empty;

		protected float m_ForcedSpeed = -1f;
	}
}
