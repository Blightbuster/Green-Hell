using System;

public class ItemTool : Item
{
	protected override void Start()
	{
		base.Start();
		this.m_IsFireTool = (base.gameObject.GetComponent<FireTool>() != null);
	}

	public override bool IsFireTool()
	{
		return this.m_IsFireTool;
	}

	public void OnWaitingForKindling()
	{
		this.m_InUse = true;
	}

	public void OnStartMakeFireGame()
	{
	}

	public void OnSuccessMakeFireGame()
	{
		this.m_InUse = false;
	}

	public void OnFailMakeFireGame()
	{
		this.m_InUse = false;
	}

	public void OnRemovedFromHand()
	{
		this.m_InUse = false;
	}

	public override bool CanBeFocuedInInventory()
	{
		return !this.m_InUse;
	}

	private bool m_IsFireTool;

	private bool m_InUse;
}
