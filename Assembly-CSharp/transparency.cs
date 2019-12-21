using System;
using UnityEngine;

public class transparency : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(300f, 25f, 200f, 20f), "Clouds Density:");
		transparency.density = GUI.HorizontalSlider(new Rect(300f, 45f, 130f, 20f), transparency.density, 0.5f, 1.5f);
		GUI.Label(new Rect(600f, 25f, 200f, 20f), "Clouds Darkness:");
		transparency.darkness = GUI.HorizontalSlider(new Rect(600f, 45f, 130f, 20f), transparency.darkness, 0f, 0.4f);
		GUI.Label(new Rect(760f, 25f, 200f, 80f), "It takes time to reduce cloudes density (old particle have to die). Due to the particle based system, total number of unique clouds is unlimited");
	}

	private void Update()
	{
	}

	private Transform cl1;

	private Transform cl2;

	private Transform cl3;

	private Transform cl4;

	private Transform cl5;

	public static float darkness = 0f;

	public static float density = 1f;
}
