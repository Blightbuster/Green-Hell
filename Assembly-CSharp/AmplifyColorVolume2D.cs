using System;
using UnityEngine;

[AddComponentMenu("Image Effects/Amplify Color Volume 2D")]
[RequireComponent(typeof(BoxCollider2D))]
public class AmplifyColorVolume2D : AmplifyColorVolumeBase
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		AmplifyColorTriggerProxy2D component = other.GetComponent<AmplifyColorTriggerProxy2D>();
		if (component != null && component.OwnerEffect.UseVolumes && (component.OwnerEffect.VolumeCollisionMask & 1 << base.gameObject.layer) != 0)
		{
			component.OwnerEffect.EnterVolume(this);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		AmplifyColorTriggerProxy2D component = other.GetComponent<AmplifyColorTriggerProxy2D>();
		if (component != null && component.OwnerEffect.UseVolumes && (component.OwnerEffect.VolumeCollisionMask & 1 << base.gameObject.layer) != 0)
		{
			component.OwnerEffect.ExitVolume(this);
		}
	}
}
