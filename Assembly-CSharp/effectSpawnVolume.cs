﻿using System;
using UnityEngine;

public class effectSpawnVolume : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		this.armed = true;
	}

	private void OnTriggerExit(Collider other)
	{
		this.armed = false;
	}

	private void Update()
	{
		if (this.armed && Input.GetKeyDown(KeyCode.Space))
		{
			UnityEngine.Object.Instantiate<Transform>(this.effectToSpawn, base.transform.position + this.spawnPosition, Quaternion.Euler(this.spawnRotation));
		}
	}

	public Transform effectToSpawn;

	public Vector3 spawnPosition = Vector3.zero;

	public Vector3 spawnRotation = Vector3.zero;

	public bool armed;
}
