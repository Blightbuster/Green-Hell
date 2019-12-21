using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;

public class PlayerController : MonoBehaviour, IAnimationEventsReceiver, IInputsReceiver
{
	public PlayerControllerType m_ControllerType { get; protected set; } = PlayerControllerType.Unknown;

	protected virtual void Awake()
	{
		this.m_Player = base.gameObject.GetComponent<Player>();
		this.m_Animator = base.gameObject.GetComponent<Animator>();
		this.m_BaseLayerIndex = this.m_Animator.GetLayerIndex("Base Layer");
		this.m_SpineLayerIndex = this.m_Animator.GetLayerIndex("Spine Layer");
		this.m_OriginalController = this.m_Animator.runtimeAnimatorController;
		this.ParseClipsReplaceScript();
	}

	private void ParseClipsReplaceScript()
	{
		string text = base.GetType().ToString();
		text += "_ClipsReplace";
		TextAsset textAsset = Resources.Load("Scripts/Player/ClipsReplace/" + text) as TextAsset;
		if (!textAsset)
		{
			return;
		}
		TextAssetParser textAssetParser = new TextAssetParser(textAsset);
		if (PlayerController.NEW_ANIM_OVERRIDES)
		{
			AnimatorOverrideController animatorOverrideController = this.m_Animator.runtimeAnimatorController as AnimatorOverrideController;
			if (animatorOverrideController == null)
			{
				animatorOverrideController = new AnimatorOverrideController(this.m_Animator.runtimeAnimatorController);
				this.m_Animator.runtimeAnimatorController = animatorOverrideController;
			}
			if (this.m_ClipOverrides == null)
			{
				this.m_ClipOverrides = new Dictionary<string, List<KeyValuePair<AnimationClip, AnimationClip>>>();
			}
			for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
			{
				Key key = textAssetParser.GetKey(i);
				if (key.GetName() == "Item")
				{
					string svalue = key.GetVariable(0).SValue;
					this.m_ClipOverrides[svalue] = new List<KeyValuePair<AnimationClip, AnimationClip>>();
					for (int j = 0; j < key.GetKeysCount(); j++)
					{
						Key key2 = key.GetKey(j);
						if (key2.GetName() == "Clip")
						{
							AnimationClip key3 = animatorOverrideController[key2.GetVariable(0).SValue];
							AnimationClip value = animatorOverrideController[key2.GetVariable(1).SValue];
							this.m_ClipOverrides[svalue].Add(new KeyValuePair<AnimationClip, AnimationClip>(key3, value));
						}
					}
				}
			}
			return;
		}
		if (this.m_ClipReplaceMap == null)
		{
			this.m_ClipReplaceMap = new Dictionary<string, List<KeyValuePair<string, string>>>();
		}
		for (int k = 0; k < textAssetParser.GetKeysCount(); k++)
		{
			Key key4 = textAssetParser.GetKey(k);
			if (key4.GetName() == "Item")
			{
				string svalue2 = key4.GetVariable(0).SValue;
				this.m_ClipReplaceMap[svalue2] = new List<KeyValuePair<string, string>>();
				for (int l = 0; l < key4.GetKeysCount(); l++)
				{
					Key key5 = key4.GetKey(l);
					if (key5.GetName() == "Clip")
					{
						KeyValuePair<string, string> item = new KeyValuePair<string, string>(key5.GetVariable(0).SValue, key5.GetVariable(1).SValue);
						this.m_ClipReplaceMap[svalue2].Add(item);
					}
				}
			}
		}
	}

	protected virtual void Start()
	{
		InputsManager.Get().RegisterReceiver(this);
	}

	public virtual void OnInputAction(InputActionData action_data)
	{
	}

	public virtual void ControllerUpdate()
	{
	}

	public virtual void ControllerLateUpdate()
	{
	}

	protected virtual void OnEnable()
	{
		this.ReplaceClips();
	}

	protected virtual void OnDisable()
	{
		this.RestoreClips();
		this.ResetBodyRotationBonesParams();
	}

