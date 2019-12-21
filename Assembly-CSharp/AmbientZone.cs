using System;
using CJTools;
using UnityEngine;

public class AmbientZone : SensorBase, IMSMultisampleController
{
	protected override void Awake()
	{
		base.Awake();
		if (this.m_InnerSource)
		{
			DebugUtils.Assert(this.m_InnerSource.isTrigger, string.Format("m_InnerSource for '{0}' position: {1} is not a trigger!", base.name, base.transform.position), true, DebugUtils.AssertType.Info);
		}
		if (this.m_SpatialBlendMax > 0f)
		{
			this.m_HaloSize = Mathf.Max(1f, this.m_HaloSize);
		}
	}

	protected override void OnEnter()
	{
		base.OnEnter();
		this.m_Multisample = MSManager.Get().PlayMultiSample(this, this.m_MultisampleName, this.m_FadeIn, this.m_ChangeRainforestAmbientVolume, this.m_RainforestAmbientVolume);
	}

	protected override void OnExit()
	{
		base.OnExit();
		MSManager.Get().StopMultiSample(this, this.m_Multisample, (this.m_InnerSource == null) ? this.m_FadeOut : 0f);
	}

	public void UpdateSpatialBlend(out float blend_value, out Vector3? inner_pos, out float distance)
	{
		if (this.m_InnerSource == null)
		{
			blend_value = 0f;
			inner_pos = null;
			distance = 0f;
			return;
		}
		Vector3 worldPosition = Player.Get().GetWorldPosition();
		inner_pos = new Vector3?(this.m_InnerSource.ClosestPoint(worldPosition));
		distance = worldPosition.Distance(inner_pos.Value);
		blend_value = CJTools.Math.GetProportionalClamp(0f, this.m_SpatialBlendMax, distance, 0.01f, this.m_HaloSize);
	}

	public string m_MultisampleName = string.Empty;

	private MSMultiSample m_Multisample;

	[Tooltip("BoxCollider used for spatialization")]
	public BoxCollider m_InnerSource;

	[Tooltip("Max value of 3D sound in spatial blend")]
	public float m_SpatialBlendMax;

	[Tooltip("Distance to inner source where 3D sound starts to fade to 2D")]
	public float m_HaloSize = 3f;

	public float m_FadeIn = 1f;

	public float m_FadeOut = 1f;

	public bool m_ChangeRainforestAmbientVolume;

	public float m_RainforestAmbientVolume = 1f;
}
