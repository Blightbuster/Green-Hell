using System;
using System.Text;
using UnityEngine;

public class P2PNetworkWriter
{
	public P2PNetworkWriter()
	{
		this.m_Buffer = new P2PNetBuffer();
		if (P2PNetworkWriter.s_Encoding == null)
		{
			P2PNetworkWriter.s_Encoding = new UTF8Encoding();
			P2PNetworkWriter.s_StringWriteBuffer = new byte[32768];
		}
	}

	public P2PNetworkWriter(byte[] buffer)
	{
		this.m_Buffer = new P2PNetBuffer(buffer);
		if (P2PNetworkWriter.s_Encoding == null)
		{
			P2PNetworkWriter.s_Encoding = new UTF8Encoding();
			P2PNetworkWriter.s_StringWriteBuffer = new byte[32768];
		}
	}

	public short Position
	{
		get
		{
			return (short)this.m_Buffer.Position;
		}
	}

	public byte[] ToArray()
	{
		byte[] array = new byte[this.m_Buffer.AsArraySegment().Count];
		Array.Copy(this.m_Buffer.AsArraySegment().Array, array, this.m_Buffer.AsArraySegment().Count);
		return array;
	}

	public byte[] AsArray()
	{
		return this.AsArraySegment().Array;
	}

	internal ArraySegment<byte> AsArraySegment()
	{
		return this.m_Buffer.AsArraySegment();
	}

	public void WritePackedUInt32(uint value)
	{
		if (value <= 240u)
		{
			this.Write((byte)value);
			return;
		}
		if (value <= 2287u)
		{
			this.Write((byte)((value - 240u) / 256u + 241u));
			this.Write((byte)((value - 240u) % 256u));
			return;
		}
		if (value <= 67823u)
		{
			this.Write(249);
			this.Write((byte)((value - 2288u) / 256u));
			this.Write((byte)((value - 2288u) % 256u));
			return;
		}
		if (value <= 16777215u)
		{
			this.Write(250);
			this.Write((byte)(value & 255u));
			this.Write((byte)(value >> 8 & 255u));
			this.Write((byte)(value >> 16 & 255u));
			return;
		}
		this.Write(251);
		this.Write((byte)(value & 255u));
		this.Write((byte)(value >> 8 & 255u));
		this.Write((byte)(value >> 16 & 255u));
		this.Write((byte)(value >> 24 & 255u));
	}

	public void WritePackedUInt64(ulong value)
	{
		if (value <= 240UL)
		{
			this.Write((byte)value);
			return;
		}
		if (value <= 2287UL)
		{
			this.Write((byte)((value - 240UL) / 256UL + 241UL));
			this.Write((byte)((value - 240UL) % 256UL));
			return;
		}
		if (value <= 67823UL)
		{
			this.Write(249);
			this.Write((byte)((value - 2288UL) / 256UL));
			this.Write((byte)((value - 2288UL) % 256UL));
			return;
		}
		if (value <= 16777215UL)
		{
			this.Write(250);
			this.Write((byte)(value & 255UL));
			this.Write((byte)(value >> 8 & 255UL));
			this.Write((byte)(value >> 16 & 255UL));
			return;
		}
		if (value <= (ulong)-1)
		{
			this.Write(251);
			this.Write((byte)(value & 255UL));
			this.Write((byte)(value >> 8 & 255UL));
			this.Write((byte)(value >> 16 & 255UL));
			this.Write((byte)(value >> 24 & 255UL));
			return;
		}
		if (value <= 1099511627775UL)
		{
			this.Write(252);
			this.Write((byte)(value & 255UL));
			this.Write((byte)(value >> 8 & 255UL));
			this.Write((byte)(value >> 16 & 255UL));
			this.Write((byte)(value >> 24 & 255UL));
			this.Write((byte)(value >> 32 & 255UL));
			return;
		}
		if (value <= 281474976710655UL)
		{
			this.Write(253);
			this.Write((byte)(value & 255UL));
			this.Write((byte)(value >> 8 & 255UL));
			this.Write((byte)(value >> 16 & 255UL));
			this.Write((byte)(value >> 24 & 255UL));
			this.Write((byte)(value >> 32 & 255UL));
			this.Write((byte)(value >> 40 & 255UL));
			return;
		}
		if (value <= 72057594037927935UL)
		{
			this.Write(254);
			this.Write((byte)(value & 255UL));
			this.Write((byte)(value >> 8 & 255UL));
			this.Write((byte)(value >> 16 & 255UL));
			this.Write((byte)(value >> 24 & 255UL));
			this.Write((byte)(value >> 32 & 255UL));
			this.Write((byte)(value >> 40 & 255UL));
			this.Write((byte)(value >> 48 & 255UL));
			return;
		}
		this.Write(byte.MaxValue);
		this.Write((byte)(value & 255UL));
		this.Write((byte)(value >> 8 & 255UL));
		this.Write((byte)(value >> 16 & 255UL));
		this.Write((byte)(value >> 24 & 255UL));
		this.Write((byte)(value >> 32 & 255UL));
		this.Write((byte)(value >> 40 & 255UL));
		this.Write((byte)(value >> 48 & 255UL));
		this.Write((byte)(value >> 56 & 255UL));
	}

