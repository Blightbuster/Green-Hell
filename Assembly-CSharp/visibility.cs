using System;
using UnityEngine;

public class visibility : MonoBehaviour
{
	private void Start()
	{
		visibility.rate = base.GetComponent<ParticleSystem>().emissionRate;
	}

	private void OnGUI()
	{
		if (base.name == "cloud1" && GUI.Button(new Rect(30f, 30f, 20f, 20f), visibility.act1))
		{
			base.GetComponent<Renderer>().enabled = true;
			visibility.act1 = "x";
			visibility.act2 = "2";
			visibility.act3 = "3";
			visibility.act4 = "4";
			visibility.act5 = "5";
			visibility.act6 = "6";
		}
		if (base.name == "cloud2" && GUI.Button(new Rect(60f, 30f, 20f, 20f), visibility.act2))
		{
			base.GetComponent<Renderer>().enabled = true;
			visibility.act1 = "1";
			visibility.act2 = "x";
			visibility.act3 = "3";
			visibility.act4 = "4";
			visibility.act5 = "5";
			visibility.act6 = "6";
		}
		if (base.name == "cloud3" && GUI.Button(new Rect(90f, 30f, 20f, 20f), visibility.act3))
		{
			base.GetComponent<Renderer>().enabled = true;
			visibility.act1 = "1";
			visibility.act2 = "2";
			visibility.act3 = "x";
			visibility.act4 = "4";
			visibility.act5 = "5";
			visibility.act6 = "6";
		}
		if (base.name == "cloud4" && GUI.Button(new Rect(120f, 30f, 20f, 20f), visibility.act4))
		{
			base.GetComponent<Renderer>().enabled = true;
			visibility.act1 = "1";
			visibility.act2 = "2";
			visibility.act3 = "3";
			visibility.act4 = "x";
			visibility.act5 = "5";
			visibility.act6 = "6";
		}
		if (base.name == "cloud5" && GUI.Button(new Rect(150f, 30f, 20f, 20f), visibility.act5))
		{
			base.GetComponent<Renderer>().enabled = true;
			visibility.act1 = "1";
			visibility.act2 = "2";
			visibility.act3 = "3";
			visibility.act4 = "4";
			visibility.act5 = "x";
			visibility.act6 = "6";
		}
		if (base.name == "cloud6" && GUI.Button(new Rect(180f, 30f, 20f, 20f), visibility.act6))
		{
			base.GetComponent<Renderer>().enabled = true;
			visibility.act1 = "1";
			visibility.act2 = "2";
			visibility.act3 = "3";
			visibility.act4 = "4";
			visibility.act5 = "5";
			visibility.act6 = "x";
		}
	}

	private void Update()
	{
		if (this.tr != transparency.density)
		{
			this.tr = transparency.density;
			base.GetComponent<ParticleSystem>().emissionRate = visibility.rate * this.tr;
			MonoBehaviour.print(this.tr);
		}
		if (this.drk != transparency.darkness)
		{
			this.drk = transparency.darkness;
			base.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1f - this.drk, 1f - this.drk, 1f - this.drk, base.GetComponent<Renderer>().material.GetColor("_TintColor").a));
			MonoBehaviour.print(this.tr);
		}
		if (base.name == "cloud1" && base.GetComponent<Renderer>().enabled && visibility.act1 == "1")
		{
			base.GetComponent<Renderer>().enabled = false;
		}
		if (base.name == "cloud2" && base.GetComponent<Renderer>().enabled && visibility.act2 == "2")
		{
			base.GetComponent<Renderer>().enabled = false;
		}
		if (base.name == "cloud3" && base.GetComponent<Renderer>().enabled && visibility.act3 == "3")
		{
			base.GetComponent<Renderer>().enabled = false;
		}
		if (base.name == "cloud4" && base.GetComponent<Renderer>().enabled && visibility.act4 == "4")
		{
			base.GetComponent<Renderer>().enabled = false;
		}
		if (base.name == "cloud5" && base.GetComponent<Renderer>().enabled && visibility.act5 == "5")
		{
			base.GetComponent<Renderer>().enabled = false;
		}
		if (base.name == "cloud6" && base.GetComponent<Renderer>().enabled && visibility.act6 == "6")
		{
			base.GetComponent<Renderer>().enabled = false;
		}
	}

	public static string act1 = "1";

	public static string act2 = "2";

	public static string act3 = "3";

	public static string act4 = "4";

	public static string act5 = "x";

	public static string act6 = "6";

	public static float rate;

	private float drk;

	private float tr = 1f;
}
