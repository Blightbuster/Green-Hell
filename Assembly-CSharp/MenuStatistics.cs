using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuStatistics : MenuScreen
{
	public override KeyCode GetShortcutKey()
	{
		if (!GreenHellGame.DEBUG)
		{
			return KeyCode.None;
		}
		return KeyCode.O;
	}

	public override void OnShow()
	{
		base.OnShow();
		this.Setup();
	}

	private void Setup()
	{
		this.m_Content.text = string.Empty;
		for (int i = 0; i < 18; i++)
		{
			Enums.Event stat_type = (Enums.Event)i;
			CJVariable statistic = StatsManager.Get().GetStatistic(stat_type);
			Text content = this.m_Content;
			content.text = content.text + stat_type.ToString() + ": ";
			CJVariable.TYPE variableType = statistic.GetVariableType();
			if (variableType == CJVariable.TYPE.Unknown)
			{
				Text content2 = this.m_Content;
				content2.text += "-\n";
			}
			else
			{
				switch (variableType)
				{
				case CJVariable.TYPE.String:
				{
					Text content3 = this.m_Content;
					content3.text += statistic.SValue;
					break;
				}
				case CJVariable.TYPE.Int:
				{
					Text content4 = this.m_Content;
					content4.text += statistic.IValue.ToString();
					break;
				}
				case CJVariable.TYPE.Float:
				{
					Text content5 = this.m_Content;
					content5.text += statistic.FValue.ToString("F1");
					break;
				}
				case CJVariable.TYPE.Bool:
				{
					Text content6 = this.m_Content;
					content6.text += statistic.BValue.ToString();
					break;
				}
				}
				Text content7 = this.m_Content;
				content7.text += "\n";
			}
		}
	}

	public Text m_Content;
}
