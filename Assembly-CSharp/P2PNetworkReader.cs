using System;
using System.Text;
using Steamworks;
using UnityEngine;

public class P2PNetworkReader
{
	public P2PNetworkReader()
	{
		this.m_buf = new P2PNetBuffer();
		P2PNetworkReader.Initialize();
	}

	public P2PNetworkReader(P2PNetworkWriter writer)
	{
		this.m_buf = new P2PNetBuffer(writer.AsArray());
		P2PNetworkReader.Initialize();
	}

	public P2PNetworkReader(byte[] buffer)
	{
		this.m_buf = new P2PNetBuffer(buffer);
		P2PNetworkReader.Initialize();
	}

	private static void Initialize()
	{
		if (P2PNetworkReader.s_Encoding == null)
		{
			P2PNetworkReader.s_StringReaderBuffer = new byte[1024];
			P2PNetworkReader.s_GuidReaderBuffer = new byte[1024];
			P2PNetworkReader.s_Encoding = new UTF8Encoding();
		}
	}

	public uint Position
	{
		get
		{
			return this.m_buf.Position;
		}
	}

	public void SeekZero()
	{
		this.m_buf.SeekZero();
	}

	public void Seek(int offset)
	{
		this.m_buf.Seek(offset);
	}

	internal byte[] Replace(byte[] buffer)
	{
		return this.m_buf.Replace(buffer);
	}

	public uint ReadPackedUInt32()
	{
		byte b = this.ReadByte();
		if (b < 241)
		{
			return (uint)b;
		}
		byte b2 = this.ReadByte();
		if (b >= 241 && b <= 248)
		{
			return 240u + 256u * (uint)(b - 241) + (uint)b2;
		}
		byte b3 = this.ReadByte();
		if (b == 249)
		{
			return 2288u + 256u * (uint)b2 + (uint)b3;
		}
		byte b4 = this.ReadByte();
		if (b == 250)
		{
			return (uint)((int)b2 + ((int)b3 << 8) + ((int)b4 << 16));
		}
		byte b5 = this.ReadByte();
		if (b >= 251)
		{
			return (uint)((int)b2 + ((int)b3 << 8) + ((int)b4 << 16) + ((int)b5 << 24));
		}
		throw new IndexOutOfRangeException("ReadPackedUInt32() failure: " + b);
	}

