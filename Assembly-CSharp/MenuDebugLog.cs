using System;

public class MenuDebugLog : MenuScreen
{
	protected override void OnShow()
	{
		base.OnShow();
		this.m_List.SetFocus(true);
		this.m_List.Clear();
		string[] array = CJDebug.m_Log.Split(new char[]
		{
			'\n'
		});
		foreach (string element in array)
		{
			this.m_List.AddElement(element, -1);
		}
		this.m_List.SetSelectionIndex(0);
	}

	public UIList m_List;
}
