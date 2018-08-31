using System;

public class UnbreakableObject : CJObject
{
	public override bool TakeDamage(DamageInfo damage_info)
	{
		bool result = base.TakeDamage(damage_info);
		if (this.m_ChatterName != string.Empty)
		{
			ChatterManager.Get().Play(this.m_ChatterName, 0f);
		}
		return result;
	}

	public string m_ChatterName = string.Empty;
}
