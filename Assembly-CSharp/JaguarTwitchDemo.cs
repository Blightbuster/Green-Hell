using System;
using UnityEngine;

public class JaguarTwitchDemo : MonoBehaviour
{
	private void Start()
	{
		JaguarTwitchDemo.s_Object = base.gameObject;
	}

	public static GameObject s_Object;
}
