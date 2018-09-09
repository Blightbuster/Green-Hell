using System;
using AmplifyColor;
using UnityEngine;

[AddComponentMenu("")]
[ExecuteInEditMode]
public class AmplifyColorVolumeBase : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		if (this.ShowInSceneView)
		{
			BoxCollider component = base.GetComponent<BoxCollider>();
			BoxCollider2D component2 = base.GetComponent<BoxCollider2D>();
			if (component != null || component2 != null)
			{
				Vector3 center;
				Vector3 size;
				if (component != null)
				{
					center = component.center;
					size = component.size;
				}
				else
				{
					center = component2.offset;
					size = component2.size;
				}
				Gizmos.color = Color.green;
				Gizmos.matrix = base.transform.localToWorldMatrix;
				Gizmos.DrawWireCube(center, size);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		BoxCollider component = base.GetComponent<BoxCollider>();
		BoxCollider2D component2 = base.GetComponent<BoxCollider2D>();
		if (component != null || component2 != null)
		{
			Color green = Color.green;
			green.a = 0.2f;
			Gizmos.color = green;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Vector3 center;
			Vector3 size;
			if (component != null)
			{
				center = component.center;
				size = component.size;
			}
			else
			{
				center = component2.offset;
				size = component2.size;
			}
			Gizmos.DrawCube(center, size);
		}
	}

	public Texture2D LutTexture;

	public float Exposure = 1f;

	public float EnterBlendTime = 1f;

	public int Priority;

	public bool ShowInSceneView = true;

	[HideInInspector]
	public VolumeEffectContainer EffectContainer = new VolumeEffectContainer();
}
