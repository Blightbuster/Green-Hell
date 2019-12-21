using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

[Serializable]
public struct P2PNetworkHash128 : IEquatable<P2PNetworkHash128>
{
	public void Reset()
	{
		this.i0 = 0;
		this.i1 = 0;
		this.i2 = 0;
		this.i3 = 0;
		this.i4 = 0;
		this.i5 = 0;
		this.i6 = 0;
		this.i7 = 0;
		this.i8 = 0;
		this.i9 = 0;
		this.i10 = 0;
		this.i11 = 0;
		this.i12 = 0;
		this.i13 = 0;
		this.i14 = 0;
		this.i15 = 0;
	}

	public bool IsValid()
	{
		return (this.i0 | this.i1 | this.i2 | this.i3 | this.i4 | this.i5 | this.i6 | this.i7 | this.i8 | this.i9 | this.i10 | this.i11 | this.i12 | this.i13 | this.i14 | this.i15) > 0;
	}

	private static int HexToNumber(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return (int)(c - '0');
		}
		if (c >= 'a' && c <= 'f')
		{
			return (int)(c - 'a' + '\n');
		}
		if (c >= 'A' && c <= 'F')
		{
			return (int)(c - 'A' + '\n');
		}
		return 0;
	}

	public static P2PNetworkHash128 Parse(string text)
	{
		int length = text.Length;
		if (length < 32)
		{
			string str = "";
			for (int i = 0; i < 32 - length; i++)
			{
				str += "0";
			}
			text = str + text;
		}
		P2PNetworkHash128 result;
		result.i0 = (byte)(P2PNetworkHash128.HexToNumber(text[0]) * 16 + P2PNetworkHash128.HexToNumber(text[1]));
		result.i1 = (byte)(P2PNetworkHash128.HexToNumber(text[2]) * 16 + P2PNetworkHash128.HexToNumber(text[3]));
		result.i2 = (byte)(P2PNetworkHash128.HexToNumber(text[4]) * 16 + P2PNetworkHash128.HexToNumber(text[5]));
		result.i3 = (byte)(P2PNetworkHash128.HexToNumber(text[6]) * 16 + P2PNetworkHash128.HexToNumber(text[7]));
		result.i4 = (byte)(P2PNetworkHash128.HexToNumber(text[8]) * 16 + P2PNetworkHash128.HexToNumber(text[9]));
		result.i5 = (byte)(P2PNetworkHash128.HexToNumber(text[10]) * 16 + P2PNetworkHash128.HexToNumber(text[11]));
		result.i6 = (byte)(P2PNetworkHash128.HexToNumber(text[12]) * 16 + P2PNetworkHash128.HexToNumber(text[13]));
		result.i7 = (byte)(P2PNetworkHash128.HexToNumber(text[14]) * 16 + P2PNetworkHash128.HexToNumber(text[15]));
		result.i8 = (byte)(P2PNetworkHash128.HexToNumber(text[16]) * 16 + P2PNetworkHash128.HexToNumber(text[17]));
		result.i9 = (byte)(P2PNetworkHash128.HexToNumber(text[18]) * 16 + P2PNetworkHash128.HexToNumber(text[19]));
		result.i10 = (byte)(P2PNetworkHash128.HexToNumber(text[20]) * 16 + P2PNetworkHash128.HexToNumber(text[21]));
		result.i11 = (byte)(P2PNetworkHash128.HexToNumber(text[22]) * 16 + P2PNetworkHash128.HexToNumber(text[23]));
		result.i12 = (byte)(P2PNetworkHash128.HexToNumber(text[24]) * 16 + P2PNetworkHash128.HexToNumber(text[25]));
		result.i13 = (byte)(P2PNetworkHash128.HexToNumber(text[26]) * 16 + P2PNetworkHash128.HexToNumber(text[27]));
		result.i14 = (byte)(P2PNetworkHash128.HexToNumber(text[28]) * 16 + P2PNetworkHash128.HexToNumber(text[29]));
		result.i15 = (byte)(P2PNetworkHash128.HexToNumber(text[30]) * 16 + P2PNetworkHash128.HexToNumber(text[31]));
		return result;
	}

	public static P2PNetworkHash128 FromPath(string path)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(path);
		byte[] array = MD5.Create().ComputeHash(bytes);
		P2PNetworkHash128 result;
		if (array.Length != 16)
		{
			Debug.LogError("Invalid hash length - it must be 16 bytes!");
			result.i0 = (result.i1 = (result.i2 = (result.i3 = (result.i4 = (result.i5 = (result.i6 = (result.i7 = (result.i8 = (result.i9 = (result.i10 = (result.i11 = (result.i12 = (result.i13 = (result.i14 = (result.i15 = 0)))))))))))))));
		}
		else
		{
			result.i0 = array[0];
			result.i1 = array[1];
			result.i2 = array[2];
			result.i3 = array[3];
			result.i4 = array[4];
			result.i5 = array[5];
			result.i6 = array[6];
			result.i7 = array[7];
			result.i8 = array[8];
			result.i9 = array[9];
			result.i10 = array[10];
			result.i11 = array[11];
			result.i12 = array[12];
			result.i13 = array[13];
			result.i14 = array[14];
			result.i15 = array[15];
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("{0:x2}{1:x2}{2:x2}{3:x2}{4:x2}{5:x2}{6:x2}{7:x2}{8:x2}{9:x2}{10:x2}{11:x2}{12:x2}{13:x2}{14:x2}{15:x2}", new object[]
		{
			this.i0,
			this.i1,
			this.i2,
			this.i3,
			this.i4,
			this.i5,
			this.i6,
			this.i7,
			this.i8,
			this.i9,
			this.i10,
			this.i11,
			this.i12,
			this.i13,
			this.i14,
			this.i15
		});
	}

	public bool Equals(P2PNetworkHash128 other)
	{
		return this.i0 == other.i0 && this.i1 == other.i1 && this.i2 == other.i2 && this.i3 == other.i3 && this.i4 == other.i4 && this.i5 == other.i5 && this.i6 == other.i6 && this.i7 == other.i7 && this.i8 == other.i8 && this.i9 == other.i9 && this.i10 == other.i10 && this.i11 == other.i11 && this.i12 == other.i12 && this.i13 == other.i13 && this.i14 == other.i14 && this.i15 == other.i15;
	}

	public byte i0;

	public byte i1;

	public byte i2;

	public byte i3;

	public byte i4;

	public byte i5;

	public byte i6;

	public byte i7;

	public byte i8;

	public byte i9;

	public byte i10;

	public byte i11;

	public byte i12;

	public byte i13;

	public byte i14;

	public byte i15;
}
