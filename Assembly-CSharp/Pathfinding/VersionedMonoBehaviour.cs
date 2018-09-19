using System;
using UnityEngine;

namespace Pathfinding
{
	public abstract class VersionedMonoBehaviour : MonoBehaviour, ISerializationCallbackReceiver
	{
		protected virtual void Awake()
		{
			if (Application.isPlaying)
			{
				this.version = this.OnUpgradeSerializedData(int.MaxValue);
			}
		}

		private void Reset()
		{
			this.version = this.OnUpgradeSerializedData(int.MaxValue);
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			this.version = this.OnUpgradeSerializedData(this.version);
		}

		protected virtual int OnUpgradeSerializedData(int version)
		{
			return 1;
		}

		[HideInInspector]
		[SerializeField]
		private int version;
	}
}
