using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	[RequireComponent(typeof(Camera))]
	public class WaterRaindropsIME : MonoBehaviour
	{
		public Vector3 WorldForce
		{
			get
			{
				return base.transform.worldToLocalMatrix * this.Force;
			}
		}

		public void Spawn(Vector3 velocity, float volume, float life, float x, float y)
		{
			Vector2 position = new Vector2(x, y);
			WaterRaindropsIME.Droplet item;
			item.Position = position;
			item.Volume = volume;
			item.Velocity = base.transform.worldToLocalMatrix * -velocity;
			item.Velocity.x = -item.Velocity.x;
			item.Life = life;
			this._Droplets.Add(item);
		}

		private void Awake()
		{
			this.CreateMaterials();
			this.CreateSimulation();
			this.SetMaterialProperties();
		}

		private void Start()
		{
			this.Tracking.Initialize(this);
		}

		private void OnPreCull()
		{
			this.Render();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			this.FadeMaps();
			this._FinalMaterial.SetTexture("unity_Spec", this._CubeMap);
			if (source.texelSize.y < 0f)
			{
				if (this._InvertMaterial == null)
				{
					this._InvertMaterial = new Material(Shader.Find("Hidden/InvertY"));
				}
				RenderTexture renderTexture = source.CreateTemporary();
				Graphics.Blit(source, renderTexture);
				Graphics.Blit(renderTexture, source);
				Graphics.Blit(source, renderTexture, this._FinalMaterial);
				Graphics.Blit(renderTexture, destination, this._InvertMaterial);
				renderTexture.ReleaseTemporary();
			}
			else
			{
				Graphics.Blit(source, destination, this._FinalMaterial);
			}
		}

		private void Update()
		{
			this.Tracking.Advance();
			this.Advance();
		}

		private void OnValidate()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (this._Initialized)
			{
				this.SetMaterialProperties();
			}
		}

		private void OnDestroy()
		{
			TextureUtility.Release(ref this._Target);
		}

		private static bool IsVisible(WaterRaindropsIME.Droplet droplet)
		{
			bool flag = true;
			flag &= (droplet.Position.x >= 0f && droplet.Position.x <= 1f);
			return flag & (droplet.Position.y >= 0f && droplet.Position.y <= 1f);
		}

		private void OnDropletUpdate(ref WaterRaindropsIME.Droplet droplet)
		{
			Vector2 position = droplet.Position;
			Vector2 velocity = droplet.Velocity;
			droplet.Life -= Time.fixedDeltaTime;
			if (droplet.Volume < 0.2f)
			{
				return;
			}
			Vector4 vector = base.transform.worldToLocalMatrix * this.Force;
			Vector2 a = new Vector2(vector.x, -vector.y);
			Vector2 b = new Vector2(this.Tracking.Force.x, this.Tracking.Force.y);
			Vector2 a2 = Vector2.zero;
			if (this._Friction != null)
			{
				a2 = this._Friction[(int)(position.x * (float)(this._Target.width - 1)), (int)(position.y * (float)(this._Target.height - 1))];
			}
			float magnitude = velocity.magnitude;
			Vector2 normalized = velocity.normalized;
			Vector2 b2 = -normalized * ((magnitude + 1f) * (magnitude + 1f)) * this.AirFriction;
			Vector2 b3 = -a2 * this.WindowFrictionMultiplier;
			Vector2 a3 = a + b + b2 + b3;
			Vector2 a4 = a3 / droplet.Volume;
			droplet.Velocity += a4 * Time.deltaTime;
			droplet.Position += droplet.Velocity * Time.deltaTime;
			float num = Vector3.Distance(droplet.Position, position);
			float num2 = num * this.VolumeLoss + 0.001f;
			droplet.Volume -= num2;
		}

		private void Draw(int index, Vector3 position, float size, Vector2 velocity)
		{
			float angle = Mathf.Atan2(velocity.y, velocity.x) * 57.29578f;
			this._Matrices[index] = Matrix4x4.TRS(position * 2f - new Vector3(1f, 1f, 0f), Quaternion.AngleAxis(angle, Vector3.forward), new Vector3(1f + velocity.magnitude * 10f, 1f, 1f) * size * 0.1f);
		}

		private void Advance()
		{
			this._Droplets.RemoveAll((WaterRaindropsIME.Droplet x) => !WaterRaindropsIME.IsVisible(x) || x.Life <= 0f || x.Volume <= 0f);
			for (int i = 0; i < this._Droplets.Count; i++)
			{
				WaterRaindropsIME.Droplet droplet = this._Droplets[i];
				this.OnDropletUpdate(ref droplet);
				if (!WaterRaindropsIME.IsVisible(droplet))
				{
					this._Droplets[i] = droplet;
				}
				else
				{
					this.Draw(i, droplet.Position, droplet.Volume, droplet.Velocity);
					this._Droplets[i] = droplet;
				}
			}
		}

		private void FadeMaps()
		{
			RenderTexture temporary = RenderTexture.GetTemporary(this._Target.width, this._Target.height, this._Target.depth, this._Target.format);
			temporary.filterMode = this._Target.filterMode;
			this._FadeMaterial.SetVector("_Modulation_STX", Vector4.one * 10f);
			Graphics.Blit(this._Target, temporary, this._FadeMaterial);
			Graphics.CopyTexture(temporary, this._Target);
			RenderTexture.ReleaseTemporary(temporary);
		}

		private void Render()
		{
			this._Buffer.Clear();
			this._Buffer.SetRenderTarget(this._Target);
			for (int i = 0; i < this._Droplets.Count; i++)
			{
				this._Buffer.DrawMesh(this._Mesh, this._Matrices[i], this._DropletMaterial);
			}
			Graphics.ExecuteCommandBuffer(this._Buffer);
		}

		private void CreateSimulation()
		{
			this._Initialized = true;
			this.CreateRenderTexture();
			this._Matrices = new Matrix4x4[4096];
			this._Buffer = new CommandBuffer();
			this._Mesh = WaterRaindropsIME.BuildQuad(1f, 1f);
			this.CreateFrictionMap();
		}

		private void CreateFrictionMap()
		{
			if (this._Target == null || this.WindowFriction == null)
			{
				return;
			}
			this._Friction = this.Sample(this.WindowFriction, this._Target.width, this._Target.height, 4);
		}

		private void CreateRenderTexture()
		{
			TextureUtility.RenderTextureDesc desc = new TextureUtility.RenderTextureDesc("[UWS] WaterRaindropsIME - Raindrops")
			{
				Height = (int)((float)Screen.height * this.Resolution),
				Width = (int)((float)Screen.width * this.Resolution),
				Format = RenderTextureFormat.RFloat,
				Filter = FilterMode.Bilinear
			};
			this._Target = desc.CreateRenderTexture();
			this._Target.Clear(false, true);
		}

		private void CreateMaterials()
		{
			ShaderUtility instance = ShaderUtility.Instance;
			this._FinalMaterial = instance.CreateMaterial(ShaderList.RaindropsFinal, HideFlags.None);
			this._FadeMaterial = instance.CreateMaterial(ShaderList.RaindropsFade, HideFlags.None);
			this._DropletMaterial = instance.CreateMaterial(ShaderList.RaindropsParticle, HideFlags.None);
		}

		private void SetMaterialProperties()
		{
			this._FinalMaterial.SetTexture("_WaterDropsTex", this._Target);
			this._FinalMaterial.SetFloat("_NormalSpread", this.Distortion.NormalSpread);
			this._FinalMaterial.SetFloat("_Distortion", this.Distortion.Multiplier);
			if (this.Twirl.Texture != null)
			{
				this._FinalMaterial.SetTexture("_Twirl", this.Twirl.Texture);
				this._FinalMaterial.SetFloat("_TwirlMultiplier", this.Twirl.Multiplier);
			}
			this._FadeMaterial.SetFloat("_Value", this.Fade.Intensity);
			this._FadeMaterial.SetFloat("_ModulationStrength", this.Fade.Multiplier);
			if (this.Fade.Texture != null)
			{
				this._FadeMaterial.SetTexture("_Modulation", this.Fade.Texture);
			}
		}

		private static Mesh BuildQuad(float width, float height)
		{
			Mesh mesh = new Mesh();
			Vector3[] array = new Vector3[4];
			float num = height * 0.5f;
			float num2 = width * 0.5f;
			array[0] = new Vector3(-num2, -num, 0f);
			array[1] = new Vector3(-num2, num, 0f);
			array[2] = new Vector3(num2, -num, 0f);
			array[3] = new Vector3(num2, num, 0f);
			Vector2[] array2 = new Vector2[array.Length];
			array2[0] = new Vector2(0f, 0f);
			array2[1] = new Vector2(0f, 1f);
			array2[2] = new Vector2(1f, 0f);
			array2[3] = new Vector2(1f, 1f);
			int[] triangles = new int[]
			{
				0,
				1,
				2,
				3,
				2,
				1
			};
			Vector3[] array3 = new Vector3[array.Length];
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i] = Vector3.forward;
			}
			mesh.vertices = array;
			mesh.uv = array2;
			mesh.triangles = triangles;
			mesh.normals = array3;
			return mesh;
		}

		private Vector2[,] Sample(Texture texture, int width, int height, int step = 4)
		{
			Vector2[,] array = new Vector2[width, height];
			Color[] pixels = this.WindowFriction.GetPixels();
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					float num = (float)j / (float)width;
					float num2 = (float)i / (float)height;
					int num3 = (int)(num * (float)texture.width);
					int num4 = (int)(num2 * (float)texture.height);
					int num5 = num4 * width + num3 - step;
					int num6 = num4 * width + num3 + step;
					int num7 = (num4 + step) * width + num3;
					int num8 = (num4 - step) * width + num3;
					float x = 0f;
					float y = 0f;
					if (WaterRaindropsIME.IsValidTextureIndex(num5, width, height) && WaterRaindropsIME.IsValidTextureIndex(num6, width, height))
					{
						x = pixels[num6].r - pixels[num5].r;
					}
					if (WaterRaindropsIME.IsValidTextureIndex(num7, width, height) && WaterRaindropsIME.IsValidTextureIndex(num8, width, height))
					{
						y = pixels[num7].r - pixels[num8].r;
					}
					array[j, i] = new Vector2(x, y);
				}
			}
			return array;
		}

		private static bool IsValidTextureIndex(int index, int width, int height)
		{
			int num = width * height;
			return index >= 0 && index < num;
		}

		[SerializeField]
		[HideInInspector]
		private Cubemap _CubeMap;

		[Header("Settings")]
		public Vector3 Force = Vector3.down;

		public float VolumeLoss = 0.02f;

		[Range(0.1f, 1f)]
		public float Resolution = 0.5f;

		[Tooltip("Air resistance causing raindrops to slow down")]
		[Range(0f, 1f)]
		[Header("Friction")]
		public float AirFriction = 0.5f;

		[Range(0f, 10f)]
		public float WindowFrictionMultiplier = 0.5f;

		[Tooltip("Adds forces caused by lens imperfections")]
		public Texture2D WindowFriction;

		[Tooltip("How much the water bends light")]
		public WaterRaindropsIME.DistortionModule Distortion;

		[Tooltip("Distorts water paths using custom texture")]
		public WaterRaindropsIME.TwirlModule Twirl;

		[Tooltip("How much time is needed for raindrops to disappear")]
		public WaterRaindropsIME.FadeModule Fade;

		[Tooltip("How much force is applied to raindrops from camera movement")]
		public WaterRaindropsIME.TrackingModule Tracking;

		private RenderTexture _Target;

		private Vector2[,] _Friction;

		private CommandBuffer _Buffer;

		private Mesh _Mesh;

		private Matrix4x4[] _Matrices;

		private readonly List<WaterRaindropsIME.Droplet> _Droplets = new List<WaterRaindropsIME.Droplet>();

		private bool _Initialized;

		private Material _FadeMaterial;

		private Material _DropletMaterial;

		private Material _FinalMaterial;

		private Material _InvertMaterial;

		[Serializable]
		public class FadeModule
		{
			[Tooltip("1 - no fade, 0 - instant fade")]
			[Range(0.5f, 1f)]
			public float Intensity = 0.99f;

			[Tooltip("Additional texture based fade")]
			public Texture2D Texture;

			[Range(0f, 1f)]
			public float Multiplier = 0.1f;
		}

		[Serializable]
		public class DistortionModule
		{
			public float Multiplier = 1f;

			[Range(0.001f, 0.008f)]
			public float NormalSpread = 0.002f;
		}

		[Serializable]
		public class TwirlModule
		{
			public Texture2D Texture;

			[Range(0f, 1f)]
			public float Multiplier = 0.1f;
		}

		[Serializable]
		public class TrackingModule
		{
			public Vector3 Force
			{
				get
				{
					return -this._Velocity;
				}
			}

			internal void Initialize(WaterRaindropsIME reference)
			{
				this._Reference = reference;
				this._PreviousPosition = this._Reference.transform.forward * this._Rotation + this._Reference.transform.position * this._Translation;
			}

			internal void Advance()
			{
				Vector3 vector = this._Reference.transform.forward * this._Rotation + this._Reference.transform.position * this._Translation;
				this._Velocity = this._Reference.transform.worldToLocalMatrix * ((vector - this._PreviousPosition) / Time.fixedDeltaTime);
				this._PreviousPosition = vector;
			}

			[Header("Force multipliers")]
			[SerializeField]
			private float _Translation;

			[SerializeField]
			private float _Rotation;

			private WaterRaindropsIME _Reference;

			private Vector3 _PreviousPosition;

			private Vector3 _Velocity;
		}

		private struct Droplet
		{
			public Vector2 Position;

			public Vector2 Velocity;

			public float Volume;

			public float Life;
		}
	}
}
