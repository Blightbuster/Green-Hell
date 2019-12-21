using System;

public class MenuDebugScenario : MenuDebugScreen
{
	public override void OnShow()
	{
		base.OnShow();
		this.m_List.SetFocus(true);
		this.Setup();
	}

	public void OnButtonComplete()
	{
		int selectedElementData = this.m_List.GetSelectedElementData<int>();
		if (selectedElementData >= 0 && selectedElementData < Scenario.Get().m_Nodes.Count)
		{
			Scenario.Get().m_Nodes[selectedElementData].Complete();
			this.Setup();
		}
	}

	private void Setup()
	{
		this.m_List.Clear();
		for (int i = 0; i < Scenario.Get().m_Nodes.Count; i++)
		{
			this.m_List.AddElement(Scenario.Get().m_Nodes[i].m_Name + " - " + Scenario.Get().m_Nodes[i].m_State.ToString(), i);
		}
	}

	public UIList m_List;
}
