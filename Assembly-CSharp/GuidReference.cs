using System;
using UnityEngine;

[Serializable]
public class GuidReference : ISerializationCallbackReceiver
{
	public event Action<GameObject> OnGuidAdded = delegate(GameObject go)
	{
	};

	public event Action OnGuidRemoved = delegate
	{
	};

	public GameObject gameObject
	{
		get
		{
			if (this.isCacheSet)
			{
				return this.cachedReference;
			}
			this.cachedReference = GuidManager.ResolveGuid(this.guid, this.addDelegate, this.removeDelegate);
			this.isCacheSet = true;
			return this.cachedReference;
		}
		private set
		{
		}
	}

	public GuidReference()
	{
	}

	public GuidReference(GuidComponent target)
	{
		this.guid = target.GetGuid();
	}

	private void GuidAdded(GameObject go)
	{
		this.cachedReference = go;
		this.OnGuidAdded(go);
	}

	private void GuidRemoved()
	{
		this.cachedReference = null;
		this.isCacheSet = false;
		this.OnGuidRemoved();
	}

	public void OnBeforeSerialize()
	{
		this.serializedGuid = this.guid.ToByteArray();
	}

	public void OnAfterDeserialize()
	{
		this.cachedReference = null;
		this.isCacheSet = false;
		if (this.serializedGuid == null || this.serializedGuid.Length != 16)
		{
			this.serializedGuid = new byte[16];
		}
		this.guid = new Guid(this.serializedGuid);
		this.addDelegate = new Action<GameObject>(this.GuidAdded);
		this.removeDelegate = new Action(this.GuidRemoved);
	}

	private GameObject cachedReference;

	private bool isCacheSet;

	[SerializeField]
	private byte[] serializedGuid;

	private Guid guid;

	private Action<GameObject> addDelegate;

	private Action removeDelegate;
}
