using System;
using CJTools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISliderEx : Slider, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler, IUIChangeableOption
{
	public void OnBeginDrag(PointerEventData eventData)
	{
		this.m_IsDragged = true;
	}

	public override void OnDrag(PointerEventData data)
	{
		base.OnDrag(data);
		if (this.m_DragClip && UISliderEx.s_LastDragSound < Time.realtimeSinceStartup - 0.1f)
		{
			UIAudioPlayer.Play(this.m_DragClip);
			UISliderEx.s_LastDragSound = Time.realtimeSinceStartup;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		this.m_IsDragged = false;
	}

	public bool DidValueChange()
	{
		return !CJTools.Math.FloatsEqual(this.m_PrevValue, this.value, 2);
	}

	public void StoreValue()
	{
		this.m_PrevValue = this.value;
	}

	public void RevertValue()
	{
		this.value = this.m_PrevValue;
	}

	public bool m_IsDragged;

	private float m_PrevValue;

	private static float s_LastDragSound = float.MinValue;

	public AudioClip m_DragClip;
}
