using System;
using System.IO;
using UnityEngine;

namespace MiniEXR
{
	public static class MiniEXR
	{
		private static void MiniEXRWrite(string _filePath, uint _width, uint _height, uint _channels, float[] _rgbaArray)
		{
			File.WriteAllBytes(_filePath, MiniEXR.MiniEXRWrite(_width, _height, _channels, _rgbaArray));
		}

		public static void MiniEXRWrite(string _filePath, uint _width, uint _height, Color[] _colorArray)
		{
			File.WriteAllBytes(_filePath, MiniEXR.MiniEXRWrite(_width, _height, _colorArray));
		}

		private static byte[] MiniEXRWrite(uint _width, uint _height, Color[] _colorArray)
		{
			float[] array = new float[_colorArray.Length * 3];
			for (int i = 0; i < _colorArray.Length; i++)
			{
				array[i * 3] = _colorArray[i].r;
				array[i * 3 + 1] = _colorArray[i].g;
				array[i * 3 + 2] = _colorArray[i].b;
			}
			return MiniEXR.MiniEXRWrite(_width, _height, 3u, array);
		}

		private static byte[] MiniEXRWrite(uint _width, uint _height, uint _channels, float[] _rgbaArray)
		{
			uint num = _width - 1u;
			uint num2 = _height - 1u;
			byte[] array = new byte[]
			{
				118,
				47,
				49,
				1,
				2,
				0,
				0,
				0,
				99,
				104,
				97,
				110,
				110,
				101,
				108,
				115,
				0,
				99,
				104,
				108,
				105,
				115,
				116,
				0,
				55,
				0,
				0,
				0,
				66,
				0,
				1,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				1,
				0,
				0,
				0,
				1,
				0,
				0,
				0,
				71,
				0,
				1,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				1,
				0,
				0,
				0,
				1,
				0,
				0,
				0,
				82,
				0,
				1,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				1,
				0,
				0,
				0,
				1,
				0,
				0,
				0,
				0,
				99,
				111,
				109,
				112,
				114,
				101,
				115,
				115,
				105,
				111,
				110,
				0,
				99,
				111,
				109,
				112,
				114,
				101,
				115,
				115,
				105,
				111,
				110,
				0,
				1,
				0,
				0,
				0,
				0,
				100,
				97,
				116,
				97,
				87,
				105,
				110,
				100,
				111,
				119,
				0,
				98,
				111,
				120,
				50,
				105,
				0,
				16,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				100,
				105,
				115,
				112,
				108,
				97,
				121,
				87,
				105,
				110,
				100,
				111,
				119,
				0,
				98,
				111,
				120,
				50,
				105,
				0,
				16,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				108,
				105,
				110,
				101,
				79,
				114,
				100,
				101,
				114,
				0,
				108,
				105,
				110,
				101,
				79,
				114,
				100,
				101,
				114,
				0,
				1,
				0,
				0,
				0,
				0,
				112,
				105,
				120,
				101,
				108,
				65,
				115,
				112,
				101,
				99,
				116,
				82,
				97,
				116,
				105,
				111,
				0,
				102,
				108,
				111,
				97,
				116,
				0,
				4,
				0,
				0,
				0,
				0,
				0,
				128,
				63,
				115,
				99,
				114,
				101,
				101,
				110,
				87,
				105,
				110,
				100,
				111,
				119,
				67,
				101,
				110,
				116,
				101,
				114,
				0,
				118,
				50,
				102,
				0,
				8,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				115,
				99,
				114,
				101,
				101,
				110,
				87,
				105,
				110,
				100,
				111,
				119,
				87,
				105,
				100,
				116,
				104,
				0,
				102,
				108,
				111,
				97,
				116,
				0,
				4,
				0,
				0,
				0,
				0,
				0,
				128,
				63,
				0
			};
			array[141] = (byte)(num & 255u);
			array[142] = (byte)(num >> 8 & 255u);
			array[143] = (byte)(num >> 16 & 255u);
			array[144] = (byte)(num >> 24 & 255u);
			array[145] = (byte)(num2 & 255u);
			array[146] = (byte)(num2 >> 8 & 255u);
			array[147] = (byte)(num2 >> 16 & 255u);
			array[148] = (byte)(num2 >> 24 & 255u);
			array[181] = (byte)(num & 255u);
			array[182] = (byte)(num >> 8 & 255u);
			array[183] = (byte)(num >> 16 & 255u);
			array[184] = (byte)(num >> 24 & 255u);
			array[185] = (byte)(num2 & 255u);
			array[186] = (byte)(num2 >> 8 & 255u);
			array[187] = (byte)(num2 >> 16 & 255u);
			array[188] = (byte)(num2 >> 24 & 255u);
			byte[] array2 = array;
			uint num3 = (uint)array2.Length;
			uint num4 = 8u * _height;
			uint num5 = _width * 3u * 2u;
			uint num6 = num5 + 8u;
			byte[] array3 = new byte[num3 + num4 + _height * num6];
			int num7 = 0;
			int num8 = 0;
			while ((long)num8 < (long)((ulong)num3))
			{
				array3[num7] = array2[num8];
				num7++;
				num8++;
			}
			uint num9 = num3 + num4;
			int num10 = 0;
			while ((long)num10 < (long)((ulong)_height))
			{
				array3[num7++] = (byte)(num9 & 255u);
				array3[num7++] = (byte)(num9 >> 8 & 255u);
				array3[num7++] = (byte)(num9 >> 16 & 255u);
				array3[num7++] = (byte)(num9 >> 24 & 255u);
				array3[num7++] = 0;
				array3[num7++] = 0;
				array3[num7++] = 0;
				array3[num7++] = 0;
				num9 += num6;
				num10++;
			}
			ushort[] array4 = new ushort[_rgbaArray.Length];
			for (int i = 0; i < _rgbaArray.Length; i++)
			{
				_rgbaArray[i] = Mathf.Pow(_rgbaArray[i], 2.2f);
				array4[i] = HalfHelper.SingleToHalf(_rgbaArray[i]);
			}
			uint num11 = 0u;
			int num12 = 0;
			while ((long)num12 < (long)((ulong)_height))
			{
				array3[num7++] = (byte)(num12 & 255);
				array3[num7++] = (byte)(num12 >> 8 & 255);
				array3[num7++] = (byte)(num12 >> 16 & 255);
				array3[num7++] = (byte)(num12 >> 24 & 255);
				array3[num7++] = (byte)(num5 & 255u);
				array3[num7++] = (byte)(num5 >> 8 & 255u);
				array3[num7++] = (byte)(num5 >> 16 & 255u);
				array3[num7++] = (byte)(num5 >> 24 & 255u);
				uint num13 = num11;
				int num14 = 0;
				while ((long)num14 < (long)((ulong)_width))
				{
					byte[] bytes = BitConverter.GetBytes(array4[(int)(num13 + 2u)]);
					array3[num7++] = bytes[0];
					array3[num7++] = bytes[1];
					num13 += _channels;
					num14++;
				}
				num13 = num11;
				int num15 = 0;
				while ((long)num15 < (long)((ulong)_width))
				{
					byte[] bytes2 = BitConverter.GetBytes(array4[(int)(num13 + 1u)]);
					array3[num7++] = bytes2[0];
					array3[num7++] = bytes2[1];
					num13 += _channels;
					num15++;
				}
				num13 = num11;
				int num16 = 0;
				while ((long)num16 < (long)((ulong)_width))
				{
					byte[] bytes3 = BitConverter.GetBytes(array4[(int)num13]);
					array3[num7++] = bytes3[0];
					array3[num7++] = bytes3[1];
					num13 += _channels;
					num16++;
				}
				num11 += _width * _channels;
				num12++;
			}
			return array3;
		}
	}
}
