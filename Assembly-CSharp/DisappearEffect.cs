using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using UnityEngine;

public class DisappearEffect : MonoBehaviour
{
	public void Initialize(Transform parent)
	{
		if (!DisappearEffect.s_DisappearShader)
		{
			DisappearEffect.s_DisappearShader = Shader.Find("Custom/CharacterDissapear");
		}
		if (!DisappearEffect.s_NoiseTexture)
		{
			DisappearEffect.s_NoiseTexture = Resources.Load<Texture>("DisappearEffect/DisappearNoise");
		}
		this.m_StartTime = Time.time;
		AI component = parent.gameObject.GetComponent<AI>();
		if (component)
		{
			this.m_SkinnedRenderer = component.m_Renderer;
			this.m_Animator = component.m_Animator;
			component.m_AnimationModule.enabled = false;
		}
		if (!this.m_SkinnedRenderer)
		{
			this.m_SkinnedRenderer = parent.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		}
		if (!this.m_SkinnedRenderer)
		{
			this.m_MeshRenderer = parent.gameObject.GetComponentInChildren<MeshRenderer>();
		}
		if (!this.m_MeshRenderer && !this.m_SkinnedRenderer)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			UnityEngine.Object.Destroy(this.m_Parent);
			return;
		}
		this.m_Renderer = ((!(this.m_SkinnedRenderer != null)) ? this.m_MeshRenderer : this.m_SkinnedRenderer);
		for (int i = 0; i < this.m_Renderer.materials.Length; i++)
		{
			this.m_Renderer.materials[i].SetTexture("_CutTex", DisappearEffect.s_NoiseTexture);
		}
		Renderer[] componentsInChildren = parent.gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer != this.m_Renderer)
			{
				renderer.gameObject.SetActive(false);
			}
		}
		this.m_Particles = new List<ParticleSystem>(base.gameObject.GetComponentsInChildren<ParticleSystem>());
		foreach (ParticleSystem particleSystem in this.m_Particles)
		{
			ParticleSystem.ShapeModule shape = particleSystem.shape;
			if (shape.meshShapeType == ParticleSystemMeshShapeType.Vertex)
			{
				if (this.m_SkinnedRenderer)
				{
					shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
					shape.skinnedMeshRenderer = this.m_SkinnedRenderer;
				}
				else
				{
					shape.shapeType = ParticleSystemShapeType.MeshRenderer;
					shape.meshRenderer = this.m_MeshRenderer;
				}
			}
		}
		this.m_Parent = parent.gameObject;
	}

	private void Update()
	{
		float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, Time.time - this.m_StartTime, 0f, this.m_Duration);
		if (this.m_Renderer)
		{
			foreach (Material material in this.m_Renderer.materials)
			{
				material.SetFloat("_NoiseCut", 1f - proportionalClamp);
				material.SetFloat("_Cutoff", proportionalClamp);
			}
		}
		if (this.m_Animator)
		{
			this.m_Animator.speed = 1f - proportionalClamp;
		}
		bool flag = false;
		foreach (ParticleSystem particleSystem in this.m_Particles)
		{
			if (particleSystem.IsAlive())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			UnityEngine.Object.Destroy(this.m_Parent);
		}
	}

	private SkinnedMeshRenderer m_SkinnedRenderer;

	private MeshRenderer m_MeshRenderer;

	private Renderer m_Renderer;

	private Animator m_Animator;

	private List<ParticleSystem> m_Particles;

	private float m_StartTime;

	public float m_Duration = 1f;

	private GameObject m_Parent;

	private static Shader s_DisappearShader;

	private static Texture s_NoiseTexture;
}
