using System;
using System.Collections.Generic;
using CJTools;

public class SkillCurve
{
	public void Load(Key key)
	{
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "Key")
			{
				SkillCurveKey skillCurveKey = new SkillCurveKey();
				skillCurveKey.Load(key2);
				this.AddKey(skillCurveKey);
			}
		}
	}

	public void AddKey(SkillCurveKey key)
	{
		key.m_Index = this.m_Curve.Count;
		this.m_Curve.Add(key);
	}

	public float Progress(float val)
	{
		SkillCurveKey key = this.GetKey(val);
		return key.m_Data.z;
	}

	public float Evaluate(float val)
	{
		SkillCurveKey key = this.GetKey(val);
		float result;
		if (val == key.m_Data.x)
		{
			result = key.m_Data.z;
		}
		else
		{
			SkillCurveKey nextKey = this.GetNextKey(key);
			float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, val, key.m_Data.x, key.m_Data.y);
			result = CJTools.Math.GetProportionalClamp(key.m_Data.z, nextKey.m_Data.z, proportionalClamp, 0f, 1f);
		}
		return result;
	}

	public int GetLevel(float val)
	{
		SkillCurveKey key = this.GetKey(val);
		return (val >= Skill.s_MaxValue) ? (key.m_Index + 1) : key.m_Index;
	}

	public SkillCurveKey GetKey(float val)
	{
		if (this.m_Curve.Count == 0)
		{
			DebugUtils.Assert("[SkillProgressCurve:] Error, curve is empty!", true, DebugUtils.AssertType.Info);
			return null;
		}
		foreach (SkillCurveKey skillCurveKey in this.m_Curve)
		{
			if ((skillCurveKey.m_Index == 0 && val < skillCurveKey.m_Data.x) || (skillCurveKey.m_Index == this.m_Curve.Count - 1 && val >= skillCurveKey.m_Data.y) || (val >= skillCurveKey.m_Data.x && val < skillCurveKey.m_Data.y))
			{
				return skillCurveKey;
			}
		}
		DebugUtils.Assert("[SkillProgressCurve:] Error, can't find key!", true, DebugUtils.AssertType.Info);
		return null;
	}

	private SkillCurveKey GetNextKey(SkillCurveKey key)
	{
		int i = 0;
		while (i < this.m_Curve.Count)
		{
			if (key == this.m_Curve[i])
			{
				if (i == this.m_Curve.Count - 1)
				{
					return key;
				}
				return this.m_Curve[i + 1];
			}
			else
			{
				i++;
			}
		}
		DebugUtils.Assert("[SkillProgressCurve:] Error, can't find key!", true, DebugUtils.AssertType.Info);
		return null;
	}

	private SkillCurveKey GetPrevKey(SkillCurveKey key)
	{
		int i = 0;
		while (i < this.m_Curve.Count)
		{
			if (key == this.m_Curve[i])
			{
				if (i == 0)
				{
					return key;
				}
				return this.m_Curve[i - 1];
			}
			else
			{
				i++;
			}
		}
		DebugUtils.Assert("[SkillProgressCurve:] Error, can't find key!", true, DebugUtils.AssertType.Info);
		return null;
	}

	public List<SkillCurveKey> m_Curve = new List<SkillCurveKey>();
}
