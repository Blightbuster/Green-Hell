using System;

public class HUDTextChat : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.All);
	}

	protected override void OnShow()
	{
		this.m_Field.ActivateInputField();
		InputsManager.Get().m_TextInputActive = true;
	}

	protected override void OnHide()
	{
		this.m_Field.DeactivateInputField();
		this.m_Field.text = string.Empty;
		InputsManager.Get().m_TextInputActive = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_ShouldBeVisible = false;
	}

	private void SendTextMessage()
	{
		string text = this.m_Field.text;
		if (text.Trim().Length > 0)
		{
			P2PSession.Instance.SendTextChatMessage(text);
			HUDMessages.Get().AddMessage(GreenHellGame.Instance.GetLocalization().Get("HUDTextChat_MessagePrefixLocalPlayer", true) + text, null, HUDMessageIcon.None, "", null);
		}
	}

	protected override bool ShouldShow()
	{
		return this.m_ShouldBeVisible;
	}

	public override void ConstantUpdate()
	{
		base.ConstantUpdate();
	}

	private bool m_ShouldBeVisible;

	private bool m_KeyPressed;

	public InputFieldCJ m_Field;
}
