using System;
using System.Collections.Generic;
using CJTools;

public class PIDistanceCommander : PICommander
{
	private void Update()
	{
		float b = Player.Get().transform.position.Distance(base.transform.position);
		float proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_MinWeight, this.m_MaxWeight, b, this.m_MinDistance, this.m_MaxDistance);
		for (int i = 0; i < this.m_Interpolators.Count; i++)
		{
			this.m_Interpolators[i].SetWeight(proportionalClamp);
		}
	}

	public float m_MinDistance = 10f;

	public float m_MaxDistance = 100f;

	public float m_MinWeight;

	public float m_MaxWeight = 1f;

	public List<ParametersInterpolator> m_Interpolators = new List<ParametersInterpolator>();
}
