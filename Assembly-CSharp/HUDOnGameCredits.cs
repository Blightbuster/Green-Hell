using System;
using System.Collections.Generic;
using UnityEngine;

public class HUDOnGameCredits : HUDBase
{
	protected override void Awake()
	{
		base.Awake();
		HUDOnGameCredits.s_Instance = this;
		this.m_Childs = new List<CanvasGroup>(base.gameObject.GetComponentsInChildren<CanvasGroup>());
		foreach (CanvasGroup canvasGroup in this.m_Childs)
		{
			canvasGroup.alpha = 0f;
		}
	}

	protected override bool ShouldShow()
	{
		return base.enabled;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.m_Childs.Count > this.m_CurrentChildIndex)
		{
			this.SetState(HUDOnGameCredits.State.Showing);
			return;
		}
		base.enabled = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	private void SetState(HUDOnGameCredits.State state)
	{
		this.m_State = state;
		this.m_EnterStateTime = Time.time;
	}

	protected override void Update()
	{
		base.Update();
		switch (this.m_State)
		{
		case HUDOnGameCredits.State.Showing:
		{
			CanvasGroup canvasGroup = this.m_Childs[this.m_CurrentChildIndex];
			canvasGroup.alpha = Mathf.Min(1f, canvasGroup.alpha + Time.deltaTime * this.m_Speed);
			if (canvasGroup.alpha >= 1f)
			{
				this.SetState(HUDOnGameCredits.State.Shown);
				return;
			}
			break;
		}
		case HUDOnGameCredits.State.Shown:
			if (Time.time - this.m_EnterStateTime >= this.m_Interval)
			{
				this.SetState(HUDOnGameCredits.State.Hiding);
				return;
			}
			break;
		case HUDOnGameCredits.State.Hiding:
		{
			CanvasGroup canvasGroup2 = this.m_Childs[this.m_CurrentChildIndex];
			canvasGroup2.alpha = Mathf.Max(0f, canvasGroup2.alpha - Time.deltaTime * this.m_Speed);
			if (canvasGroup2.alpha <= 0f)
			{
				this.m_CurrentChildIndex++;
				if (this.m_Childs.Count > this.m_CurrentChildIndex)
				{
					this.SetState(HUDOnGameCredits.State.Showing);
					return;
				}
				base.enabled = false;
				this.m_Finished = true;
				this.SetState(HUDOnGameCredits.State.None);
			}
			break;
		}
		default:
			return;
		}
	}

	public bool IsFinished()
	{
		return this.m_Finished;
	}

	private HUDOnGameCredits.State m_State;

	private float m_EnterStateTime;

	public float m_Interval;

	public float m_Speed = 1f;

	private List<CanvasGroup> m_Childs;

	private int m_CurrentChildIndex;

	private bool m_Finished;

	private static HUDOnGameCredits s_Instance;

	private enum State
	{
		None,
		Showing,
		Shown,
		Hiding
	}
}
