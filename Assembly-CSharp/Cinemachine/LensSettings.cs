using System;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(2f, DocumentationSortingAttribute.Level.UserRef)]
	[Serializable]
	public struct LensSettings
	{
		public LensSettings(float fov, float orthographicSize, float nearClip, float farClip, float dutch, bool ortho, float aspect)
		{
			this = default(LensSettings);
			this.FieldOfView = fov;
			this.OrthographicSize = orthographicSize;
			this.NearClipPlane = nearClip;
			this.FarClipPlane = farClip;
			this.Dutch = dutch;
			this.Orthographic = ortho;
			this.Aspect = aspect;
		}

		internal bool Orthographic { get; set; }

		internal float Aspect { get; set; }

		public static LensSettings FromCamera(Camera fromCamera)
		{
			LensSettings @default = LensSettings.Default;
			if (fromCamera != null)
			{
				@default.FieldOfView = fromCamera.fieldOfView;
				@default.OrthographicSize = fromCamera.orthographicSize;
				@default.NearClipPlane = fromCamera.nearClipPlane;
				@default.FarClipPlane = fromCamera.farClipPlane;
				@default.Orthographic = fromCamera.orthographic;
				@default.Aspect = fromCamera.aspect;
			}
			return @default;
		}

		public static LensSettings Lerp(LensSettings lensA, LensSettings lensB, float t)
		{
			t = Mathf.Clamp01(t);
			return new LensSettings
			{
				FarClipPlane = Mathf.Lerp(lensA.FarClipPlane, lensB.FarClipPlane, t),
				NearClipPlane = Mathf.Lerp(lensA.NearClipPlane, lensB.NearClipPlane, t),
				FieldOfView = Mathf.Lerp(lensA.FieldOfView, lensB.FieldOfView, t),
				OrthographicSize = Mathf.Lerp(lensA.OrthographicSize, lensB.OrthographicSize, t),
				Dutch = Mathf.Lerp(lensA.Dutch, lensB.Dutch, t),
				Aspect = Mathf.Lerp(lensA.Aspect, lensB.Aspect, t),
				Orthographic = (lensA.Orthographic && lensB.Orthographic)
			};
		}

		public void Validate()
		{
			this.NearClipPlane = Mathf.Max(this.NearClipPlane, 0.01f);
			this.FarClipPlane = Mathf.Max(this.FarClipPlane, this.NearClipPlane + 0.01f);
		}

		public static LensSettings Default = new LensSettings(40f, 10f, 0.1f, 5000f, 0f, false, 1f);

		[Range(1f, 179f)]
		[Tooltip("This is the camera view in vertical degrees. For cinematic people, a 50mm lens on a super-35mm sensor would equal a 19.6 degree FOV")]
		public float FieldOfView;

		[Tooltip("When using an orthographic camera, this defines the half-height, in world coordinates, of the camera view.")]
		public float OrthographicSize;

		[Tooltip("This defines the near region in the renderable range of the camera frustum. Raising this value will stop the game from drawing things near the camera, which can sometimes come in handy.  Larger values will also increase your shadow resolution.")]
		public float NearClipPlane;

		[Tooltip("This defines the far region of the renderable range of the camera frustum. Typically you want to set this value as low as possible without cutting off desired distant objects")]
		public float FarClipPlane;

		[Tooltip("Camera Z roll, or tilt, in degrees.")]
		[Range(-180f, 180f)]
		public float Dutch;
	}
}
