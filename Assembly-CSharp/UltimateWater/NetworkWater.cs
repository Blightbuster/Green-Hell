using System;
using UnityEngine;
using UnityEngine.Networking;

namespace UltimateWater
{
	[AddComponentMenu("Ultimate Water/Network Synchronization", 2)]
	[RequireComponent(typeof(Water))]
	public class NetworkWater : NetworkBehaviour
	{
		private void Awake()
		{
			this._Water = base.GetComponent<Water>();
			if (this._Water == null)
			{
				base.enabled = false;
				Debug.LogWarning("[Water] component not assigned to [Network Water]");
			}
		}

		private void Update()
		{
			if (base.isServer)
			{
				this.Network_Time = Time.time;
			}
			else
			{
				this.Network_Time = this._Time + Time.deltaTime;
			}
			this._Water.Time = this._Time;
		}

		private void UNetVersion()
		{
		}

		public float Network_Time
		{
			get
			{
				return this._Time;
			}
			set
			{
				base.SetSyncVar<float>(value, ref this._Time, 1u);
			}
		}

		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				writer.Write(this._Time);
				return true;
			}
			bool flag = false;
			if ((base.syncVarDirtyBits & 1u) != 0u)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.Write(this._Time);
			}
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
			}
			return flag;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (initialState)
			{
				this._Time = reader.ReadSingle();
				return;
			}
			int num = (int)reader.ReadPackedUInt32();
			if ((num & 1) != 0)
			{
				this._Time = reader.ReadSingle();
			}
		}

		[SyncVar]
		private float _Time;

		private Water _Water;
	}
}
