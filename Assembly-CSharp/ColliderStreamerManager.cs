using System;
using System.Collections.Generic;
using UnityEngine;

public class ColliderStreamerManager : MonoBehaviour
{
	public void AddColliderStreamer(ColliderStreamer colliderStreamer)
	{
		this.colliderStreamers.Add(colliderStreamer);
	}

	public void AddColliderScene(ColliderScene colliderScene)
	{
		foreach (ColliderStreamer colliderStreamer in this.colliderStreamers)
		{
			if (colliderStreamer != null && colliderStreamer.sceneName == colliderScene.sceneName)
			{
				colliderStreamer.SetSceneGameObject(colliderScene.gameObject);
				break;
			}
		}
	}

	public void Update()
	{
		if (this.spawnedPlayer && this.player == null && !string.IsNullOrEmpty(this.playerTag))
		{
			GameObject gameObject = GameObject.FindGameObjectWithTag(this.playerTag);
			if (gameObject != null)
			{
				this.player = gameObject.transform;
			}
		}
	}

	[Tooltip("Object that will start loading process after it hits the collider.")]
	public Transform player;

	[Tooltip("Collider Streamer Manager will wait for player spawn and fill it automatically")]
	public bool spawnedPlayer;

	[HideInInspector]
	public string playerTag = "Player";

	public static string COLLIDERSTREAMERMANAGERTAG = "ColliderStreamerManager";

	[HideInInspector]
	public List<ColliderStreamer> colliderStreamers;
}
