using System;
using UnityEngine;
using UnityEngine.Sprites;

[ExecuteInEditMode]
public class SpriteMaskController : MonoBehaviour
{
	private void OnEnable()
	{
		this.m_spriteRenderer = base.GetComponent<SpriteRenderer>();
		this.m_uvs = DataUtility.GetInnerUV(this.m_spriteRenderer.sprite);
		this.m_spriteRenderer.sharedMaterial.SetVector("_CustomUVS", this.m_uvs);
	}

	private SpriteRenderer m_spriteRenderer;

	private Vector4 m_uvs;
}