	public virtual bool IsActive()
	{
		return base.enabled;
	}

	public bool CanReceiveAction()
	{
		return this.IsActive();
	}

	public bool CanReceiveActionPaused()
	{
		return false;
	}

	public virtual void Stop()
	{
		this.m_Player.StopController(this.m_ControllerType);
	}

	public virtual void OnBlockRotation()
	{
	}

	public virtual void OnUnblockRotation()
	{
	}

	public virtual bool ForceReceiveAnimEvent()
	{
		return false;
	}

	public virtual void OnAnimEvent(AnimEventID id)
	{
	}

	public virtual Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return null;
	}

	public void ResetBodyRotationBonesParams()
	{
		Dictionary<Transform, float> bodyRotationBonesParams = this.GetBodyRotationBonesParams();
		if (bodyRotationBonesParams != null && bodyRotationBonesParams.Count > 0)
		{
			for (int i = 0; i < bodyRotationBonesParams.Count; i++)
			{
				Transform key = bodyRotationBonesParams.Keys.ElementAt(i);
				bodyRotationBonesParams[key] = 0f;
			}
		}
	}

	public virtual bool SetupActiveControllerOnStop()
	{
		return true;
	}

	public virtual void GetInputActions(ref List<int> actions)
	{
	}

	public virtual string ReplaceClipsGetItemName()
	{
		return string.Empty;
	}

	private void ReplaceClips()
	{
		string text = this.ReplaceClipsGetItemName();
		if (text.Length == 0)
		{
			return;
		}
		if (PlayerController.NEW_ANIM_OVERRIDES)
		{
			if (this.m_ClipOverrides == null)
			{
				return;
			}
			if (this.m_ClipOverrides.TryGetValue(text, out this.m_SetOverrides))
			{
				AnimatorOverrideController animatorOverrideController = this.m_Animator.runtimeAnimatorController as AnimatorOverrideController;
				DebugUtils.Assert(animatorOverrideController != null, true);
				animatorOverrideController.ApplyOverrides(this.m_SetOverrides);
				return;
			}
		}
		else
		{
			if (this.m_ClipReplaceMap == null)
			{
				return;
			}
			List<KeyValuePair<string, string>> list = null;
			if (!this.m_ClipReplaceMap.TryGetValue(text, out list))
			{
				return;
			}
			AnimatorStateInfo[] array = new AnimatorStateInfo[this.m_Animator.layerCount];
			for (int i = 0; i < this.m_Animator.layerCount; i++)
			{
				array[i] = this.m_Animator.GetCurrentAnimatorStateInfo(i);
			}
			object[] array2 = new object[this.m_Animator.parameterCount];
			for (int j = 0; j < this.m_Animator.parameterCount; j++)
			{
				array2[j] = this.GetAnimatorParameterData(this.m_Animator.parameters[j]);
			}
			AnimatorOverrideController animatorOverrideController2 = new AnimatorOverrideController();
			animatorOverrideController2.runtimeAnimatorController = this.m_Animator.runtimeAnimatorController;
			this.m_Animator.runtimeAnimatorController = animatorOverrideController2;
			for (int k = 0; k < this.m_ClipReplaceMap[text].Count; k++)
			{
				AnimationClip animationClip = animatorOverrideController2[this.m_ClipReplaceMap[text][k].Value];
				animatorOverrideController2[this.m_ClipReplaceMap[text][k].Key] = animatorOverrideController2[this.m_ClipReplaceMap[text][k].Value];
			}
			this.m_Animator.Update(0f);
			for (int l = 0; l < this.m_Animator.parameterCount; l++)
			{
				this.SetAnimatorParameterData(this.m_Animator.parameters[l], array2[l]);
			}
			for (int m = 0; m < this.m_Animator.layerCount; m++)
			{
				if (m != 2)
				{
					this.m_Animator.Play(array[m].fullPathHash, m, array[m].normalizedTime);
				}
			}
			this.m_ClipsReplaced = true;
		}
	}

	private void RestoreClips()
	{
		if (PlayerController.NEW_ANIM_OVERRIDES)
		{
			if (this.m_ClipOverrides == null)
			{
				return;
			}
			if (this.m_SetOverrides != null)
			{
				AnimatorOverrideController animatorOverrideController = this.m_Animator.runtimeAnimatorController as AnimatorOverrideController;
				DebugUtils.Assert(animatorOverrideController != null, true);
				for (int i = 0; i < this.m_SetOverrides.Count; i++)
				{
					animatorOverrideController[this.m_SetOverrides[i].Key] = null;
				}
				this.m_SetOverrides = null;
				return;
			}
		}
		else if (this.m_ClipsReplaced && this.m_Animator.runtimeAnimatorController != this.m_OriginalController)
		{
			AnimatorStateInfo[] array = new AnimatorStateInfo[this.m_Animator.layerCount];
			for (int j = 0; j < this.m_Animator.layerCount; j++)
			{
				array[j] = this.m_Animator.GetCurrentAnimatorStateInfo(j);
			}
			object[] array2 = new object[this.m_Animator.parameterCount];
			for (int k = 0; k < this.m_Animator.parameterCount; k++)
			{
				array2[k] = this.GetAnimatorParameterData(this.m_Animator.parameters[k]);
			}
			this.m_Animator.runtimeAnimatorController = this.m_OriginalController;
			this.m_Animator.Update(0f);
			for (int l = 0; l < this.m_Animator.parameterCount; l++)
			{
				this.SetAnimatorParameterData(this.m_Animator.parameters[l], array2[l]);
			}
			for (int m = 0; m < this.m_Animator.layerCount; m++)
			{
				this.m_Animator.Play(array[m].fullPathHash, m, array[m].normalizedTime);
			}
			this.m_Animator.Update(0f);
			this.m_ClipsReplaced = false;
		}
	}

	private object GetAnimatorParameterData(AnimatorControllerParameter param)
	{
		object result = null;
		switch (param.type)
		{
		case AnimatorControllerParameterType.Float:
			result = this.m_Animator.GetFloat(param.nameHash);
			break;
		case AnimatorControllerParameterType.Int:
			result = this.m_Animator.GetInteger(param.nameHash);
			break;
		case AnimatorControllerParameterType.Bool:
			result = this.m_Animator.GetBool(param.nameHash);
			break;
		}
		return result;
	}

	private void SetAnimatorParameterData(AnimatorControllerParameter param, object data)
	{
		switch (param.type)
		{
		case AnimatorControllerParameterType.Float:
			this.m_Animator.SetFloat(param.nameHash, (float)data);
			return;
		case (AnimatorControllerParameterType)2:
			break;
		case AnimatorControllerParameterType.Int:
			this.m_Animator.SetInteger(param.nameHash, (int)data);
			return;
		case AnimatorControllerParameterType.Bool:
			this.m_Animator.SetBool(param.nameHash, (bool)data);
			break;
		default:
			return;
		}
	}

	protected virtual void OnDestroy()
	{
		InputsManager.Get().UnregisterReceiver(this);
	}

	public virtual void OnTakeDamage(DamageInfo info)
	{
	}

	public virtual void OnItemChanged(Item item, Hand hand)
	{
		if (item != null)
		{
			item.gameObject.SetActive(true);
		}
	}

	public virtual bool PlayUnequipAnimation()
	{
		return false;
	}

	protected Player m_Player;

	[HideInInspector]
	public Animator m_Animator;

	protected int m_BaseLayerIndex = -1;

	protected int m_SpineLayerIndex = -1;

	private static readonly bool NEW_ANIM_OVERRIDES = true;

	private Dictionary<string, List<KeyValuePair<string, string>>> m_ClipReplaceMap;

	[HideInInspector]
	public Dictionary<string, List<KeyValuePair<AnimationClip, AnimationClip>>> m_ClipOverrides;

	private List<KeyValuePair<AnimationClip, AnimationClip>> m_SetOverrides;

	private RuntimeAnimatorController m_OriginalController;

	private bool m_ClipsReplaced;
}