	public void Write(char value)
	{
		this.m_Buffer.WriteByte((byte)value);
	}

	public void Write(byte value)
	{
		this.m_Buffer.WriteByte(value);
	}

	public void Write(sbyte value)
	{
		this.m_Buffer.WriteByte((byte)value);
	}

	public void Write(short value)
	{
		this.m_Buffer.WriteByte2((byte)(value & 255), (byte)(value >> 8 & 255));
	}

	public void Write(ushort value)
	{
		this.m_Buffer.WriteByte2((byte)(value & 255), (byte)(value >> 8 & 255));
	}

	public void Write(int value)
	{
		this.m_Buffer.WriteByte4((byte)(value & 255), (byte)(value >> 8 & 255), (byte)(value >> 16 & 255), (byte)(value >> 24 & 255));
	}

	public void Write(uint value)
	{
		this.m_Buffer.WriteByte4((byte)(value & 255u), (byte)(value >> 8 & 255u), (byte)(value >> 16 & 255u), (byte)(value >> 24 & 255u));
	}

	public void Write(long value)
	{
		this.m_Buffer.WriteByte8((byte)(value & 255L), (byte)(value >> 8 & 255L), (byte)(value >> 16 & 255L), (byte)(value >> 24 & 255L), (byte)(value >> 32 & 255L), (byte)(value >> 40 & 255L), (byte)(value >> 48 & 255L), (byte)(value >> 56 & 255L));
	}

	public void Write(ulong value)
	{
		this.m_Buffer.WriteByte8((byte)(value & 255UL), (byte)(value >> 8 & 255UL), (byte)(value >> 16 & 255UL), (byte)(value >> 24 & 255UL), (byte)(value >> 32 & 255UL), (byte)(value >> 40 & 255UL), (byte)(value >> 48 & 255UL), (byte)(value >> 56 & 255UL));
	}

	public void Write(float value)
	{
		P2PNetworkWriter.s_FloatConverter.floatValue = value;
		this.Write(P2PNetworkWriter.s_FloatConverter.intValue);
	}

	public void Write(double value)
	{
		P2PNetworkWriter.s_FloatConverter.doubleValue = value;
		this.Write(P2PNetworkWriter.s_FloatConverter.longValue);
	}

