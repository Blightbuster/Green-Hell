using System;

public class ReplicatedPlayerParams : ReplicatedBehaviour
{
	public bool m_IsRunning
	{
		get
		{
			return this.m_IsRunningInternal;
		}
		private set
		{
			if (this.m_IsRunningInternal != value)
			{
				this.m_IsRunningInternal = value;
				this.ReplSetDirty();
			}
		}
	}

	public bool m_IsInWater
	{
		get
		{
			return this.m_IsInWaterInternal;
		}
		private set
		{
			if (this.m_IsInWaterInternal != value)
			{
				this.m_IsInWaterInternal = value;
				this.ReplSetDirty();
			}
		}
	}

	public bool m_IsSwimming
	{
		get
		{
			return this.m_IsSwimmingInternal;
		}
		private set
		{
			if (this.m_IsSwimmingInternal != value)
			{
				this.m_IsSwimmingInternal = value;
				this.ReplSetDirty();
			}
		}
	}

	public bool m_IsSleeping
	{
		get
		{
			return this.m_IsSleepingInternal;
		}
		private set
		{
			if (this.m_IsSleepingInternal != value)
			{
				this.m_IsSleepingInternal = value;
				this.ReplSetDirty();
			}
		}
	}

	public int m_LastCollisionFlags
	{
		get
		{
			return this.m_LastCollisionFlagsInternal;
		}
		private set
		{
			if (this.m_LastCollisionFlagsInternal != value)
			{
				this.m_LastCollisionFlagsInternal = value;
				this.ReplSetDirty();
			}
		}
	}

	public float m_WantedSpeed2d
	{
		get
		{
			return this.m_WantedSpeed2dInternal;
		}
		private set
		{
			if (Math.Abs(this.m_WantedSpeed2dInternal - value) > 0.01f)
			{
				this.m_WantedSpeed2dInternal = value;
				this.ReplSetDirty();
			}
		}
	}

	public bool m_IsDead
	{
		get
		{
			return this.m_IsDeadInternal;
		}
		private set
		{
			if (this.m_IsDeadInternal != value)
			{
				this.m_IsDeadInternal = value;
				this.ReplSetDirty();
			}
		}
	}

	public bool m_IsInSafeZone
	{
		get
		{
			return this.m_IsInSafeZoneInternal;
		}
		private set
		{
			if (this.m_IsInSafeZoneInternal != value)
			{
				this.m_IsInSafeZoneInternal = value;
				this.ReplSetDirty();
			}
		}
	}

	public float m_Health
	{
		get
		{
			return this.m_HealthInternal;
		}
		private set
		{
			if (Math.Abs(this.m_HealthInternal - value) > 0.01f)
			{
				this.m_HealthInternal = value;
				this.ReplSetDirty();
			}
		}
	}

	public float m_MaxHealth
	{
		get
		{
			return this.m_MaxHealthInternal;
		}
		private set
		{
			if (Math.Abs(this.m_MaxHealthInternal - value) > 0.01f)
			{
				this.m_MaxHealthInternal = value;
				this.ReplSetDirty();
			}
		}
	}

	private void Update()
	{
		if (base.ReplIsOwner())
		{
			this.m_IsRunning = Player.Get().GetFPPController().IsRunning();
			this.m_IsInWater = Player.Get().IsInWater();
			this.m_IsSwimming = Player.Get().m_SwimController.IsActive();
			this.m_IsSleeping = Player.Get().m_SleepController.IsSleeping();
			this.m_LastCollisionFlags = (int)Player.Get().GetFPPController().m_LastCollisionFlags;
			this.m_WantedSpeed2d = Player.Get().GetFPPController().m_WantedSpeed.current.To2D().magnitude;
			this.m_IsDead = Player.Get().IsDead();
			this.m_IsInSafeZone = Player.Get().IsInSafeZone();
			this.m_Health = PlayerConditionModule.Get().GetHP();
			this.m_MaxHealth = PlayerConditionModule.Get().GetMaxHP();
		}
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		writer.Write(this.m_IsRunning);
		writer.Write(this.m_IsInWater);
		writer.Write(this.m_IsSwimming);
		writer.Write(this.m_IsSleeping);
		writer.Write(this.m_LastCollisionFlags);
		writer.Write(this.m_WantedSpeed2d);
		writer.Write(this.m_IsDead);
		writer.Write(this.m_IsInSafeZone);
		writer.Write(this.m_Health);
		writer.Write(this.m_MaxHealth);
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		this.m_IsRunningInternal = reader.ReadBoolean();
		this.m_IsInWaterInternal = reader.ReadBoolean();
		this.m_IsSwimmingInternal = reader.ReadBoolean();
		this.m_IsSleepingInternal = reader.ReadBoolean();
		this.m_LastCollisionFlagsInternal = reader.ReadInt32();
		this.m_WantedSpeed2dInternal = reader.ReadFloat();
		this.m_IsDeadInternal = reader.ReadBoolean();
		this.m_IsInSafeZoneInternal = reader.ReadBoolean();
		this.m_HealthInternal = reader.ReadFloat();
		this.m_MaxHealthInternal = reader.ReadFloat();
	}

	private bool m_IsRunningInternal;

	private bool m_IsInWaterInternal;

	private bool m_IsSwimmingInternal;

	private bool m_IsSleepingInternal;

	private int m_LastCollisionFlagsInternal;

	private float m_WantedSpeed2dInternal;

	private bool m_IsDeadInternal;

	private bool m_IsInSafeZoneInternal;

	private float m_HealthInternal;

	private float m_MaxHealthInternal;
}
