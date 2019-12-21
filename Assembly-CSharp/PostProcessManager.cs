using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessManager : MonoBehaviour
{
	public static global::PostProcessManager Get()
	{
		return global::PostProcessManager.s_Instance;
	}

	private void Awake()
	{
		global::PostProcessManager.s_Instance = this;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(true);
		}
		this.m_Volumes = new PostProcessVolume[12];
		PostProcessVolume[] componentsInChildren = base.GetComponentsInChildren<PostProcessVolume>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			for (global::PostProcessManager.Effect effect = global::PostProcessManager.Effect.Game; effect < global::PostProcessManager.Effect.Count; effect++)
			{
				if (componentsInChildren[j].gameObject.name.EndsWith(effect.ToString()))
				{
					this.m_Volumes[(int)effect] = componentsInChildren[j];
					componentsInChildren[j].priority = (float)this.GetPriority(effect);
					this.SetWeight(effect, (effect == global::PostProcessManager.Effect.Game) ? 1f : 0f);
				}
			}
		}
	}

	private int GetPriority(global::PostProcessManager.Effect effect)
	{
		switch (effect)
		{
		case global::PostProcessManager.Effect.Game:
			return 9;
		case global::PostProcessManager.Effect.Blood:
			return 8;
		case global::PostProcessManager.Effect.Poison:
			return 2;
		case global::PostProcessManager.Effect.InGameMenu:
			return 2;
		case global::PostProcessManager.Effect.Sanity:
			return 10;
		case global::PostProcessManager.Effect.Notepad:
			return 7;
		case global::PostProcessManager.Effect.Coca:
			return 10;
		case global::PostProcessManager.Effect.LowHP:
			return 3;
		case global::PostProcessManager.Effect.LowEnergy:
			return 2;
		case global::PostProcessManager.Effect.Underwater:
			return 3;
		case global::PostProcessManager.Effect.Dream:
			return 3;
		case global::PostProcessManager.Effect.DebugDof:
			return 3;
		default:
			return 0;
		}
	}

	public float GetWeight(global::PostProcessManager.Effect pp)
	{
		PostProcessVolume postProcessVolume = this.m_Volumes[(int)pp];
		if (!postProcessVolume)
		{
			DebugUtils.Assert("Missing PostProcess - " + pp.ToString(), true, DebugUtils.AssertType.Info);
			return 0f;
		}
		return postProcessVolume.weight;
	}

	public void SetWeight(global::PostProcessManager.Effect pp, float weight)
	{
		PostProcessVolume postProcessVolume = this.m_Volumes[(int)pp];
		if (!postProcessVolume)
		{
			DebugUtils.Assert("Missing PostProcess - " + pp.ToString(), true, DebugUtils.AssertType.Info);
			return;
		}
		postProcessVolume.weight = Mathf.Clamp01(weight);
	}

	public PostProcessVolume GetVolume(global::PostProcessManager.Effect effect)
	{
		return this.m_Volumes[(int)effect];
	}

	private PostProcessVolume[] m_Volumes;

	private static global::PostProcessManager s_Instance;

	public enum Effect
	{
		None = -1,
		Game,
		Blood,
		Poison,
		InGameMenu,
		Sanity,
		Notepad,
		Coca,
		LowHP,
		LowEnergy,
		Underwater,
		Dream,
		DebugDof,
		Count
	}
}
