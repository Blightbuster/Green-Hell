using System;
using UnityEngine;

namespace VolumetricFogAndMist
{
	public class FogVolume : MonoBehaviour
	{
		private void Start()
		{
			if (this.targetFog == null)
			{
				this.targetFog = VolumetricFog.instance;
			}
			if (this.targetFog != null)
			{
				this.targetFog.useFogVolumes = true;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (this.cameraInside || this.targetFog == null)
			{
				return;
			}
			if (other == this.targetCollider || other.gameObject.transform.GetComponentInChildren<Camera>() == this.targetFog.fogCamera)
			{
				this.cameraInside = true;
				if (this.enableProfileTransition && this.targetProfile != null)
				{
					this.targetFog.SetTargetProfile(this.targetProfile, this.transitionDuration);
				}
				if (this.enableAlphaTransition)
				{
					this.targetFog.SetTargetAlpha(this.targetFogAlpha, this.targetSkyHazeAlpha, this.transitionDuration);
				}
				if (this.enableFogColorTransition)
				{
					this.targetFog.SetTargetColor(this.targetFogColor, this.transitionDuration);
				}
				if (this.enableFogSpecularColorTransition)
				{
					this.targetFog.SetTargetSpecularColor(this.targetFogSpecularColor, this.transitionDuration);
				}
				if (this.enableLightColorTransition)
				{
					this.targetFog.SetTargetLightColor(this.targetLightColor, this.transitionDuration);
				}
				if (this.debugMode)
				{
					Debug.Log("Fog Volume entered by " + other.name);
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (!this.cameraInside || this.targetFog == null)
			{
				return;
			}
			if (other == this.targetCollider || other.gameObject.transform.GetComponentInChildren<Camera>() == this.targetFog.fogCamera)
			{
				this.cameraInside = false;
				if (this.enableProfileTransition && this.targetProfile != null)
				{
					this.targetFog.ClearTargetProfile(this.transitionDuration);
				}
				if (this.enableAlphaTransition)
				{
					this.targetFog.ClearTargetAlpha(this.transitionDuration);
				}
				if (this.enableFogColorTransition)
				{
					this.targetFog.ClearTargetColor(this.transitionDuration);
				}
				if (this.enableFogSpecularColorTransition)
				{
					this.targetFog.ClearTargetSpecularColor(this.transitionDuration);
				}
				if (this.enableLightColorTransition)
				{
					this.targetFog.ClearTargetLightColor(this.transitionDuration);
				}
				if (this.debugMode)
				{
					Debug.Log("Fog Volume exited by " + other.name);
				}
			}
		}

		private const float GRAY = 0.8901961f;

		[Tooltip("Enables transition to a given profile.")]
		public bool enableProfileTransition;

		[Tooltip("Assign the transition profile.")]
		public VolumetricFogProfile targetProfile;

		[Tooltip("Enables alpha transition.")]
		public bool enableAlphaTransition;

		[Range(0f, 1f)]
		[Tooltip("Target alpha for fog when camera enters this fog volume")]
		public float targetFogAlpha = 0.5f;

		[Tooltip("Target alpha for sky haze when camera enters this fog volume")]
		[Range(0f, 1f)]
		public float targetSkyHazeAlpha = 0.5f;

		[Tooltip("Enables fog color transition.")]
		public bool enableFogColorTransition;

		[Tooltip("Target fog color 1 when gamera enters this fog folume")]
		public Color targetFogColor = new Color(0.8901961f, 0.8901961f, 0.8901961f);

		[Tooltip("Enables fog specular color transition.")]
		public bool enableFogSpecularColorTransition;

		[Tooltip("Target fog color 2 when gamera enters this fog folume")]
		public Color targetFogSpecularColor = new Color(0.8901961f, 0.8901961f, 0.8901961f);

		[Tooltip("Enables light color transition.")]
		public bool enableLightColorTransition;

		[Tooltip("Target light color when gamera enters this fog folume")]
		public Color targetLightColor = Color.white;

		[Tooltip("Set this to zero for changing fog alpha immediately upon enter/exit fog volume.")]
		public float transitionDuration = 3f;

		[Tooltip("Set collider that will trigger this fog volume. If not set, this fog volume will react to any collider which has the main camera. If you use a third person controller, assign the character collider here.")]
		public Collider targetCollider;

		[Tooltip("When enabled, a console message will be printed whenever this fog volume is entered or exited.")]
		public bool debugMode;

		[Tooltip("Assign target Volumetric Fog component that will be affected by this volume.")]
		public VolumetricFog targetFog;

		private bool cameraInside;
	}
}
