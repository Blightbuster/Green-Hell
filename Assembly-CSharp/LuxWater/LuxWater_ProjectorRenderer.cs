using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater
{
	[RequireComponent(typeof(Camera))]
	[ExecuteInEditMode]
	public class LuxWater_ProjectorRenderer : MonoBehaviour
	{
		private void OnEnable()
		{
			this._LuxWater_FoamOverlayPID = Shader.PropertyToID("_LuxWater_FoamOverlay");
			this._LuxWater_NormalOverlayPID = Shader.PropertyToID("_LuxWater_NormalOverlay");
			LuxWater_ProjectorRenderer.cb_Foam = new CommandBuffer();
			LuxWater_ProjectorRenderer.cb_Foam.name = "Lux Water: Foam Overlay Buffer";
			LuxWater_ProjectorRenderer.cb_Normals = new CommandBuffer();
			LuxWater_ProjectorRenderer.cb_Normals.name = "Lux Water: Normal Overlay Buffer";
		}

		private void OnDisable()
		{
			if (this.ProjectedFoam != null)
			{
				UnityEngine.Object.DestroyImmediate(this.ProjectedFoam);
			}
			if (this.ProjectedNormals != null)
			{
				UnityEngine.Object.DestroyImmediate(this.ProjectedNormals);
			}
			if (this.defaultBump != null)
			{
				UnityEngine.Object.DestroyImmediate(this.defaultBump);
			}
			if (this.DebugMat != null)
			{
				UnityEngine.Object.DestroyImmediate(this.DebugMat);
			}
			if (LuxWater_ProjectorRenderer.cb_Foam != null)
			{
				LuxWater_ProjectorRenderer.cb_Foam.Clear();
				LuxWater_ProjectorRenderer.cb_Foam.Dispose();
			}
			if (LuxWater_ProjectorRenderer.cb_Normals != null)
			{
				LuxWater_ProjectorRenderer.cb_Normals.Clear();
				LuxWater_ProjectorRenderer.cb_Normals.Dispose();
			}
			Shader.DisableKeyword("USINGWATERPROJECTORS");
		}

		private void OnPreRender()
		{
			this.cam = base.GetComponent<Camera>();
			int count = LuxWater_Projector.FoamProjectors.Count;
			int count2 = LuxWater_Projector.NormalProjectors.Count;
			if (count + count2 == 0)
			{
				if (LuxWater_ProjectorRenderer.cb_Foam != null)
				{
					LuxWater_ProjectorRenderer.cb_Foam.Clear();
				}
				if (LuxWater_ProjectorRenderer.cb_Normals != null)
				{
					LuxWater_ProjectorRenderer.cb_Normals.Clear();
				}
				Shader.DisableKeyword("USINGWATERPROJECTORS");
				return;
			}
			Shader.EnableKeyword("USINGWATERPROJECTORS");
			Matrix4x4 projectionMatrix = this.cam.projectionMatrix;
			Matrix4x4 worldToCameraMatrix = this.cam.worldToCameraMatrix;
			Matrix4x4 worldToProjectMatrix = projectionMatrix * worldToCameraMatrix;
			int pixelWidth = this.cam.pixelWidth;
			int pixelHeight = this.cam.pixelHeight;
			GeomUtil.CalculateFrustumPlanes(this.frustumPlanes, worldToProjectMatrix);
			int num = Mathf.FloorToInt((float)(pixelWidth / (int)this.FoamBufferResolution));
			int height = Mathf.FloorToInt((float)(pixelHeight / (int)this.FoamBufferResolution));
			if (!this.ProjectedFoam)
			{
				this.ProjectedFoam = new RenderTexture(num, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
				Shader.SetGlobalTexture(this._LuxWater_FoamOverlayPID, this.ProjectedFoam);
			}
			else if (this.ProjectedFoam.width != num)
			{
				this.ProjectedFoam = new RenderTexture(num, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			}
			GL.PushMatrix();
			GL.modelview = worldToCameraMatrix;
			GL.LoadProjectionMatrix(projectionMatrix);
			LuxWater_ProjectorRenderer.cb_Foam.Clear();
			LuxWater_ProjectorRenderer.cb_Foam.SetRenderTarget(this.ProjectedFoam);
			LuxWater_ProjectorRenderer.cb_Foam.ClearRenderTarget(true, true, new Color(0f, 0f, 0f, 0f), 1f);
			this.drawnFoamProjectors = 0;
			for (int i = 0; i < count; i++)
			{
				LuxWater_Projector luxWater_Projector = LuxWater_Projector.FoamProjectors[i];
				this.tempBounds = luxWater_Projector.m_Rend.bounds;
				if (GeometryUtility.TestPlanesAABB(this.frustumPlanes, this.tempBounds))
				{
					LuxWater_ProjectorRenderer.cb_Foam.DrawRenderer(luxWater_Projector.m_Rend, luxWater_Projector.m_Mat);
					this.drawnFoamProjectors++;
				}
			}
			Graphics.ExecuteCommandBuffer(LuxWater_ProjectorRenderer.cb_Foam);
			num = Mathf.FloorToInt((float)(pixelWidth / (int)this.NormalBufferResolution));
			height = Mathf.FloorToInt((float)(pixelHeight / (int)this.NormalBufferResolution));
			if (!this.ProjectedNormals)
			{
				this.ProjectedNormals = new RenderTexture(num, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
				Shader.SetGlobalTexture(this._LuxWater_NormalOverlayPID, this.ProjectedNormals);
			}
			else if (this.ProjectedNormals.width != num)
			{
				this.ProjectedNormals = new RenderTexture(num, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			}
			LuxWater_ProjectorRenderer.cb_Normals.Clear();
			LuxWater_ProjectorRenderer.cb_Normals.SetRenderTarget(this.ProjectedNormals);
			LuxWater_ProjectorRenderer.cb_Normals.ClearRenderTarget(true, true, new Color(0f, 0f, 1f, 0f), 1f);
			this.drawnNormalProjectors = 0;
			for (int j = 0; j < count2; j++)
			{
				LuxWater_Projector luxWater_Projector2 = LuxWater_Projector.NormalProjectors[j];
				this.tempBounds = luxWater_Projector2.m_Rend.bounds;
				if (GeometryUtility.TestPlanesAABB(this.frustumPlanes, this.tempBounds))
				{
					LuxWater_ProjectorRenderer.cb_Normals.DrawRenderer(luxWater_Projector2.m_Rend, luxWater_Projector2.m_Mat);
					this.drawnNormalProjectors++;
				}
			}
			Graphics.ExecuteCommandBuffer(LuxWater_ProjectorRenderer.cb_Normals);
			GL.PopMatrix();
		}

		private void OnDrawGizmos()
		{
			Camera component = base.GetComponent<Camera>();
			int num = 0;
			int num2 = (int)(component.aspect * 128f);
			if (this.DebugMat == null)
			{
				this.DebugMat = new Material(Shader.Find("Hidden/LuxWater_Debug"));
			}
			if (this.DebugNormalMat == null)
			{
				this.DebugNormalMat = new Material(Shader.Find("Hidden/LuxWater_DebugNormals"));
			}
			if (this.DebugFoamBuffer)
			{
				if (this.ProjectedFoam == null)
				{
					return;
				}
				GL.PushMatrix();
				GL.LoadPixelMatrix(0f, (float)Screen.width, (float)Screen.height, 0f);
				Graphics.DrawTexture(new Rect((float)num, 0f, (float)num2, 128f), this.ProjectedFoam, this.DebugMat);
				GL.PopMatrix();
				num = num2;
			}
			if (this.DebugNormalBuffer)
			{
				if (this.ProjectedNormals == null)
				{
					return;
				}
				GL.PushMatrix();
				GL.LoadPixelMatrix(0f, (float)Screen.width, (float)Screen.height, 0f);
				Graphics.DrawTexture(new Rect((float)num, 0f, (float)num2, 128f), this.ProjectedNormals, this.DebugNormalMat);
				GL.PopMatrix();
			}
		}

		[Space(8f)]
		public LuxWater_ProjectorRenderer.BufferResolution FoamBufferResolution = LuxWater_ProjectorRenderer.BufferResolution.Full;

		public LuxWater_ProjectorRenderer.BufferResolution NormalBufferResolution = LuxWater_ProjectorRenderer.BufferResolution.Full;

		[Space(2f)]
		[Header("Debug")]
		[Space(4f)]
		public bool DebugFoamBuffer;

		public bool DebugNormalBuffer;

		public bool DebugStats;

		private int drawnFoamProjectors;

		private int drawnNormalProjectors;

		private static CommandBuffer cb_Foam;

		private static CommandBuffer cb_Normals;

		private Camera cam;

		private Transform camTransform;

		private RenderTexture ProjectedFoam;

		private RenderTexture ProjectedNormals;

		private Texture2D defaultBump;

		private Bounds tempBounds;

		private int _LuxWater_FoamOverlayPID;

		private int _LuxWater_NormalOverlayPID;

		private Plane[] frustumPlanes = new Plane[6];

		private Material DebugMat;

		private Material DebugNormalMat;

		public enum BufferResolution
		{
			Full = 1,
			Half,
			Quarter = 4,
			Eighth = 8
		}
	}
}
