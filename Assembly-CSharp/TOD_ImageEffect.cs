using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public abstract class TOD_ImageEffect : MonoBehaviour
{
	protected Material CreateMaterial(Shader shader)
	{
		if (!shader)
		{
			Debug.Log("Missing shader in " + this.ToString());
			base.enabled = false;
			return null;
		}
		if (!shader.isSupported)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"The shader ",
				shader.ToString(),
				" on effect ",
				this.ToString(),
				" is not supported on this platform!"
			}));
			base.enabled = false;
			return null;
		}
		return new Material(shader)
		{
			hideFlags = HideFlags.DontSave
		};
	}

	protected void Awake()
	{
		if (!this.cam)
		{
			this.cam = base.GetComponent<Camera>();
		}
		if (!this.sky)
		{
			this.sky = (UnityEngine.Object.FindObjectOfType(typeof(TOD_Sky)) as TOD_Sky);
		}
	}

	protected bool CheckSupport(bool needDepth = false, bool needHdr = false)
	{
		if (!this.cam)
		{
			return false;
		}
		if (!this.sky || !this.sky.Initialized)
		{
			return false;
		}
		if (!SystemInfo.supportsImageEffects)
		{
			Debug.LogWarning("The image effect " + this.ToString() + " has been disabled as it's not supported on the current platform.");
			base.enabled = false;
			return false;
		}
		if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			Debug.LogWarning("The image effect " + this.ToString() + " has been disabled as it requires a depth texture.");
			base.enabled = false;
			return false;
		}
		if (needHdr && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			Debug.LogWarning("The image effect " + this.ToString() + " has been disabled as it requires HDR.");
			base.enabled = false;
			return false;
		}
		if (needDepth)
		{
			this.cam.depthTextureMode |= DepthTextureMode.Depth;
		}
		if (needHdr)
		{
			this.cam.hdr = true;
		}
		return true;
	}

	protected void DrawBorder(RenderTexture dest, Material material)
	{
		RenderTexture.active = dest;
		bool flag = true;
		GL.PushMatrix();
		GL.LoadOrtho();
		for (int i = 0; i < material.passCount; i++)
		{
			material.SetPass(i);
			float y;
			float y2;
			if (flag)
			{
				y = 1f;
				y2 = 0f;
			}
			else
			{
				y = 0f;
				y2 = 1f;
			}
			float x = 0f;
			float x2 = 1f / ((float)dest.width * 1f);
			float y3 = 0f;
			float y4 = 1f;
			GL.Begin(7);
			GL.TexCoord2(0f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x2, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x2, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(x, y4, 0.1f);
			x = 1f - 1f / ((float)dest.width * 1f);
			x2 = 1f;
			y3 = 0f;
			y4 = 1f;
			GL.TexCoord2(0f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x2, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x2, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(x, y4, 0.1f);
			x = 0f;
			x2 = 1f;
			y3 = 0f;
			y4 = 1f / ((float)dest.height * 1f);
			GL.TexCoord2(0f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x2, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x2, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(x, y4, 0.1f);
			x = 0f;
			x2 = 1f;
			y3 = 1f - 1f / ((float)dest.height * 1f);
			y4 = 1f;
			GL.TexCoord2(0f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x2, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x2, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.End();
		}
		GL.PopMatrix();
	}

	protected void CustomBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr = 0)
	{
		RenderTexture.active = dest;
		fxMaterial.SetTexture("_MainTex", source);
		GL.PushMatrix();
		GL.LoadOrtho();
		fxMaterial.SetPass(passNr);
		GL.Begin(7);
		GL.MultiTexCoord2(0, 0f, 0f);
		GL.Vertex3(0f, 0f, 3f);
		GL.MultiTexCoord2(0, 1f, 0f);
		GL.Vertex3(1f, 0f, 2f);
		GL.MultiTexCoord2(0, 1f, 1f);
		GL.Vertex3(1f, 1f, 1f);
		GL.MultiTexCoord2(0, 0f, 1f);
		GL.Vertex3(0f, 1f, 0f);
		GL.End();
		GL.PopMatrix();
	}

	protected Matrix4x4 FrustumCorners()
	{
		float nearClipPlane = this.cam.nearClipPlane;
		float farClipPlane = this.cam.farClipPlane;
		float fieldOfView = this.cam.fieldOfView;
		float aspect = this.cam.aspect;
		Vector3 forward = this.cam.transform.forward;
		Vector3 right = this.cam.transform.right;
		Vector3 up = this.cam.transform.up;
		Matrix4x4 identity = Matrix4x4.identity;
		float num = fieldOfView * 0.5f;
		Vector3 b = right * nearClipPlane * Mathf.Tan(num * 0.0174532924f) * aspect;
		Vector3 b2 = up * nearClipPlane * Mathf.Tan(num * 0.0174532924f);
		Vector3 vector = forward * nearClipPlane - b + b2;
		float d = vector.magnitude * farClipPlane / nearClipPlane;
		vector.Normalize();
		vector *= d;
		Vector3 vector2 = forward * nearClipPlane + b + b2;
		vector2.Normalize();
		vector2 *= d;
		Vector3 vector3 = forward * nearClipPlane + b - b2;
		vector3.Normalize();
		vector3 *= d;
		Vector3 vector4 = forward * nearClipPlane - b - b2;
		vector4.Normalize();
		vector4 *= d;
		identity.SetRow(0, vector);
		identity.SetRow(1, vector2);
		identity.SetRow(2, vector3);
		identity.SetRow(3, vector4);
		return identity;
	}

	public TOD_Sky sky;

	protected Camera cam;
}
