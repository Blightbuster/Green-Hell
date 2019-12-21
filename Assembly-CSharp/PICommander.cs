using System;
using System.Collections.Generic;
using UnityEngine;

public class PICommander : MonoBehaviour
{
	public PICommander()
	{
		PICommander.s_Commanders.Add(this);
	}

	private void OnDestroy()
	{
		PICommander.s_Commanders.Remove(this);
	}

	public static PICommander GetCommander(string commander_name)
	{
		int i = 0;
		while (i < PICommander.s_Commanders.Count)
		{
			if (PICommander.s_Commanders[i].Equals(null))
			{
				PICommander.s_Commanders.RemoveAt(i);
			}
			else
			{
				if (PICommander.s_Commanders[i].gameObject.name == commander_name)
				{
					return PICommander.s_Commanders[i];
				}
				i++;
			}
		}
		return null;
	}

	protected virtual void ResetParams()
	{
		this.m_DisableWithFadeStartTime = float.MaxValue;
		this.m_DisableWithFadeDuration = 3f;
	}

	public void DisableWithFade(float time)
	{
		this.m_DisableWithFadeStartTime = Time.time;
		this.m_DisableWithFadeDuration = time;
	}

	private static List<PICommander> s_Commanders = new List<PICommander>();

	protected float m_DisableWithFadeStartTime = float.MaxValue;

	protected float m_DisableWithFadeDuration = 3f;
}
