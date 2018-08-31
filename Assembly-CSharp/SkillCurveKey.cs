using System;
using UnityEngine;

public class SkillCurveKey
{
	public void Load(Key key)
	{
		this.m_Data.x = key.GetVariable(0).FValue;
		this.m_Data.y = key.GetVariable(1).FValue;
		this.m_Data.z = key.GetVariable(2).FValue;
	}

	public Vector3 m_Data = Vector4.zero;

	public int m_Index;
}
