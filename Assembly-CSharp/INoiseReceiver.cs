using System;
using UnityEngine;

public interface INoiseReceiver
{
	GameObject GetObject();

	void OnNoise(Noise noise);
}
