using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("Relief Terrain/Helpers/Deferred Params")]
[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class RTP_DeferredParams : MonoBehaviour
{
	public void OnEnable()
	{
		if (this.NotifyDecals())
		{
			return;
		}
		if (this.mycam == null)
		{
			this.mycam = base.GetComponent<Camera>();
			if (this.mycam == null)
			{
				return;
			}
		}
		this.Initialize();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(this.SetupCam));
	}

	public void OnDisable()
	{
		this.NotifyDecals();
		this.Cleanup();
	}

	public void OnDestroy()
	{
		this.NotifyDecals();
		this.Cleanup();
	}

	private bool NotifyDecals()
	{
		Type type = Type.GetType("UBERDecalSystem.DecalManager");
		if (type != null)
		{
			bool flag = UnityEngine.Object.FindObjectOfType(type) != null && UnityEngine.Object.FindObjectOfType(type) is MonoBehaviour && (UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).enabled;
			if (flag)
			{
				(UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).Invoke("OnDisable", 0f);
				(UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).Invoke("OnEnable", 0f);
				return true;
			}
		}
		return false;
	}

	private void Cleanup()
	{
		if (this.combufPreLight != null)
		{
			if (this.mycam)
			{
				this.mycam.RemoveCommandBuffer(CameraEvent.BeforeReflections, this.combufPreLight);
				this.mycam.RemoveCommandBuffer(CameraEvent.AfterLighting, this.combufPostLight);
			}
			foreach (Camera camera in this.sceneCamsWithBuffer)
			{
				if (camera)
				{
					camera.RemoveCommandBuffer(CameraEvent.BeforeReflections, this.combufPreLight);
					camera.RemoveCommandBuffer(CameraEvent.AfterLighting, this.combufPostLight);
				}
			}
		}
		this.sceneCamsWithBuffer.Clear();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(this.SetupCam));
	}

	private void SetupCam(Camera cam)
	{
		bool flag = false;
		if (cam == this.mycam || flag)
		{
			this.RefreshComBufs(cam, flag);
		}
	}

	public void RefreshComBufs(Camera cam, bool isSceneCam)
	{
		if (cam && this.combufPreLight != null && this.combufPostLight != null)
		{
			CommandBuffer[] commandBuffers = cam.GetCommandBuffers(CameraEvent.BeforeReflections);
			bool flag = false;
			foreach (CommandBuffer commandBuffer in commandBuffers)
			{
				if (commandBuffer.name == this.combufPreLight.name)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				cam.AddCommandBuffer(CameraEvent.BeforeReflections, this.combufPreLight);
				cam.AddCommandBuffer(CameraEvent.AfterLighting, this.combufPostLight);
				if (isSceneCam)
				{
					this.sceneCamsWithBuffer.Add(cam);
				}
			}
		}
	}

	public void Initialize()
	{
		if (this.combufPreLight == null)
		{
			int nameID = Shader.PropertyToID("_UBERPropsBuffer");
			if (this.CopyPropsMat == null)
			{
				if (this.CopyPropsMat != null)
				{
					UnityEngine.Object.DestroyImmediate(this.CopyPropsMat);
				}
				this.CopyPropsMat = new Material(Shader.Find("Hidden/UBER_CopyPropsTexture"));
				this.CopyPropsMat.hideFlags = HideFlags.DontSave;
			}
			this.combufPreLight = new CommandBuffer();
			this.combufPreLight.name = "UBERPropsPrelight";
			this.combufPreLight.GetTemporaryRT(nameID, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RHalf);
			this.combufPreLight.Blit(BuiltinRenderTextureType.CameraTarget, nameID, this.CopyPropsMat);
			this.combufPostLight = new CommandBuffer();
			this.combufPostLight.name = "UBERPropsPostlight";
			this.combufPostLight.ReleaseTemporaryRT(nameID);
		}
	}

	private Camera mycam;

	private CommandBuffer combufPreLight;

	private CommandBuffer combufPostLight;

	public Material CopyPropsMat;

	private HashSet<Camera> sceneCamsWithBuffer = new HashSet<Camera>();
}
