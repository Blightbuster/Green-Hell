using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ParticleGridGenerator : MonoBehaviour
{
	private void Start()
	{
		this.ps = base.GetComponent<ParticleSystem>();
		this.UpdateGrid();
	}

	private void OnEnable()
	{
		this.ps = base.GetComponent<ParticleSystem>();
		this.UpdateGrid();
	}

	public void UpdateGrid()
	{
		this.GenerateGrid();
		this.GenerateParticles();
		this.CreateOffsetVector();
		ParticleSystemRenderer component = base.GetComponent<ParticleSystemRenderer>();
		if (this.rewriteVertexStreams)
		{
			ParticleSystemRenderer particleSystemRenderer = component;
			ParticleSystemVertexStream[] array = new ParticleSystemVertexStream[7];
			RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.B084C67FF0B307B33AF49050A3B2138803BE2C60).FieldHandle);
			particleSystemRenderer.SetActiveVertexStreams(new List<ParticleSystemVertexStream>(array));
		}
		component.alignment = ParticleSystemRenderSpace.Local;
	}

	private void GenerateGrid()
	{
		this.positions = new Vector3[this.xSize * this.ySize * this.zSize];
		int i = 0;
		int num = 0;
		while (i < this.zSize)
		{
			this.even = 0f;
			if (i % 2 == 0)
			{
				this.even = this.OffsetEven;
			}
			for (int j = 0; j < this.ySize; j++)
			{
				int k = 0;
				while (k < this.xSize)
				{
					this.positions[num] = new Vector3((float)k * this.xDistance + this.even, (float)j * this.yDistance, (float)i * this.zDistance);
					k++;
					num++;
				}
			}
			i++;
		}
	}

	private void GenerateParticles()
	{
		this.particles = new ParticleSystem.Particle[this.xSize * this.ySize * this.zSize];
		for (int i = 0; i < this.particles.Length; i++)
		{
			this.particles[i].position = this.positions[i];
			if (this.randomColorAlpha)
			{
				this.particleColor.a = UnityEngine.Random.Range(0f, 1f);
			}
			this.particles[i].startColor = this.particleColor;
			this.particles[i].startSize = this.particleSize;
			this.particles[i].rotation3D = this.particleRotation3D;
		}
		this.ps.SetParticles(this.particles, this.particles.Length);
	}

	private void CreateOffsetVector()
	{
		this.ps.GetCustomParticleData(this.customData, ParticleSystemCustomData.Custom1);
		for (int i = 0; i < this.particles.Length; i++)
		{
			this.customData[i] = base.gameObject.transform.up;
		}
		this.ps.SetCustomParticleData(this.customData, ParticleSystemCustomData.Custom1);
	}

	private void FixedUpdate()
	{
		if (this.updateEveryFrame)
		{
			this.UpdateGrid();
		}
	}

	public bool rewriteVertexStreams = true;

	public float particleSize = 1f;

	public Color particleColor = Color.white;

	public Vector3 particleRotation3D;

	public bool randomColorAlpha = true;

	public float xDistance = 0.25f;

	public float yDistance = 0.25f;

	public float zDistance = 0.25f;

	public int xSize = 10;

	public int ySize = 10;

	public int zSize = 10;

	public float OffsetEven = 0.125f;

	public bool updateEveryFrame;

	private float even;

	private Vector3[] positions;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private List<Vector4> customData = new List<Vector4>();

	private List<Vector4> customData2 = new List<Vector4>();
}
