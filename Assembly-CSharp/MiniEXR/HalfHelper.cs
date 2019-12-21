using System;

namespace MiniEXR
{
	internal static class HalfHelper
	{
		private static uint ConvertMantissa(int i)
		{
			uint num = (uint)((uint)i << 13);
			uint num2 = 0u;
			while ((num & 8388608u) == 0u)
			{
				num2 -= 8388608u;
				num <<= 1;
			}
			num &= 4286578687u;
			num2 += 947912704u;
			return num | num2;
		}

		private static uint[] GenerateMantissaTable()
		{
			uint[] array = new uint[2048];
			array[0] = 0u;
			for (int i = 1; i < 1024; i++)
			{
				array[i] = HalfHelper.ConvertMantissa(i);
			}
			for (int j = 1024; j < 2048; j++)
			{
				array[j] = (uint)(939524096 + (j - 1024 << 13));
			}
			return array;
		}

		private static uint[] GenerateExponentTable()
		{
			uint[] array = new uint[64];
			array[0] = 0u;
			for (int i = 1; i < 31; i++)
			{
				array[i] = (uint)((uint)i << 23);
			}
			array[31] = 1199570944u;
			array[32] = 2147483648u;
			for (int j = 33; j < 63; j++)
			{
				array[j] = (uint)((ulong)int.MinValue + (ulong)((long)((long)(j - 32) << 23)));
			}
			array[63] = 3347054592u;
			return array;
		}

		private static ushort[] GenerateOffsetTable()
		{
			ushort[] array = new ushort[64];
			array[0] = 0;
			for (int i = 1; i < 32; i++)
			{
				array[i] = 1024;
			}
			array[32] = 0;
			for (int j = 33; j < 64; j++)
			{
				array[j] = 1024;
			}
			return array;
		}

		private static ushort[] GenerateBaseTable()
		{
			ushort[] array = new ushort[512];
			for (int i = 0; i < 256; i++)
			{
				sbyte b = (sbyte)(127 - i);
				if (b > 24)
				{
					array[i | 0] = 0;
					array[i | 256] = 32768;
				}
				else if (b > 14)
				{
					array[i | 0] = (ushort)(1024 >> (int)(18 + b));
					array[i | 256] = (ushort)(1024 >> (int)(18 + b) | 32768);
				}
				else if (b >= -15)
				{
					array[i | 0] = (ushort)(15 - b << 10);
					array[i | 256] = (ushort)((int)(15 - b) << 10 | 32768);
				}
				else if (b > -128)
				{
					array[i | 0] = 31744;
					array[i | 256] = 64512;
				}
				else
				{
					array[i | 0] = 31744;
					array[i | 256] = 64512;
				}
			}
			return array;
		}

		private static sbyte[] GenerateShiftTable()
		{
			sbyte[] array = new sbyte[512];
			for (int i = 0; i < 256; i++)
			{
				sbyte b = (sbyte)(127 - i);
				if (b > 24)
				{
					array[i | 0] = 24;
					array[i | 256] = 24;
				}
				else if (b > 14)
				{
					array[i | 0] = b - 1;
					array[i | 256] = b - 1;
				}
				else if (b >= -15)
				{
					array[i | 0] = 13;
					array[i | 256] = 13;
				}
				else if (b > -128)
				{
					array[i | 0] = 24;
					array[i | 256] = 24;
				}
				else
				{
					array[i | 0] = 13;
					array[i | 256] = 13;
				}
			}
			return array;
		}

		public static float HalfToSingle(ushort half)
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(HalfHelper.mantissaTable[(int)(HalfHelper.offsetTable[half >> 10] + (half & 1023))] + HalfHelper.exponentTable[half >> 10]), 0);
		}

		public static ushort SingleToHalf(float single)
		{
			uint num = BitConverter.ToUInt32(BitConverter.GetBytes(single), 0);
			return (ushort)((uint)HalfHelper.baseTable[(int)(num >> 23 & 511u)] + ((num & 8388607u) >> (int)HalfHelper.shiftTable[(int)(num >> 23)]));
		}

		private static uint[] mantissaTable = HalfHelper.GenerateMantissaTable();

		private static uint[] exponentTable = HalfHelper.GenerateExponentTable();

		private static ushort[] offsetTable = HalfHelper.GenerateOffsetTable();

		private static ushort[] baseTable = HalfHelper.GenerateBaseTable();

		private static sbyte[] shiftTable = HalfHelper.GenerateShiftTable();
	}
}
