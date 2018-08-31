using System;
using UnityEngine;

public class SceneSplitManager : MonoBehaviour
{
	private void Start()
	{
		this.AddToStreamer();
	}

	private void AddToStreamer()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag(Streamer.STREAMERTAG);
		foreach (GameObject gameObject in array)
		{
			Streamer component = gameObject.GetComponent<Streamer>();
			if (component != null)
			{
				foreach (string text in component.sceneCollection.names)
				{
					if (text.Replace(".unity", string.Empty) == this.sceneName)
					{
						component.AddSceneGO(this.sceneName, base.gameObject);
						return;
					}
				}
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = this.color;
		Gizmos.DrawWireCube(this.position + this.size * 0.5f, this.size);
	}

	public string sceneName;

	public Color color;

	[HideInInspector]
	public Vector3 position;

	[HideInInspector]
	public Vector3 size = new Vector3(10f, 10f, 10f);
}
