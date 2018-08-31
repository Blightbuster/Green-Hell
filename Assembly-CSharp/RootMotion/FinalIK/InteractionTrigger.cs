using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Trigger")]
	[HelpURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
	public class InteractionTrigger : MonoBehaviour
	{
		[ContextMenu("TUTORIAL VIDEO")]
		private void OpenTutorial4()
		{
			Application.OpenURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		[ContextMenu("Support Group")]
		private void SupportGroup()
		{
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}

		[ContextMenu("Asset Store Thread")]
		private void ASThread()
		{
			Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		private void Start()
		{
		}

		public int GetBestRangeIndex(Transform character, Transform raycastFrom, RaycastHit raycastHit)
		{
			if (base.GetComponent<Collider>() == null)
			{
				Warning.Log("Using the InteractionTrigger requires a Collider component.", base.transform, false);
				return -1;
			}
			int result = -1;
			float num = 180f;
			float num2 = 0f;
			for (int i = 0; i < this.ranges.Length; i++)
			{
				if (this.ranges[i].IsInRange(character, raycastFrom, raycastHit, base.transform, out num2) && num2 <= num)
				{
					num = num2;
					result = i;
				}
			}
			return result;
		}

		[Tooltip("The valid ranges of the character's and/or it's camera's position for triggering interaction when the character is in contact with the collider of this trigger.")]
		public InteractionTrigger.Range[] ranges = new InteractionTrigger.Range[0];

		[Serializable]
		public class CharacterPosition
		{
			public Vector3 offset3D
			{
				get
				{
					return new Vector3(this.offset.x, 0f, this.offset.y);
				}
			}

			public Vector3 direction3D
			{
				get
				{
					return Quaternion.AngleAxis(this.angleOffset, Vector3.up) * Vector3.forward;
				}
			}

			public bool IsInRange(Transform character, Transform trigger, out float error)
			{
				error = 0f;
				if (!this.use)
				{
					return true;
				}
				error = 180f;
				if (this.radius <= 0f)
				{
					return false;
				}
				if (this.maxAngle <= 0f)
				{
					return false;
				}
				Vector3 forward = trigger.forward;
				if (this.fixYAxis)
				{
					forward.y = 0f;
				}
				if (forward == Vector3.zero)
				{
					return false;
				}
				Vector3 vector = (!this.fixYAxis) ? trigger.up : Vector3.up;
				Quaternion rotation = Quaternion.LookRotation(forward, vector);
				Vector3 vector2 = trigger.position + rotation * this.offset3D;
				Vector3 b = (!this.orbit) ? vector2 : trigger.position;
				Vector3 vector3 = character.position - b;
				Vector3.OrthoNormalize(ref vector, ref vector3);
				vector3 *= Vector3.Project(character.position - b, vector3).magnitude;
				if (this.orbit)
				{
					float magnitude = this.offset.magnitude;
					float magnitude2 = vector3.magnitude;
					if (magnitude2 < magnitude - this.radius || magnitude2 > magnitude + this.radius)
					{
						return false;
					}
				}
				else if (vector3.magnitude > this.radius)
				{
					return false;
				}
				Vector3 vector4 = rotation * this.direction3D;
				Vector3.OrthoNormalize(ref vector, ref vector4);
				if (this.orbit)
				{
					Vector3 vector5 = vector2 - trigger.position;
					if (vector5 == Vector3.zero)
					{
						vector5 = Vector3.forward;
					}
					Quaternion rotation2 = Quaternion.LookRotation(vector5, vector);
					vector3 = Quaternion.Inverse(rotation2) * vector3;
					float angle = Mathf.Atan2(vector3.x, vector3.z) * 57.29578f;
					vector4 = Quaternion.AngleAxis(angle, vector) * vector4;
				}
				float num = Vector3.Angle(vector4, character.forward);
				if (num > this.maxAngle)
				{
					return false;
				}
				error = num / this.maxAngle * 180f;
				return true;
			}

			[Tooltip("If false, will not care where the character stands, as long as it is in contact with the trigger collider.")]
			public bool use;

			[Tooltip("The offset of the character's position relative to the trigger in XZ plane. Y position of the character is unlimited as long as it is contact with the collider.")]
			public Vector2 offset;

			[Tooltip("Angle offset from the default forward direction.")]
			[Range(-180f, 180f)]
			public float angleOffset;

			[Range(0f, 180f)]
			[Tooltip("Max angular offset of the character's forward from the direction of this trigger.")]
			public float maxAngle = 45f;

			[Tooltip("Max offset of the character's position from this range's center.")]
			public float radius = 0.5f;

			[Tooltip("If true, will rotate the trigger around it's Y axis relative to the position of the character, so the object can be interacted with from all sides.")]
			public bool orbit;

			[Tooltip("Fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object. For example a gun will be able to be picked up from the same direction relative to the barrel no matter which side the gun is resting on.")]
			public bool fixYAxis;
		}

		[Serializable]
		public class CameraPosition
		{
			public Quaternion GetRotation()
			{
				Vector3 forward = this.lookAtTarget.transform.forward;
				if (this.fixYAxis)
				{
					forward.y = 0f;
				}
				if (forward == Vector3.zero)
				{
					return Quaternion.identity;
				}
				Vector3 upwards = (!this.fixYAxis) ? this.lookAtTarget.transform.up : Vector3.up;
				return Quaternion.LookRotation(forward, upwards);
			}

			public bool IsInRange(Transform raycastFrom, RaycastHit hit, Transform trigger, out float error)
			{
				error = 0f;
				if (this.lookAtTarget == null)
				{
					return true;
				}
				error = 180f;
				if (raycastFrom == null)
				{
					return false;
				}
				if (hit.collider != this.lookAtTarget)
				{
					return false;
				}
				if (hit.distance > this.maxDistance)
				{
					return false;
				}
				if (this.direction == Vector3.zero)
				{
					return false;
				}
				if (this.maxDistance <= 0f)
				{
					return false;
				}
				if (this.maxAngle <= 0f)
				{
					return false;
				}
				Vector3 to = this.GetRotation() * this.direction;
				float num = Vector3.Angle(raycastFrom.position - hit.point, to);
				if (num > this.maxAngle)
				{
					return false;
				}
				error = num / this.maxAngle * 180f;
				return true;
			}

			[Tooltip("What the camera should be looking at to trigger the interaction?")]
			public Collider lookAtTarget;

			[Tooltip("The direction from the lookAtTarget towards the camera (in lookAtTarget's space).")]
			public Vector3 direction = -Vector3.forward;

			[Tooltip("Max distance from the lookAtTarget to the camera.")]
			public float maxDistance = 0.5f;

			[Range(0f, 180f)]
			[Tooltip("Max angle between the direction and the direction towards the camera.")]
			public float maxAngle = 45f;

			[Tooltip("Fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object.")]
			public bool fixYAxis;
		}

		[Serializable]
		public class Range
		{
			public bool IsInRange(Transform character, Transform raycastFrom, RaycastHit raycastHit, Transform trigger, out float maxError)
			{
				maxError = 0f;
				float a = 0f;
				float b = 0f;
				if (!this.characterPosition.IsInRange(character, trigger, out a))
				{
					return false;
				}
				if (!this.cameraPosition.IsInRange(raycastFrom, raycastHit, trigger, out b))
				{
					return false;
				}
				maxError = Mathf.Max(a, b);
				return true;
			}

			[SerializeField]
			[HideInInspector]
			public string name;

			[SerializeField]
			[HideInInspector]
			public bool show = true;

			[Tooltip("The range for the character's position and rotation.")]
			public InteractionTrigger.CharacterPosition characterPosition;

			[Tooltip("The range for the character camera's position and rotation.")]
			public InteractionTrigger.CameraPosition cameraPosition;

			[Tooltip("Definitions of the interactions associated with this range.")]
			public InteractionTrigger.Range.Interaction[] interactions;

			[Serializable]
			public class Interaction
			{
				[Tooltip("The InteractionObject to interact with.")]
				public InteractionObject interactionObject;

				[Tooltip("The effectors to interact with.")]
				public FullBodyBipedEffector[] effectors;
			}
		}
	}
}
