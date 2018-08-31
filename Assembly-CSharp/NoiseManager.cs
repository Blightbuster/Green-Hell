using System;
using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
	public static NoiseManager Get()
	{
		return NoiseManager.s_Instance;
	}

	private void Awake()
	{
		NoiseManager.s_Instance = this;
	}

	public static void RegisterReceiver(INoiseReceiver receiver)
	{
		NoiseManager.s_Receivers.Add(receiver);
	}

	public static void UnregisterReceiver(INoiseReceiver receiver)
	{
		NoiseManager.s_Receivers.Remove(receiver);
	}

	public void MakeNoise(Noise noise)
	{
		foreach (INoiseReceiver noiseReceiver in NoiseManager.s_Receivers)
		{
			noiseReceiver.OnNoise(noise);
		}
	}

	public static List<INoiseReceiver> s_Receivers = new List<INoiseReceiver>();

	private static NoiseManager s_Instance = null;
}
