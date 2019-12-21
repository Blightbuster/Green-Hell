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
		for (int i = 0; i < array.Length; i++)
		{
			Streamer component = array[i].GetComponent<Streamer>();
			if (component != null)
			{
				string[] names = component.sceneCollection.names;
				for (int j = 0; j < names.Length; j++)
				{
					if (names[j].Replace(".unity", "") == this.sceneName)
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
