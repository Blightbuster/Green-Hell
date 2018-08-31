using System;
using System.Collections.Generic;
using UnityEngine;

public class IconColors
{
	public IconColors()
	{
		IconColors.s_Instance = this;
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("IconColors", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			IconColors.Icon key2 = (IconColors.Icon)Enum.Parse(typeof(IconColors.Icon), key.GetName());
			Color value = new Color((float)key.GetVariable(0).IValue / 255f, (float)key.GetVariable(1).IValue / 255f, (float)key.GetVariable(2).IValue / 255f, (float)key.GetVariable(3).IValue / 255f);
			this.m_ColorsData.Add(key2, value);
		}
	}

	public static Color GetColor(IconColors.Icon icon)
	{
		Color white = Color.white;
		IconColors.s_Instance.m_ColorsData.TryGetValue(icon, out white);
		return white;
	}

	private Dictionary<IconColors.Icon, Color> m_ColorsData = new Dictionary<IconColors.Icon, Color>();

	private static IconColors s_Instance;

	public enum Icon
	{
		Carbo,
		Fat,
		Proteins,
		Hydration,
		Energy
	}
}