	public void Write(string value)
	{
		if (value == null)
		{
			this.m_Buffer.WriteByte2(0, 0);
			return;
		}
		int byteCount = P2PNetworkWriter.s_Encoding.GetByteCount(value);
		if (byteCount >= 32768)
		{
			throw new IndexOutOfRangeException("Serialize(string) too long: " + value.Length);
		}
		this.Write((ushort)byteCount);
		int bytes = P2PNetworkWriter.s_Encoding.GetBytes(value, 0, value.Length, P2PNetworkWriter.s_StringWriteBuffer, 0);
		this.m_Buffer.WriteBytes(P2PNetworkWriter.s_StringWriteBuffer, (ushort)bytes);
	}

	public void Write(bool value)
	{
		if (value)
		{
			this.m_Buffer.WriteByte(1);
			return;
		}
		this.m_Buffer.WriteByte(0);
	}

	public void Write(byte[] buffer, int count)
	{
		if (count > 65535)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("NetworkWriter Write: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes.");
			}
			return;
		}
		this.m_Buffer.WriteBytes(buffer, (ushort)count);
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		if (count > 65535)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("NetworkWriter Write: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes.");
			}
			return;
		}
		this.m_Buffer.WriteBytesAtOffset(buffer, (ushort)offset, (ushort)count);
	}

	public void WriteBytesAndSize(byte[] buffer, int count)
	{
		if (buffer == null || count == 0)
		{
			this.Write(0);
			return;
		}
		if (count > 65535)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("NetworkWriter WriteBytesAndSize: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes.");
			}
			return;
		}
		this.Write((ushort)count);
		this.m_Buffer.WriteBytes(buffer, (ushort)count);
	}

	public void WriteBytesFull(byte[] buffer)
	{
		if (buffer == null)
		{
			this.Write(0);
			return;
		}
		if (buffer.Length > 65535)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("NetworkWriter WriteBytes: buffer is too large (" + buffer.Length + ") bytes. The maximum buffer size is 64K bytes.");
			}
			return;
		}
		this.Write((ushort)buffer.Length);
		this.m_Buffer.WriteBytes(buffer, (ushort)buffer.Length);
	}

	public void Write(Vector2 value)
	{
		this.Write(value.x);
		this.Write(value.y);
	}

	public void Write(Vector3 value)
	{
		this.Write(value.x);
		this.Write(value.y);
		this.Write(value.z);
	}

	public void Write(Vector4 value)
	{
		this.Write(value.x);
		this.Write(value.y);
		this.Write(value.z);
		this.Write(value.w);
	}

	public void Write(Color value)
	{
		this.Write(value.r);
		this.Write(value.g);
		this.Write(value.b);
		this.Write(value.a);
	}

	public void Write(Color32 value)
	{
		this.Write(value.r);
		this.Write(value.g);
		this.Write(value.b);
		this.Write(value.a);
	}

	public void Write(Quaternion value)
	{
		this.Write(value.x);
		this.Write(value.y);
		this.Write(value.z);
		this.Write(value.w);
	}

	public void Write(Rect value)
	{
		this.Write(value.xMin);
		this.Write(value.yMin);
		this.Write(value.width);
		this.Write(value.height);
	}

	public void Write(Plane value)
	{
		this.Write(value.normal);
		this.Write(value.distance);
	}

	public void Write(Ray value)
	{
		this.Write(value.direction);
		this.Write(value.origin);
	}

	public void Write(Matrix4x4 value)
	{
		this.Write(value.m00);
		this.Write(value.m01);
		this.Write(value.m02);
		this.Write(value.m03);
		this.Write(value.m10);
		this.Write(value.m11);
		this.Write(value.m12);
		this.Write(value.m13);
		this.Write(value.m20);
		this.Write(value.m21);
		this.Write(value.m22);
		this.Write(value.m23);
		this.Write(value.m30);
		this.Write(value.m31);
		this.Write(value.m32);
		this.Write(value.m33);
	}

	public void Write(P2PNetworkHash128 value)
	{
		this.Write(value.i0);
		this.Write(value.i1);
		this.Write(value.i2);
		this.Write(value.i3);
		this.Write(value.i4);
		this.Write(value.i5);
		this.Write(value.i6);
		this.Write(value.i7);
		this.Write(value.i8);
		this.Write(value.i9);
		this.Write(value.i10);
		this.Write(value.i11);
		this.Write(value.i12);
		this.Write(value.i13);
		this.Write(value.i14);
		this.Write(value.i15);
	}

	public void Write(Transform value)
	{
		if (value == null || value.gameObject == null)
		{
			this.WritePackedUInt32(0u);
			return;
		}
		GuidComponent component = value.gameObject.GetComponent<GuidComponent>();
		if (component != null)
		{
			this.Write(component);
			return;
		}
		if (P2PLogFilter.logWarn)
		{
			Debug.LogWarning("NetworkWriter " + value + " has no GuidComponent");
		}
		this.WritePackedUInt32(0u);
	}

	public void Write(GameObject value)
	{
		if (value == null)
		{
			this.Write(P2PNetworkWriter.EMTY_GUID_BYTES, GuidComponent.GUID_BYTES_CNT);
			return;
		}
		GuidComponent component = value.GetComponent<GuidComponent>();
		if (component == null)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("P2PNetworkWriter writing GameObject " + value.name + " without GuidComponent");
			}
			this.Write(P2PNetworkWriter.EMTY_GUID_BYTES, GuidComponent.GUID_BYTES_CNT);
			return;
		}
		this.Write(component.GetGuidBytes(), GuidComponent.GUID_BYTES_CNT);
	}

	[Obsolete("Deprecated due to allocation, preferably use Write(GameObject) or Write(byte[], int)")]
	public void Write(Guid guid)
	{
		this.Write(guid.ToByteArray(), GuidComponent.GUID_BYTES_CNT);
	}

	public void Write(P2PPeer peer)
	{
		this.Write(peer.GetHostId());
		this.Write(peer.m_Address);
	}

	public void Write(IP2PAddress addr)
	{
		ETransporLayerType layerType = P2PTransportLayer.Instance.GetLayerType();
		if (layerType == ETransporLayerType.UNet)
		{
			P2PAddressUnet p2PAddressUnet = addr as P2PAddressUnet;
			this.Write(p2PAddressUnet.m_IP);
			this.Write(p2PAddressUnet.m_Port);
			return;
		}
		if (layerType != ETransporLayerType.Steam)
		{
			throw new NotImplementedException();
		}
		P2PAddressSteam p2PAddressSteam = addr as P2PAddressSteam;
		this.Write(p2PAddressSteam.m_SteamID.m_SteamID);
	}

	public void Write(P2PMessageBase msg)
	{
		msg.Serialize(this);
	}

	public void Write(bool[] array)
	{
		this.WritePackedUInt32((uint)array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			this.Write(array[i]);
		}
	}

	public void Write(int[] array)
	{
		this.WritePackedUInt32((uint)array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			this.Write(array[i]);
		}
	}

	public void SeekZero()
	{
		this.m_Buffer.SeekZero();
	}

	public void Seek(int offset)
	{
		this.m_Buffer.Seek(offset);
	}

	public void StartMessage(short msgType)
	{
		if (this.m_Lock)
		{
			DebugUtils.Assert("Starting new message on locked buffer!!!", true, DebugUtils.AssertType.FatalError);
		}
		this.m_Lock = true;
		this.SeekZero();
		this.m_Buffer.WriteByte2(0, 0);
		this.Write(msgType);
	}

	public void FinishMessage()
	{
		this.m_Buffer.FinishMessage();
		this.m_Lock = false;
	}

	private const int k_MaxStringLength = 32768;

	private P2PNetBuffer m_Buffer;

	private static Encoding s_Encoding;

	private static byte[] s_StringWriteBuffer;

	private bool m_Lock;

	private static UIntFloat s_FloatConverter;

	public static readonly byte[] EMTY_GUID_BYTES = Guid.Empty.ToByteArray();
}
