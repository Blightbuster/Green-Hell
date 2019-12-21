using System;
using UnityEngine;

public class LuxWater_SetToGerstnerHeight : MonoBehaviour
{
	private void Start()
	{
		this.trans = base.transform;
		this.pos = this.trans.position;
		LuxWaterUtils.GetGersterWavesDescription(ref this.Description, this.WaterMaterial);
	}

	private void OnBecameVisible()
	{
		this.ObjectIsVisible = true;
	}

	private void OnBecameInvisible()
	{
		this.ObjectIsVisible = false;
	}

	private void Update()
	{
		if (this.ObjectIsVisible)
		{
			if (this.WaterMaterial == null)
			{
				return;
			}
			if (this.UpdateWaterMaterialPerFrame)
			{
				LuxWaterUtils.GetGersterWavesDescription(ref this.Description, this.WaterMaterial);
			}
			Vector3 gestnerDisplacement = LuxWaterUtils.GetGestnerDisplacement(base.transform.position, this.Description, this.TimeOffset);
			Vector3 position = this.pos;
			position.x += gestnerDisplacement.x * this.Damping.x;
			position.y += gestnerDisplacement.y * this.Damping.y;
			position.z += gestnerDisplacement.z * this.Damping.z;
			this.trans.position = position;
		}
	}

	public Material WaterMaterial;

	public Vector3 Damping = new Vector3(0.3f, 1f, 0.3f);

	public float TimeOffset;

	public bool UpdateWaterMaterialPerFrame;

	private Transform trans;

	private Vector3 pos;

	private LuxWaterUtils.GersterWavesDescription Description;

	private bool ObjectIsVisible;
}
