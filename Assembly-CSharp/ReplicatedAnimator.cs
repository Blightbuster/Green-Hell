using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ReplicatedAnimator : ReplicatedBehaviour
{
	public Animator LocalSourceAnimator
	{
		get
		{
			return this.m_LocalSourceAnimator;
		}
	}

	public Animator NetAnimator
	{
		get
		{
			return this.m_NetAnimator;
		}
	}

	public void OnParamChanged()
	{
		this.m_AnimatorParamsDirty = true;
	}

	public void OnAnimChanged()
	{
		this.m_LayeredAnimationsDirty = true;
	}

	private void Awake()
	{
		if (this.m_UseLocalPlayerSource && base.ReplIsOwner())
		{
			this.m_SourceAnimatorObject = Player.Get().gameObject;
			DebugUtils.Assert(this.m_SourceAnimatorObject, true);
		}
		else
		{
			this.m_SourceAnimatorObject = base.gameObject;
		}
		this.m_LocalSourceAnimator = this.m_SourceAnimatorObject.GetComponent<Animator>();
		DebugUtils.Assert(this.m_LocalSourceAnimator, true);
		this.m_LayeredAnimationStringHashes = new int[this.m_LocalSourceAnimator.layerCount];
		if (this.m_SourceAnimatorObject != base.gameObject || P2PNetworkManager.s_TPPTest)
		{
			this.m_NetAnimator = base.GetComponent<Animator>();
		}
		else
		{
			this.m_NetAnimator = this.m_LocalSourceAnimator;
		}
		DebugUtils.Assert(this.m_NetAnimator, true);
		this.m_PrevLayeredAnimationStringHashes = new int[this.m_LayeredAnimationStringHashes.Length];
		this.m_LayeredAnimationStringHashes.CopyTo(this.m_PrevLayeredAnimationStringHashes, 0);
		if (this.m_ReplicatedAnimatorParamsScript)
		{
			this.ParseScript(this.m_ReplicatedAnimatorParamsScript);
			List<ReplicatedAnimator.AnimParam> replicatedAnimatorParams = this.m_ReplicatedAnimatorParams;
			for (int i = 0; i < replicatedAnimatorParams.Count; i++)
			{
				ReplicatedAnimator.AnimParam animParam = replicatedAnimatorParams[i];
				if (animParam.data_type == typeof(int))
				{
					this.m_AnimatorParams.Add(new AnimatorParamReplicatorInt(animParam.param_name, this));
				}
				else if (animParam.data_type == typeof(float))
				{
					this.m_AnimatorParams.Add(new AnimatorParamReplicatorFloat(animParam.param_name, this));
				}
				else if (animParam.data_type == typeof(bool))
				{
					this.m_AnimatorParams.Add(new AnimatorParamReplicatorBool(animParam.param_name, this));
				}
				else
				{
					DebugUtils.Assert(false, true);
				}
			}
		}
	}

	private void OnEnable()
	{
		if (!base.ReplIsOwner())
		{
			this.UpdateNonOwner();
		}
	}

	private void PrepareLayeredAnimations()
	{
		if (this.LocalSourceAnimator && this.LocalSourceAnimator.runtimeAnimatorController != null)
		{
			for (int i = 0; i < this.LocalSourceAnimator.layerCount; i++)
			{
				AnimatorStateInfo currentAnimatorStateInfo = this.LocalSourceAnimator.GetCurrentAnimatorStateInfo(i);
				if (this.m_LayeredAnimationStringHashes[i] != currentAnimatorStateInfo.shortNameHash)
				{
					this.m_LayeredAnimationStringHashes[i] = currentAnimatorStateInfo.shortNameHash;
					this.OnAnimChanged();
				}
			}
		}
	}

	private void PrepareParameters()
	{
		for (int i = 0; i < this.m_AnimatorParams.Count; i++)
		{
			this.m_AnimatorParams[i].UpdateCurrent();
		}
	}

	private void ParseScript(TextAsset script)
	{
		TextAssetParser textAssetParser = new TextAssetParser(script);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "PropFloat")
			{
				this.m_ReplicatedAnimatorParams.Add(new ReplicatedAnimator.AnimParam(key.GetVariable(0).SValue, typeof(float)));
			}
			else if (key.GetName() == "PropInt")
			{
				this.m_ReplicatedAnimatorParams.Add(new ReplicatedAnimator.AnimParam(key.GetVariable(0).SValue, typeof(int)));
			}
			else if (key.GetName() == "PropBool")
			{
				this.m_ReplicatedAnimatorParams.Add(new ReplicatedAnimator.AnimParam(key.GetVariable(0).SValue, typeof(bool)));
			}
			else if (key.GetName() == "DontInterruptState")
			{
				int key2 = Animator.StringToHash(key.GetVariable(0).SValue);
				if (!this.m_NonInterruptableStateSets.ContainsKey(key2))
				{
					this.m_NonInterruptableStateSets.Add(key2, new HashSet<int>());
				}
				this.m_NonInterruptableStateSets[key2].Add(Animator.StringToHash(key.GetVariable(1).SValue));
			}
		}
	}

	private void Update()
	{
		if (!base.ReplIsOwner() || P2PNetworkManager.s_TPPTest)
		{
			for (int i = 0; i < this.m_AnimatorParams.Count; i++)
			{
				this.m_AnimatorParams[i].Update(Time.deltaTime);
			}
			if (this.m_StateChangeDelayed)
			{
				this.UpdateNonOwner();
			}
		}
		if (P2PNetworkManager.s_TPPTest)
		{
			if (ReplTools.IsPlayingAlone())
			{
				this.OnReplicationPrepare();
			}
			this.OnReplicationResolve();
			for (int j = 0; j < this.m_AnimatorParams.Count; j++)
			{
				this.m_AnimatorParams[j].TestUpdate();
			}
		}
	}

	public override void OnReplicationPrepare()
	{
		this.PrepareLayeredAnimations();
		this.PrepareParameters();
		if (this.m_AnimatorParamsDirty || this.m_LayeredAnimationsDirty)
		{
			this.ReplSetDirty();
		}
	}

	public override void OnReplicationResolve()
	{
		if (!base.enabled)
		{
			return;
		}
		if (this.NetAnimator == null)
		{
			return;
		}
		this.UpdateNonOwner();
	}

	public void UpdateNonOwner()
	{
		DebugUtils.Assert(!base.ReplIsOwner(), true);
		this.m_StateChangeDelayed = false;
		for (int i = 0; i < this.NetAnimator.layerCount; i++)
		{
			if (this.m_LayeredAnimationStringHashes[i] != this.m_PrevLayeredAnimationStringHashes[i])
			{
				AnimatorStateInfo currentAnimatorStateInfo = this.NetAnimator.GetCurrentAnimatorStateInfo(i);
				bool flag = false;
				HashSet<int> hashSet;
				if (!currentAnimatorStateInfo.loop && currentAnimatorStateInfo.normalizedTime < 1f && this.m_NonInterruptableStateSets.TryGetValue(this.m_PrevLayeredAnimationStringHashes[i], out hashSet))
				{
					flag = hashSet.Contains(this.m_LayeredAnimationStringHashes[i]);
				}
				if (flag)
				{
					this.m_StateChangeDelayed = true;
				}
				else
				{
					this.NetAnimator.CrossFadeInFixedTime(this.m_LayeredAnimationStringHashes[i], this.m_TransitionDuration, i);
					this.m_PrevLayeredAnimationStringHashes[i] = this.m_LayeredAnimationStringHashes[i];
				}
			}
		}
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initialState)
	{
		if (this.m_AnimatorParamsDirty || initialState)
		{
			writer.Write(this.m_AnimatorParams.Count);
			for (int i = 0; i < this.m_AnimatorParams.Count; i++)
			{
				this.m_AnimatorParams[i].Serialize(writer);
			}
			this.m_AnimatorParamsDirty = false;
		}
		else
		{
			writer.Write(0);
		}
		if (this.m_LayeredAnimationsDirty || initialState)
		{
			writer.Write(this.m_LayeredAnimationStringHashes.Length);
			for (int j = 0; j < this.m_LayeredAnimationStringHashes.Length; j++)
			{
				writer.Write(this.m_LayeredAnimationStringHashes[j]);
			}
			this.m_LayeredAnimationsDirty = false;
			return;
		}
		writer.Write(0);
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initialState)
	{
		int num = reader.ReadInt32();
		if (num > 0)
		{
			if (num == this.m_AnimatorParams.Count)
			{
				for (int i = 0; i < this.m_AnimatorParams.Count; i++)
				{
					this.m_AnimatorParams[i].Deserialize(reader);
				}
			}
			else if (P2PLogFilter.logError)
			{
				Debug.LogError("AnimatorReplicator: Wrong animation parameter count");
			}
		}
		int num2 = reader.ReadInt32();
		if (num2 > 0)
		{
			if (num2 == this.m_LayeredAnimationStringHashes.Length)
			{
				for (int j = 0; j < this.m_LayeredAnimationStringHashes.Length; j++)
				{
					this.m_LayeredAnimationStringHashes[j] = reader.ReadInt32();
				}
				return;
			}
			if (P2PLogFilter.logError)
			{
				Debug.LogError("AnimatorReplicator: Wrong animation count");
			}
		}
	}

	private List<ReplicatedAnimator.AnimParam> m_ReplicatedAnimatorParams = new List<ReplicatedAnimator.AnimParam>();

	[SerializeField]
	public bool m_UseLocalPlayerSource;

	[SerializeField]
	public GameObject m_SourceAnimatorObject;

	[SerializeField]
	public float m_TransitionDuration = 0.25f;

	[SerializeField]
	public TextAsset m_ReplicatedAnimatorParamsScript;

	private Animator m_LocalSourceAnimator;

	private Animator m_NetAnimator;

	private bool m_AnimatorParamsDirty;

	private List<IAnimatorParamReplicator> m_AnimatorParams = new List<IAnimatorParamReplicator>();

	private bool m_LayeredAnimationsDirty;

	private int[] m_LayeredAnimationStringHashes;

	private int[] m_PrevLayeredAnimationStringHashes;

	public float m_SpringSpeed = 20f;

	private bool m_StateChangeDelayed;

	private Dictionary<int, HashSet<int>> m_NonInterruptableStateSets = new Dictionary<int, HashSet<int>>();

	public struct AnimParam
	{
		public AnimParam(string name, Type type)
		{
			this.data_type = type;
			this.param_name = name;
		}

		public readonly Type data_type;

		public readonly string param_name;
	}
}
