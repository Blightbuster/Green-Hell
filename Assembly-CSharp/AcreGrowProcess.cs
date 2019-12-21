using System;

public class AcreGrowProcess : Trigger, IProcessor
{
	protected override void Start()
	{
		base.Start();
		if (this.m_Acre && !HUDProcess.Get().IsProcessRegistered(this))
		{
			if (this.m_Acre.GetState() == AcreState.Ready)
			{
				HUDProcess.Get().RegisterProcess(this, "plant_icon", this, true);
				return;
			}
			if (this.m_Acre.GetState() == AcreState.Growing || this.m_Acre.GetState() == AcreState.GrownNoFruits)
			{
				HUDProcess.Get().RegisterProcess(this, "grow_icon0", this, true);
			}
		}
	}

	public float GetProcessProgress(Trigger trigger)
	{
		if (this.m_Acre.GetState() == AcreState.GrownNoFruits)
		{
			return this.m_Acre.GetRespawnProgress();
		}
		if (this.m_Acre.m_Plant == null)
		{
			return 0f;
		}
		return this.m_Acre.m_Plant.transform.localScale.x;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		HUDProcess.Get().UnregisterProcess(this);
	}

	public Acre m_Acre;
}
