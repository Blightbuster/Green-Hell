﻿using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMover : MonoBehaviour
{
	private void Awake()
	{
		if (this.streamers.Length != 0)
		{
			if (!this.streamers[0].spawnedPlayer)
			{
				this.MovePlayer();
				return;
			}
			this.waitForPlayer = true;
		}
	}

	private void Update()
	{
		if (this.waitForPlayer)
		{
			if (this.player == null && !string.IsNullOrEmpty(this.streamers[0].playerTag))
			{
				GameObject gameObject = GameObject.FindGameObjectWithTag(this.streamers[0].playerTag);
				if (gameObject != null)
				{
					this.player = gameObject.transform;
					this.MovePlayer();
					this.waitForPlayer = false;
					return;
				}
			}
		}
		else if (!this.playerMoved)
		{
			if (this.streamers.Length != 0)
			{
				bool flag = true;
				this.progress = 0f;
				foreach (Streamer streamer in this.streamers)
				{
					this.progress += streamer.LoadingProgress / (float)this.streamers.Length;
					flag = (flag && streamer.initialized);
				}
				if (flag && this.progress >= 1f)
				{
					if (this.onDone != null)
					{
						this.onDone.Invoke();
					}
					this.Done();
					return;
				}
			}
			else
			{
				Debug.Log("No streamer Attached");
			}
		}
	}

	public void Done()
	{
		this.player.position = this.temporaryObject.transform.position;
		this.player.rotation = this.temporaryObject.transform.rotation;
		Streamer[] array = this.streamers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].player = this.player;
		}
		UnityEngine.Object.Destroy(this.temporaryObject);
		this.playerMoved = true;
		base.gameObject.SetActive(false);
	}

	public void MovePlayer()
	{
		this.temporaryObject = new GameObject("Temporary");
		this.temporaryObject.transform.position = this.player.position;
		this.temporaryObject.transform.rotation = this.player.rotation;
		Streamer[] array = this.streamers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].player = this.temporaryObject.transform;
		}
		this.player.position = this.safePosition.position;
		this.player.rotation = this.safePosition.rotation;
		base.gameObject.SetActive(true);
		this.playerMoved = false;
	}

	[Tooltip("List of streamers objects that should affect loading screen. Drag and drop here all your streamer objects from scene hierarchy which should be used in loading screen.")]
	public Streamer[] streamers;

	[Space(10f)]
	[Tooltip("Drag and drop here, an object that system have to follow during streaming process.")]
	public Transform player;

	[Tooltip("The player safe position during loading.")]
	public Transform safePosition;

	[Space(10f)]
	public UnityEvent onDone;

	private GameObject temporaryObject;

	private float progress;

	private bool waitForPlayer;

	private bool playerMoved;
}
