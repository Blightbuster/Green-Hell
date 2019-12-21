using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDSelectDialogElemData
{
	public GameObject m_Object;

	public Text m_Text;

	public string m_DialogName = string.Empty;

	public RawImage m_BG;

	public List<Dialog> m_Dialogs;

	public GameObject m_CounterObject;

	public bool m_IsGroup;

	public GameObject m_PadIconSelect;
}
