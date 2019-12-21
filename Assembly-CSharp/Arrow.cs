using System;

public class Arrow : Weapon
{
	public bool m_Loaded
	{
		get
		{
			return this.m_LoadedProp;
		}
		set
		{
			this.m_LoadedProp = value;
			base.UpdateScale(false);
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !this.m_Loaded && base.CanTrigger();
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("ArrowLoaded" + index, this.m_Loaded);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_Loaded = SaveGame.LoadBVal("ArrowLoaded" + index);
	}

	public override void SetupAfterLoad(int index)
	{
		base.SetupAfterLoad(index);
		if (this.m_Loaded)
		{
			if (BowController.Get().IsActive())
			{
				BowController.Get().SetArrow(this);
				return;
			}
			InventoryBackpack.Get().InsertItem(this, null, null, true, true, true, true, true);
		}
	}

	private bool m_LoadedProp;

	public float m_AimScale = 1f;
}
