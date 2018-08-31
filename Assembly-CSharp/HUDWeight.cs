using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDWeight : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return InventoryBackpack.Get().m_Backpack.gameObject.activeSelf;
	}

	protected override void Update()
	{
		base.Update();
		if (!this.m_Text)
		{
			return;
		}
		this.UpdateText();
		this.UpdateColor();
	}

	private void UpdateText()
	{
		this.m_Text.text = InventoryBackpack.Get().m_CurrentWeight.ToString("F1") + "/" + InventoryBackpack.Get().m_MaxWeight.ToString("F1");
	}

	private void UpdateColor()
	{
		float a = this.m_Text.color.a;
		if (InventoryBackpack.Get().IsMaxOverload())
		{
			this.m_Text.color = Color.red;
		}
		else if (InventoryBackpack.Get().IsCriticalOverload())
		{
			float num = Mathf.Abs(Mathf.Sin(Time.time * 2f));
			Color white = Color.white;
			white.g = num;
			white.b = num;
			this.m_Text.color = white;
		}
		else
		{
			this.m_Text.color = Color.white;
		}
		Color color = this.m_Text.color;
		color.a = a;
		this.m_Text.color = color;
	}

	public Text m_Text;
}
