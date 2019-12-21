using System;
using System.Collections.Generic;
using UnityEngine;

public class MiaDisappearEffectController : MonoBehaviour
{
	private void Awake()
	{
		this.m_Materials.Clear();
		if (this.m_Renderers != null)
		{
			for (int i = 0; i < this.m_Renderers.Count; i++)
			{
				if (!(this.m_Renderers[i] == null))
				{
					Material[] materials = this.m_Renderers[i].materials;
					for (int j = 0; j < materials.Length; j++)
					{
						this.m_Materials.Add(materials[j]);
					}
				}
			}
		}
		this.InitializeVerticesPosition();
		this.m_Prefab = GreenHellGame.Instance.GetPrefab(this.m_FXPrefabName);
	}

	private void OnEnable()
	{
		this.ResetParams();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(true);
		}
		this.ResetVerticesPosition();
	}

	private void ResetParams()
	{
		this.m_MaskValueCurrent = this.m_MaskValueMax;
		this.m_StartTime = Time.time;
	}

	private void Update()
	{
		bool flag = false;
		float num = this.m_Curve.Evaluate((Time.time - this.m_StartTime) / this.m_Duration);
		if (num < 0f)
		{
			num = 0f;
		}
		else if (num > 1f)
		{
			flag = true;
			num = 1f;
		}
		for (int i = 0; i < this.m_Materials.Count; i++)
		{
			this.m_Materials[i].SetFloat(this.m_MaskProperty, this.m_MaskValueMax - num * (this.m_MaskValueMax - this.m_MaskValueMin));
		}
		if (flag)
		{
			if (this.m_Loop)
			{
				this.ResetParams();
			}
			else
			{
				base.enabled = false;
			}
		}
		this.UpdateParticlesSpawn();
	}

	private void UpdateParticlesSpawn()
	{
		float num = 1f - this.m_Curve.Evaluate((Time.time - this.m_StartTime) / this.m_Duration);
		float num2 = this.m_VerticesMinY + num * (this.m_VerticesMaxY - this.m_VerticesMinY);
		for (int i = 0; i < this.m_ParticleSpawnPos.Count; i++)
		{
			MiaDisappearEffectController.MiaDisappearEffectParticleSpawnPoint miaDisappearEffectParticleSpawnPoint = this.m_ParticleSpawnPos[i];
			if (!miaDisappearEffectParticleSpawnPoint.m_AlreadySpawned && miaDisappearEffectParticleSpawnPoint.m_Pos.y >= num2)
			{
				miaDisappearEffectParticleSpawnPoint.m_AlreadySpawned = true;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_Prefab, miaDisappearEffectParticleSpawnPoint.m_Pos, Quaternion.identity);
				ParticlesManager.Get().Spawn(gameObject, miaDisappearEffectParticleSpawnPoint.m_Pos, Quaternion.identity, Vector3.zero, null, -1f, false);
				ParticleSystem.ColorOverLifetimeModule colorOverLifetime = gameObject.GetComponent<ParticleSystem>().colorOverLifetime;
				ParticleSystem.MinMaxGradient color = colorOverLifetime.color;
				if (color.gradient != null)
				{
					GradientColorKey[] array = new GradientColorKey[2];
					GradientAlphaKey[] array2 = new GradientAlphaKey[2];
					array[0].color = miaDisappearEffectParticleSpawnPoint.m_Color;
					array[0].time = 0f;
					array[1].color = miaDisappearEffectParticleSpawnPoint.m_Color;
					array[1].time = 1f;
					array2[0].alpha = 1f;
					array2[0].time = 0f;
					array2[1].alpha = 0f;
					array2[1].time = 1f;
					color.gradient.SetKeys(array, array2);
				}
				colorOverLifetime.color = color;
			}
		}
	}

	private void InitializeVerticesPosition()
	{
		if (this.m_MeshFilter == null || this.m_MeshFilter.mesh == null)
		{
			return;
		}
		this.m_ParticleSpawnPos.Clear();
		Vector3[] vertices = this.m_MeshFilter.mesh.vertices;
		Color[] colors = this.m_MeshFilter.mesh.colors;
		this.m_VerticesMaxY = float.MinValue;
		this.m_VerticesMinY = float.MaxValue;
		if (colors.Length != vertices.Length)
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			return;
		}
		for (int i = 0; i < vertices.Length; i += this.m_EveryNthVertex)
		{
			MiaDisappearEffectController.MiaDisappearEffectParticleSpawnPoint miaDisappearEffectParticleSpawnPoint = new MiaDisappearEffectController.MiaDisappearEffectParticleSpawnPoint();
			miaDisappearEffectParticleSpawnPoint.m_AlreadySpawned = false;
			miaDisappearEffectParticleSpawnPoint.m_Pos = this.m_MeshFilter.gameObject.transform.TransformPoint(vertices[i]);
			miaDisappearEffectParticleSpawnPoint.m_Color = colors[i];
			this.m_ParticleSpawnPos.Add(miaDisappearEffectParticleSpawnPoint);
			if (miaDisappearEffectParticleSpawnPoint.m_Pos.y > this.m_VerticesMaxY)
			{
				this.m_VerticesMaxY = miaDisappearEffectParticleSpawnPoint.m_Pos.y;
			}
			if (miaDisappearEffectParticleSpawnPoint.m_Pos.y < this.m_VerticesMinY)
			{
				this.m_VerticesMinY = miaDisappearEffectParticleSpawnPoint.m_Pos.y;
			}
		}
	}

	private void ResetVerticesPosition()
	{
		for (int i = 0; i < this.m_ParticleSpawnPos.Count; i++)
		{
			this.m_ParticleSpawnPos[i].m_AlreadySpawned = false;
		}
	}

	public List<Renderer> m_Renderers = new List<Renderer>();

	private List<Material> m_Materials = new List<Material>();

	private int m_MaskProperty = Shader.PropertyToID("_disappear_mask_multiply");

	public float m_MaskValueMax = 1f;

	public float m_MaskValueMin;

	public float m_Duration = 3f;

	private float m_MaskValueCurrent = 1f;

	private float m_StartTime;

	public bool m_Loop;

	public MeshFilter m_MeshFilter;

	private GameObject m_Prefab;

	public float m_VerticesMaxY = float.MinValue;

	public float m_VerticesMinY = float.MaxValue;

	public int m_EveryNthVertex = 1;

	public string m_FXPrefabName = "mia_disappear_single";

	public AnimationCurve m_Curve = new AnimationCurve();

	private List<MiaDisappearEffectController.MiaDisappearEffectParticleSpawnPoint> m_ParticleSpawnPos = new List<MiaDisappearEffectController.MiaDisappearEffectParticleSpawnPoint>();

	private class MiaDisappearEffectParticleSpawnPoint
	{
		public Vector3 m_Pos = Vector3.zero;

		public bool m_AlreadySpawned;

		public Color m_Color;
	}
}