	public ulong ReadPackedUInt64()
	{
		byte b = this.ReadByte();
		if (b < 241)
		{
			return (ulong)b;
		}
		byte b2 = this.ReadByte();
		if (b >= 241 && b <= 248)
		{
			return 240UL + 256UL * ((ulong)b - 241UL) + (ulong)b2;
		}
		byte b3 = this.ReadByte();
		if (b == 249)
		{
			return 2288UL + 256UL * (ulong)b2 + (ulong)b3;
		}
		byte b4 = this.ReadByte();
		if (b == 250)
		{
			return (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16);
		}
		byte b5 = this.ReadByte();
		if (b == 251)
		{
			return (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24);
		}
		byte b6 = this.ReadByte();
		if (b == 252)
		{
			return (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32);
		}
		byte b7 = this.ReadByte();
		if (b == 253)
		{
			return (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40);
		}
		byte b8 = this.ReadByte();
		if (b == 254)
		{
			return (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48);
		}
		byte b9 = this.ReadByte();
		if (b == 255)
		{
			return (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48) + ((ulong)b9 << 56);
		}
		throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + b);
	}

	public byte ReadByte()
	{
		return this.m_buf.ReadByte();
	}

	public sbyte ReadSByte()
	{
		return (sbyte)this.m_buf.ReadByte();
	}

	public short ReadInt16()
	{
		return (short)((ushort)(0 | this.m_buf.ReadByte()) | (ushort)(this.m_buf.ReadByte() << 8));
	}

	public ushort ReadUInt16()
	{
		return (ushort)(0 | this.m_buf.ReadByte()) | (ushort)(this.m_buf.ReadByte() << 8);
	}

	public int ReadInt32()
	{
		return (int)(0 | this.m_buf.ReadByte()) | (int)this.m_buf.ReadByte() << 8 | (int)this.m_buf.ReadByte() << 16 | (int)this.m_buf.ReadByte() << 24;
	}

	public uint ReadUInt32()
	{
		return (uint)((int)(0 | this.m_buf.ReadByte()) | (int)this.m_buf.ReadByte() << 8 | (int)this.m_buf.ReadByte() << 16 | (int)this.m_buf.ReadByte() << 24);
	}

	public long ReadInt64()
	{
		long num = 0L;
		ulong num2 = (ulong)this.m_buf.ReadByte();
		long num3 = num | (long)num2;
		num2 = (ulong)this.m_buf.ReadByte() << 8;
		long num4 = num3 | (long)num2;
		num2 = (ulong)this.m_buf.ReadByte() << 16;
		long num5 = num4 | (long)num2;
		num2 = (ulong)this.m_buf.ReadByte() << 24;
		long num6 = num5 | (long)num2;
		num2 = (ulong)this.m_buf.ReadByte() << 32;
		long num7 = num6 | (long)num2;
		num2 = (ulong)this.m_buf.ReadByte() << 40;
		long num8 = num7 | (long)num2;
		num2 = (ulong)this.m_buf.ReadByte() << 48;
		long num9 = num8 | (long)num2;
		num2 = (ulong)this.m_buf.ReadByte() << 56;
		return num9 | (long)num2;
	}

	public ulong ReadUInt64()
	{
		ulong num = 0UL;
		ulong num2 = (ulong)this.m_buf.ReadByte();
		ulong num3 = num | num2;
		num2 = (ulong)this.m_buf.ReadByte() << 8;
		ulong num4 = num3 | num2;
		num2 = (ulong)this.m_buf.ReadByte() << 16;
		ulong num5 = num4 | num2;
		num2 = (ulong)this.m_buf.ReadByte() << 24;
		ulong num6 = num5 | num2;
		num2 = (ulong)this.m_buf.ReadByte() << 32;
		ulong num7 = num6 | num2;
		num2 = (ulong)this.m_buf.ReadByte() << 40;
		ulong num8 = num7 | num2;
		num2 = (ulong)this.m_buf.ReadByte() << 48;
		ulong num9 = num8 | num2;
		num2 = (ulong)this.m_buf.ReadByte() << 56;
		return num9 | num2;
	}

	public float ReadFloat()
	{
		return FloatConversion.ToSingle(this.ReadUInt32());
	}

	public double ReadDouble()
	{
		return FloatConversion.ToDouble(this.ReadUInt64());
	}

	public string ReadString()
	{
		ushort num = this.ReadUInt16();
		if (num == 0)
		{
			return "";
		}
		if (num >= 32768)
		{
			throw new IndexOutOfRangeException("ReadString() too long: " + num);
		}
		while ((int)num > P2PNetworkReader.s_StringReaderBuffer.Length)
		{
			P2PNetworkReader.s_StringReaderBuffer = new byte[P2PNetworkReader.s_StringReaderBuffer.Length * 2];
		}
		this.m_buf.ReadBytes(P2PNetworkReader.s_StringReaderBuffer, (uint)num);
		return new string(P2PNetworkReader.s_Encoding.GetChars(P2PNetworkReader.s_StringReaderBuffer, 0, (int)num));
	}

	public char ReadChar()
	{
		return (char)this.m_buf.ReadByte();
	}

	public bool ReadBoolean()
	{
		return this.m_buf.ReadByte() == 1;
	}

	public byte[] ReadBytes(int count)
	{
		if (count < 0)
		{
			throw new IndexOutOfRangeException("NetworkReader ReadBytes " + count);
		}
		byte[] array = new byte[count];
		this.m_buf.ReadBytes(array, (uint)count);
		return array;
	}

	public byte[] ReadBytesAndSize()
	{
		ushort num = this.ReadUInt16();
		if (num == 0)
		{
			return null;
		}
		return this.ReadBytes((int)num);
	}

	public Vector2 ReadVector2()
	{
		return new Vector2(this.ReadFloat(), this.ReadFloat());
	}

	public Vector3 ReadVector3()
	{
		return new Vector3(this.ReadFloat(), this.ReadFloat(), this.ReadFloat());
	}

	public Vector4 ReadVector4()
	{
		return new Vector4(this.ReadFloat(), this.ReadFloat(), this.ReadFloat(), this.ReadFloat());
	}

	public Color ReadColor()
	{
		return new Color(this.ReadFloat(), this.ReadFloat(), this.ReadFloat(), this.ReadFloat());
	}

	public Color32 ReadColor32()
	{
		return new Color32(this.ReadByte(), this.ReadByte(), this.ReadByte(), this.ReadByte());
	}

	public Quaternion ReadQuaternion()
	{
		return new Quaternion(this.ReadFloat(), this.ReadFloat(), this.ReadFloat(), this.ReadFloat());
	}

	public Rect ReadRect()
	{
		return new Rect(this.ReadFloat(), this.ReadFloat(), this.ReadFloat(), this.ReadFloat());
	}

	public Plane ReadPlane()
	{
		return new Plane(this.ReadVector3(), this.ReadFloat());
	}

	public Ray ReadRay()
	{
		return new Ray(this.ReadVector3(), this.ReadVector3());
	}

	public Matrix4x4 ReadMatrix4x4()
	{
		return new Matrix4x4
		{
			m00 = this.ReadFloat(),
			m01 = this.ReadFloat(),
			m02 = this.ReadFloat(),
			m03 = this.ReadFloat(),
			m10 = this.ReadFloat(),
			m11 = this.ReadFloat(),
			m12 = this.ReadFloat(),
			m13 = this.ReadFloat(),
			m20 = this.ReadFloat(),
			m21 = this.ReadFloat(),
			m22 = this.ReadFloat(),
			m23 = this.ReadFloat(),
			m30 = this.ReadFloat(),
			m31 = this.ReadFloat(),
			m32 = this.ReadFloat(),
			m33 = this.ReadFloat()
		};
	}

	public P2PNetworkHash128 ReadNetworkHash128()
	{
		P2PNetworkHash128 result;
		result.i0 = this.ReadByte();
		result.i1 = this.ReadByte();
		result.i2 = this.ReadByte();
		result.i3 = this.ReadByte();
		result.i4 = this.ReadByte();
		result.i5 = this.ReadByte();
		result.i6 = this.ReadByte();
		result.i7 = this.ReadByte();
		result.i8 = this.ReadByte();
		result.i9 = this.ReadByte();
		result.i10 = this.ReadByte();
		result.i11 = this.ReadByte();
		result.i12 = this.ReadByte();
		result.i13 = this.ReadByte();
		result.i14 = this.ReadByte();
		result.i15 = this.ReadByte();
		return result;
	}

	public Transform ReadTransform()
	{
		GameObject gameObject = GuidManager.ResolveGuid(this.ReadGuidBytesInternal());
		if (gameObject == null)
		{
			if (P2PLogFilter.logDebug)
			{
				Debug.Log("[ReadTransform] guid not found");
			}
			return null;
		}
		return gameObject.transform;
	}

	public GameObject ReadGameObject()
	{
		GameObject gameObject = GuidManager.ResolveGuid(this.ReadGuidBytesInternal());
		gameObject == null;
		return gameObject;
	}

	public GameObject ReadGameObjectAndGuid(out byte[] guid)
	{
		GameObject gameObject = GuidManager.ResolveGuid(this.ReadGuidBytesInternal());
		if (gameObject == null && P2PLogFilter.logDebug)
		{
			Debug.Log("[ReadGameObject] guid not found!");
		}
		guid = P2PNetworkReader.s_GuidBytesBuffer;
		return gameObject;
	}

	public ReplicatedGameObject ReadReplicatedGameObject()
	{
		return new ReplicatedGameObject(this.ReadGuidBytes());
	}

	[Obsolete("Deprecated due to allocation, preferably use Read(GameObject) or ReadGuidBytes()")]
	public Guid ReadGuid()
	{
		return new Guid(this.ReadBytes(GuidComponent.GUID_BYTES_CNT));
	}

	public byte[] ReadGuidBytesTemporary()
	{
		return this.ReadGuidBytesInternal();
	}

	public byte[] ReadGuidBytes()
	{
		return this.ReadBytes(GuidComponent.GUID_BYTES_CNT);
	}

	private byte[] ReadGuidBytesInternal()
	{
		this.m_buf.ReadBytes(P2PNetworkReader.s_GuidBytesBuffer, (uint)GuidComponent.GUID_BYTES_CNT);
		return P2PNetworkReader.s_GuidBytesBuffer;
	}

	public P2PPeer ReadPeer()
	{
		P2PPeer p2PPeer = new P2PPeer();
		p2PPeer.SetHostId(this.ReadInt16());
		p2PPeer.m_Address = this.ReadPeerAddress();
		return p2PPeer;
	}

	public IP2PAddress ReadPeerAddress()
	{
		ETransporLayerType layerType = P2PTransportLayer.Instance.GetLayerType();
		IP2PAddress result;
		if (layerType != ETransporLayerType.UNet)
		{
			if (layerType != ETransporLayerType.Steam)
			{
				throw new NotImplementedException();
			}
			result = new P2PAddressSteam
			{
				m_SteamID = new CSteamID(this.ReadUInt64())
			};
		}
		else
		{
			result = new P2PAddressUnet
			{
				m_IP = this.ReadString(),
				m_Port = this.ReadInt32()
			};
		}
		return result;
	}

	public bool[] ReadBoolArray()
	{
		uint num = this.ReadPackedUInt32();
		bool[] array = new bool[num];
		int num2 = 0;
		while ((long)num2 < (long)((ulong)num))
		{
			array[num2] = this.ReadBoolean();
			num2++;
		}
		return array;
	}

	public int[] ReadIntArray()
	{
		uint num = this.ReadPackedUInt32();
		int[] array = new int[num];
		int num2 = 0;
		while ((long)num2 < (long)((ulong)num))
		{
			array[num2] = this.ReadInt32();
			num2++;
		}
		return array;
	}

	public override string ToString()
	{
		return this.m_buf.ToString();
	}

	public void SetGuard(uint pos)
	{
		this.m_buf.SetGuard(pos);
	}

	public void RemoveGuard()
	{
		this.m_buf.RemoveGuard();
	}

	public TMsg ReadMessage<TMsg>() where TMsg : P2PMessageBase, new()
	{
		TMsg tmsg = Activator.CreateInstance<TMsg>();
		tmsg.Deserialize(this);
		return tmsg;
	}

	private P2PNetBuffer m_buf;

	private const int k_MaxStringLength = 32768;

	private const int k_InitialStringBufferSize = 1024;

	private const int k_GuidBufferSize = 1024;

	private static byte[] s_StringReaderBuffer;

	private static byte[] s_GuidReaderBuffer;

	private static Encoding s_Encoding;

	private static byte[] s_GuidBytesBuffer = new byte[GuidComponent.GUID_BYTES_CNT];
}
