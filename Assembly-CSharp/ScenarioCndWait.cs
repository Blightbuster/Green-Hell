using System;
using UnityEngine;

public class ScenarioCndWait : ScenarioElement
{
	public override void Setup()
	{
		base.Setup();
		string[] array = this.m_EncodedContent.Split(new char[]
		{
			':'
		});
		if (array.Length != 3)
		{
			DebugUtils.Assert(string.Concat(new string[]
			{
				"[ScenarioCndWait:Setup] Error in element - ",
				this.m_Content,
				", node - ",
				this.m_Node.m_Name,
				". Check spelling!"
			}), true, DebugUtils.AssertType.Info);
		}
		string text = array[2];
		if (text.Contains(","))
		{
			DebugUtils.Assert(string.Concat(new string[]
			{
				"[ScenarioCndWait:Setup] Error in element - ",
				this.m_Content,
				", node - ",
				this.m_Node.m_Name,
				". Check spelling!"
			}), true, DebugUtils.AssertType.Info);
		}
		this.m_Duration = float.Parse(text);
	}

	public override void Activate()
	{
		base.Activate();
		this.m_StartTime = Time.time;
	}

	protected override bool ShouldComplete()
	{
		return Time.time - this.m_StartTime >= this.m_Duration;
	}

	private float m_StartTime;

	private float m_Duration;
}
