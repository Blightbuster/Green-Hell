using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDPassOut : HUDBase
{
	public static HUDPassOut Get()
	{
		return HUDPassOut.s_Instance;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void Awake()
	{
		base.Awake();
		DebugUtils.Assert(this.m_Text, true);
		DebugUtils.Assert(this.m_BG, true);
		HUDPassOut.s_Instance = this;
	}

	protected override bool ShouldShow()
	{
		return ConsciousnessController.Get().GetPassingOutProgress() > 0f;
	}

	protected override void OnShow()
	{
		this.m_Text.enabled = true;
		Color color = this.m_Text.color;
		color.a = 0f;
		this.m_Text.color = color;
		this.m_BG.enabled = true;
		color = this.m_BG.color;
		color.a = 0f;
		this.m_BG.color = color;
	}

	protected override void OnHide()
	{
		this.m_Text.enabled = false;
		this.m_BG.enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_BG.color.a < 1f)
		{
			Color color = this.m_BG.color;
			color.a += Time.deltaTime;
			float num = 0.7f;
			if (ConsciousnessController.Get().IsState(ConsciousnessController.ConsciousnessState.PassingOut) && color.a > num)
			{
				color.a = Mathf.Clamp(color.a, 0f, num);
			}
			else
			{
				color.a = Mathf.Clamp01(color.a);
			}
			this.m_BG.color = color;
			return;
		}
		if (this.m_Text.color.a < 1f)
		{
			Color color2 = this.m_Text.color;
			color2.a += Time.deltaTime;
			color2.a = Mathf.Clamp01(color2.a);
			this.m_Text.color = color2;
		}
	}

	public Text m_Text;

	public RawImage m_BG;

	private static HUDPassOut s_Instance;
}
