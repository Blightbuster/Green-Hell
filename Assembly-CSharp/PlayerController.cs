using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;

public class PlayerController : MonoBehaviour, IAnimationEventsReceiver, IInputsReceiver
{
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
		if (this.m_ClipReplaceMap == null)
		{
			this.m_ClipReplaceMap = new Dictionary<string, List<KeyValuePair<string, string>>>();
		}
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "Item")
			{
				string svalue = key.GetVariable(0).SValue;
				this.m_ClipReplaceMap[svalue] = new List<KeyValuePair<string, string>>();
				for (int j = 0; j < key.GetKeysCount(); j++)
				{
					Key key2 = key.GetKey(j);
					if (key2.GetName() == "Clip")
					{
						KeyValuePair<string, string> item = new KeyValuePair<string, string>(key2.GetVariable(0).SValue, key2.GetVariable(1).SValue);
						this.m_ClipReplaceMap[svalue].Add(item);
					}
				}
			}
		}
	}

	protected virtual void Start()
	{
		InputsManager.Get().RegisterReceiver(this);
	}

	public virtual void OnInputAction(InputsManager.InputAction action)
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
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
		animatorOverrideController.runtimeAnimatorController = this.m_Animator.runtimeAnimatorController;
		this.m_Animator.runtimeAnimatorController = animatorOverrideController;
		for (int k = 0; k < this.m_ClipReplaceMap[text].Count; k++)
		{
			AnimationClip animationClip = animatorOverrideController[this.m_ClipReplaceMap[text][k].Value];
			animatorOverrideController[this.m_ClipReplaceMap[text][k].Key] = animatorOverrideController[this.m_ClipReplaceMap[text][k].Value];
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

	private void RestoreClips()
	{
		if (this.m_ClipsReplaced && this.m_Animator.runtimeAnimatorController != this.m_OriginalController)
		{
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
			this.m_Animator.runtimeAnimatorController = this.m_OriginalController;
			this.m_Animator.Update(0f);
			for (int k = 0; k < this.m_Animator.parameterCount; k++)
			{
				this.SetAnimatorParameterData(this.m_Animator.parameters[k], array2[k]);
			}
			for (int l = 0; l < this.m_Animator.layerCount; l++)
			{
				this.m_Animator.Play(array[l].fullPathHash, l, array[l].normalizedTime);
			}
			this.m_ClipsReplaced = false;
		}
	}

	private object GetAnimatorParameterData(AnimatorControllerParameter param)
	{
		object result = null;
		AnimatorControllerParameterType type = param.type;
		if (type != AnimatorControllerParameterType.Int)
		{
			if (type != AnimatorControllerParameterType.Float)
			{
				if (type == AnimatorControllerParameterType.Bool)
				{
					result = this.m_Animator.GetBool(param.nameHash);
				}
			}
			else
			{
				result = this.m_Animator.GetFloat(param.nameHash);
			}
		}
		else
		{
			result = this.m_Animator.GetInteger(param.nameHash);
		}
		return result;
	}

	private void SetAnimatorParameterData(AnimatorControllerParameter param, object data)
	{
		AnimatorControllerParameterType type = param.type;
		if (type != AnimatorControllerParameterType.Int)
		{
			if (type != AnimatorControllerParameterType.Float)
			{
				if (type == AnimatorControllerParameterType.Bool)
				{
					this.m_Animator.SetBool(param.nameHash, (bool)data);
				}
			}
			else
			{
				this.m_Animator.SetFloat(param.nameHash, (float)data);
			}
		}
		else
		{
			this.m_Animator.SetInteger(param.nameHash, (int)data);
		}
	}

	protected virtual void OnDestroy()
	{
		InputsManager.Get().UnregisterReceiver(this);
	}

	protected PlayerControllerType m_ControllerType = PlayerControllerType.Unknown;

	protected Player m_Player;

	[HideInInspector]
	public Animator m_Animator;

	protected int m_BaseLayerIndex = -1;

	protected int m_SpineLayerIndex = -1;

	private Dictionary<string, List<KeyValuePair<string, string>>> m_ClipReplaceMap;

	private RuntimeAnimatorController m_OriginalController;

	private bool m_ClipsReplaced;
}
