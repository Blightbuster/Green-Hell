using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class GuidManager
{
	public static bool Add(GuidComponent guidComponent)
	{
		if (GuidManager.Instance == null)
		{
			GuidManager.Instance = new GuidManager();
		}
		return GuidManager.Instance.InternalAdd(guidComponent);
	}

	public static void Remove(Guid guid)
	{
		if (GuidManager.Instance == null)
		{
			GuidManager.Instance = new GuidManager();
		}
		GuidManager.Instance.InternalRemove(guid);
	}

	public static GameObject ResolveGuid(Guid guid, Action<GameObject> onAddCallback, Action onRemoveCallback)
	{
		if (GuidManager.Instance == null)
		{
			GuidManager.Instance = new GuidManager();
		}
		return GuidManager.Instance.ResolveGuidInternal(guid, onAddCallback, onRemoveCallback);
	}

	public static GameObject ResolveGuid(Guid guid, Action onDestroyCallback)
	{
		if (GuidManager.Instance == null)
		{
			GuidManager.Instance = new GuidManager();
		}
		return GuidManager.Instance.ResolveGuidInternal(guid, null, onDestroyCallback);
	}

	public static GameObject ResolveGuid(Guid guid)
	{
		if (GuidManager.Instance == null)
		{
			GuidManager.Instance = new GuidManager();
		}
		return GuidManager.Instance.ResolveGuidInternal(guid, null, null);
	}

	public static GameObject ResolveGuid(byte[] guid_bytes)
	{
		return GuidManager.Instance.ResolveGuidInternal(new Guid(guid_bytes), null, null);
	}

	private GuidManager()
	{
		this.guidToObjectMap = new Dictionary<Guid, GuidManager.GuidInfo>();
	}

	private bool InternalAdd(GuidComponent guidComponent)
	{
		Guid guid = guidComponent.GetGuid();
		GuidManager.GuidInfo guidInfo = new GuidManager.GuidInfo(guidComponent);
		if (!this.guidToObjectMap.ContainsKey(guid))
		{
			this.guidToObjectMap.Add(guid, guidInfo);
			return true;
		}
		GuidManager.GuidInfo guidInfo2 = this.guidToObjectMap[guid];
		if (guidInfo2.go != null && guidInfo2.go != guidComponent.gameObject)
		{
			if (Application.isPlaying)
			{
				Debug.LogWarningFormat("Guid Collision Detected between {0} and {1}.\nAssigning new Guid. Consider tracking runtime instances using a direct reference or other method.", new object[]
				{
					(this.guidToObjectMap[guid].go != null) ? this.guidToObjectMap[guid].go.name : "NULL",
					(guidComponent != null) ? guidComponent.name : "NULL"
				});
			}
			else
			{
				Debug.LogWarningFormat(guidComponent, "Guid Collision Detected while creating {0}.\nAssigning new Guid.", new object[]
				{
					(guidComponent != null) ? guidComponent.name : "NULL"
				});
			}
			return false;
		}
		guidInfo2.go = guidInfo.go;
		guidInfo2.HandleAddCallback();
		this.guidToObjectMap[guid] = guidInfo2;
		return true;
	}

	private void InternalRemove(Guid guid)
	{
		GuidManager.GuidInfo guidInfo;
		if (this.guidToObjectMap.TryGetValue(guid, out guidInfo))
		{
			guidInfo.HandleRemoveCallback();
		}
		this.guidToObjectMap.Remove(guid);
	}

	private GameObject ResolveGuidInternal(Guid guid, Action<GameObject> onAddCallback, Action onRemoveCallback)
	{
		if (guid != Guid.Empty)
		{
			GuidManager.GuidInfo guidInfo;
			if (this.guidToObjectMap.TryGetValue(guid, out guidInfo))
			{
				if (onAddCallback != null)
				{
					guidInfo.OnAdd += onAddCallback;
				}
				if (onRemoveCallback != null)
				{
					guidInfo.OnRemove += onRemoveCallback;
				}
				this.guidToObjectMap[guid] = guidInfo;
				return guidInfo.go;
			}
			if (onAddCallback != null)
			{
				guidInfo.OnAdd += onAddCallback;
			}
			if (onRemoveCallback != null)
			{
				guidInfo.OnRemove += onRemoveCallback;
			}
			this.guidToObjectMap.Add(guid, guidInfo);
		}
		return null;
	}

	private static GuidManager Instance;

	private static GuidManager.GuidBuffer s_Helper;

	private Dictionary<Guid, GuidManager.GuidInfo> guidToObjectMap;

	private struct GuidInfo
	{
		public event Action<GameObject> OnAdd;

		public event Action OnRemove;

		public GuidInfo(GuidComponent comp)
		{
			this.go = comp.gameObject;
			this.OnRemove = null;
			this.OnAdd = null;
		}

		public void HandleAddCallback()
		{
			if (this.OnAdd != null)
			{
				this.OnAdd(this.go);
			}
		}

		public void HandleRemoveCallback()
		{
			if (this.OnRemove != null)
			{
				this.OnRemove();
			}
		}

		public GameObject go;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct GuidBuffer
	{
		public GuidBuffer(Guid guid)
		{
			this = default(GuidManager.GuidBuffer);
			this.Guid = guid;
		}

		public unsafe void CopyFrom(byte[] src)
		{
			if (src.Length < GuidComponent.GUID_BYTES_CNT)
			{
				throw new ArgumentException("Source buffer is too small");
			}
			fixed (byte[] array = src)
			{
				byte* ptr;
				if (src == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				fixed (long* ptr2 = &this.buffer.FixedElementField)
				{
					long* ptr3 = ptr2;
					long* ptr4 = (long*)ptr;
					*ptr3 = *ptr4;
					ptr3[1] = ptr4[1];
				}
			}
		}

		[FixedBuffer(typeof(long), 2)]
		[FieldOffset(0)]
		private GuidManager.GuidBuffer.<buffer>e__FixedBuffer buffer;

		[FieldOffset(0)]
		public Guid Guid;

		[CompilerGenerated]
		[UnsafeValueType]
		[StructLayout(LayoutKind.Sequential, Size = 16)]
		public struct <buffer>e__FixedBuffer
		{
			public long FixedElementField;
		}
	}
}
