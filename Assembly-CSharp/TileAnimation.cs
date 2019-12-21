using System;
using UnityEngine;

public class TileAnimation : MonoBehaviour
{
	private void Awake()
	{
		this.rendererReference = base.GetComponent<Renderer>();
		this.rendererReference.materials[0].mainTextureScale = new Vector2(1f / (float)this.xFrames, 1f / (float)this.yFrames);
		if (this.billboard && !this.mainCamera)
		{
			this.mainCamera = Camera.main;
		}
		this.randomStart = (int)(UnityEngine.Random.value * (float)this.xFrames * (float)this.yFrames);
	}

	private void Update()
	{
		this.frame = (int)Mathf.Repeat((float)(Mathf.FloorToInt(Time.time * this.speed) + this.randomStart), (float)(this.xFrames * this.yFrames));
		int num = this.frame % this.xFrames;
		int num2 = this.frame / this.xFrames;
		this.offset.x = (float)num / ((float)this.xFrames * 1f);
		this.offset.y = 1f - ((float)num2 + 1f) / ((float)this.yFrames * 1f);
		this.rendererReference.materials[0].mainTextureOffset = this.offset;
		if (this.billboard)
		{
			base.transform.LookAt(base.transform.position + this.mainCamera.transform.rotation * Vector3.forward, this.mainCamera.transform.rotation * Vector3.up);
		}
	}

	public int xFrames = 8;

	public int yFrames = 4;

	public float speed = 25f;

	public bool billboard;

	public Camera mainCamera;

	private int frame;

	private Renderer rendererReference;

	private int randomStart;

	private Vector2 offset;
}
