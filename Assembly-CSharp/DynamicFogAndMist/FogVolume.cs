using System;
using UnityEngine;

namespace DynamicFogAndMist
{
	public class FogVolume : MonoBehaviour
	{
		private void Start()
		{
			if (this.targetFog == null)
			{
				this.targetFog = DynamicFog.instance;
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
					this.targetFog.SetTargetColors(this.targetFogColor1, this.targetFogColor2, this.transitionDuration);
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
					this.targetFog.ClearTargetColors(this.transitionDuration);
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
		public DynamicFogProfile targetProfile;

		[Tooltip("Enables alpha transition.")]
		public bool enableAlphaTransition;

		[Tooltip("Target alpha for fog when camera enters this fog volume")]
		[Range(0f, 1f)]
		public float targetFogAlpha = 0.5f;

		[Tooltip("Target alpha for sky haze when camera enters this fog volume")]
		[Range(0f, 1f)]
		public float targetSkyHazeAlpha = 0.5f;

		[Tooltip("Enables fog color transition.")]
		public bool enableFogColorTransition;

		[Tooltip("Target fog color 1 when gamera enters this fog folume")]
		public Color targetFogColor1 = new Color(0.8901961f, 0.8901961f, 0.8901961f);

		[Tooltip("Target fog color 2 when gamera enters this fog folume")]
		public Color targetFogColor2 = new Color(0.8901961f, 0.8901961f, 0.8901961f);

		[Tooltip("Set this to zero for changing fog alpha immediately upon enter/exit fog volume.")]
		public float transitionDuration = 3f;

		[Tooltip("Set collider that will trigger this fog volume. If not set, this fog volume will react to any collider which has the main camera. If you use a third person controller, assign the character collider here.")]
		public Collider targetCollider;

		[Tooltip("When enabled, a console message will be printed whenever this fog volume is entered or exited.")]
		public bool debugMode;

		[Tooltip("Assign target Dynamic Fog component that will be affected by this volume.")]
		public DynamicFog targetFog;

		private bool cameraInside;
	}
}
