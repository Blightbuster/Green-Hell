using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace IESLights
{
	public static class ParseIES
	{
		public static IESData Parse(string path, NormalizationMode normalizationMode)
		{
			string[] array = File.ReadAllLines(path);
			int num = 0;
			ParseIES.FindNumberOfAnglesLine(array, ref num);
			if (num == array.Length - 1)
			{
				throw new IESParseException("No line containing number of angles found.");
			}
			int numberOfValuesToFind;
			int num2;
			PhotometricType photometricType;
			ParseIES.ReadProperties(array, ref num, out numberOfValuesToFind, out num2, out photometricType);
			List<float> verticalAngles = ParseIES.ReadValues(array, numberOfValuesToFind, ref num);
			List<float> horizontalAngles = ParseIES.ReadValues(array, num2, ref num);
			List<List<float>> list = new List<List<float>>();
			for (int i = 0; i < num2; i++)
			{
				list.Add(ParseIES.ReadValues(array, numberOfValuesToFind, ref num));
			}
			IESData iesdata = new IESData
			{
				VerticalAngles = verticalAngles,
				HorizontalAngles = horizontalAngles,
				CandelaValues = list,
				PhotometricType = photometricType
			};
			ParseIES.NormalizeValues(iesdata, normalizationMode == NormalizationMode.Logarithmic);
			if (normalizationMode == NormalizationMode.EqualizeHistogram)
			{
				ParseIES.EqualizeHistogram(iesdata);
			}
			if (photometricType != PhotometricType.TypeA)
			{
				ParseIES.DiscardUnusedVerticalHalf(iesdata);
				ParseIES.SetVerticalAndHorizontalType(iesdata);
				iesdata.HalfSpotlightFov = ParseIES.CalculateHalfSpotFov(iesdata);
			}
			else
			{
				ParseIES.PadToSquare(iesdata);
			}
			return iesdata;
		}

		private static void DiscardUnusedVerticalHalf(IESData iesData)
		{
			if (iesData.VerticalAngles[0] != 0f || iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] != 180f)
			{
				return;
			}
			int k = 0;
			while (k < iesData.VerticalAngles.Count && !iesData.NormalizedValues.Any((List<float> slice) => slice[k] > 0.1f))
			{
				if (iesData.VerticalAngles[k] == 90f)
				{
					ParseIES.DiscardBottomHalf(iesData);
					return;
				}
				if (iesData.VerticalAngles[k] > 90f)
				{
					iesData.VerticalAngles[k] = 90f;
					ParseIES.DiscardBottomHalf(iesData);
					return;
				}
				int j = k;
				k = j + 1;
			}
			int i = iesData.VerticalAngles.Count - 1;
			while (i >= 0 && !iesData.NormalizedValues.Any((List<float> slice) => slice[i] > 0.1f))
			{
				if (iesData.VerticalAngles[i] == 90f)
				{
					ParseIES.DiscardTopHalf(iesData);
					return;
				}
				if (iesData.VerticalAngles[i] < 90f)
				{
					iesData.VerticalAngles[i] = 90f;
					ParseIES.DiscardTopHalf(iesData);
					return;
				}
				int j = i;
				i = j - 1;
			}
		}

		private static void DiscardBottomHalf(IESData iesData)
		{
			int num = 0;
			int num2 = 0;
			while (num2 < iesData.VerticalAngles.Count && iesData.VerticalAngles[num2] != 90f)
			{
				num++;
				num2++;
			}
			ParseIES.DiscardHalf(iesData, 0, num);
		}

		private static void DiscardTopHalf(IESData iesData)
		{
			int num = 0;
			for (int i = 0; i < iesData.VerticalAngles.Count; i++)
			{
				if (iesData.VerticalAngles[i] == 90f)
				{
					num = i + 1;
					break;
				}
			}
			int range = iesData.VerticalAngles.Count - num;
			ParseIES.DiscardHalf(iesData, num, range);
		}

		private static void DiscardHalf(IESData iesData, int start, int range)
		{
			iesData.VerticalAngles.RemoveRange(start, range);
			for (int i = 0; i < iesData.CandelaValues.Count; i++)
			{
				iesData.CandelaValues[i].RemoveRange(start, range);
				iesData.NormalizedValues[i].RemoveRange(start, range);
			}
		}

		private static void PadToSquare(IESData iesData)
		{
			if (Mathf.Abs(iesData.HorizontalAngles.Count - iesData.VerticalAngles.Count) <= 1)
			{
				return;
			}
			int num = Mathf.Max(iesData.HorizontalAngles.Count, iesData.VerticalAngles.Count);
			if (iesData.HorizontalAngles.Count < num)
			{
				ParseIES.PadHorizontal(iesData, num);
				return;
			}
			ParseIES.PadVertical(iesData, num);
		}

		private static void PadHorizontal(IESData iesData, int longestSide)
		{
			int num = longestSide - iesData.HorizontalAngles.Count;
			int num2 = num / 2;
			iesData.PadBeforeAmount = (iesData.PadAfterAmount = num2);
			List<float> item = Enumerable.Repeat<float>(0f, iesData.VerticalAngles.Count).ToList<float>();
			for (int i = 0; i < num2; i++)
			{
				iesData.NormalizedValues.Insert(0, item);
			}
			for (int j = 0; j < num - num2; j++)
			{
				iesData.NormalizedValues.Add(item);
			}
		}

		private static void PadVertical(IESData iesData, int longestSide)
		{
			int num = longestSide - iesData.VerticalAngles.Count;
			if (Mathf.Sign(iesData.VerticalAngles[0]) == (float)Math.Sign(iesData.VerticalAngles[iesData.VerticalAngles.Count - 1]))
			{
				int num2 = num / 2;
				iesData.PadBeforeAmount = num2;
				iesData.PadAfterAmount = num - num2;
				using (List<List<float>>.Enumerator enumerator = iesData.NormalizedValues.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						List<float> list = enumerator.Current;
						list.InsertRange(0, new List<float>(new float[num2]));
						list.AddRange(new List<float>(new float[num - num2]));
					}
					return;
				}
			}
			int num3 = longestSide / 2 - iesData.VerticalAngles.Count((float v) => v >= 0f);
			if (iesData.VerticalAngles[0] < 0f)
			{
				iesData.PadBeforeAmount = num - num3;
				iesData.PadAfterAmount = num3;
				using (List<List<float>>.Enumerator enumerator = iesData.NormalizedValues.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						List<float> list2 = enumerator.Current;
						list2.InsertRange(0, new List<float>(new float[num - num3]));
						list2.AddRange(new List<float>(new float[num3]));
					}
					return;
				}
			}
			iesData.PadBeforeAmount = num3;
			iesData.PadAfterAmount = num - num3;
			foreach (List<float> list3 in iesData.NormalizedValues)
			{
				list3.InsertRange(0, new List<float>(new float[num3]));
				list3.AddRange(new List<float>(new float[num - num3]));
			}
		}

		private static void SetVerticalAndHorizontalType(IESData iesData)
		{
			if ((iesData.VerticalAngles[0] == 0f && iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] == 90f) || (iesData.VerticalAngles[0] == -90f && iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] == 0f))
			{
				iesData.VerticalType = VerticalType.Bottom;
			}
			else if (iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] == 180f && iesData.VerticalAngles[0] == 90f)
			{
				iesData.VerticalType = VerticalType.Top;
			}
			else
			{
				iesData.VerticalType = VerticalType.Full;
			}
			if (iesData.HorizontalAngles.Count == 1)
			{
				iesData.HorizontalType = HorizontalType.None;
				return;
			}
			if (iesData.HorizontalAngles[iesData.HorizontalAngles.Count - 1] - iesData.HorizontalAngles[0] == 90f)
			{
				iesData.HorizontalType = HorizontalType.Quadrant;
				return;
			}
			if (iesData.HorizontalAngles[iesData.HorizontalAngles.Count - 1] - iesData.HorizontalAngles[0] == 180f)
			{
				iesData.HorizontalType = HorizontalType.Half;
				return;
			}
			iesData.HorizontalType = HorizontalType.Full;
			if (iesData.HorizontalAngles[iesData.HorizontalAngles.Count - 1] != 360f)
			{
				ParseIES.StitchHorizontalAssymetry(iesData);
			}
		}

		private static void StitchHorizontalAssymetry(IESData iesData)
		{
			iesData.HorizontalAngles.Add(360f);
			iesData.CandelaValues.Add(iesData.CandelaValues[0]);
			iesData.NormalizedValues.Add(iesData.NormalizedValues[0]);
		}

		private static float CalculateHalfSpotFov(IESData iesData)
		{
			if (iesData.VerticalType == VerticalType.Bottom && iesData.VerticalAngles[0] == 0f)
			{
				return ParseIES.CalculateHalfSpotlightFovForBottomHalf(iesData);
			}
			if (iesData.VerticalType == VerticalType.Top || (iesData.VerticalType == VerticalType.Bottom && iesData.VerticalAngles[0] == -90f))
			{
				return ParseIES.CalculateHalfSpotlightFovForTopHalf(iesData);
			}
			return -1f;
		}

		private static float CalculateHalfSpotlightFovForBottomHalf(IESData iesData)
		{
			for (int i = iesData.VerticalAngles.Count - 1; i >= 0; i--)
			{
				int j = 0;
				while (j < iesData.NormalizedValues.Count)
				{
					if (iesData.NormalizedValues[j][i] >= 0.1f)
					{
						if (i < iesData.VerticalAngles.Count - 1)
						{
							return iesData.VerticalAngles[i + 1];
						}
						return iesData.VerticalAngles[i];
					}
					else
					{
						j++;
					}
				}
			}
			return 0f;
		}

		private static float CalculateHalfSpotlightFovForTopHalf(IESData iesData)
		{
			for (int i = 0; i < iesData.VerticalAngles.Count; i++)
			{
				int j = 0;
				while (j < iesData.NormalizedValues.Count)
				{
					if (iesData.NormalizedValues[j][i] >= 0.1f)
					{
						if (iesData.VerticalType == VerticalType.Top)
						{
							if (i > 0)
							{
								return 180f - iesData.VerticalAngles[i - 1];
							}
							return 180f - iesData.VerticalAngles[i];
						}
						else
						{
							if (i > 0)
							{
								return -iesData.VerticalAngles[i - 1];
							}
							return -iesData.VerticalAngles[i];
						}
					}
					else
					{
						j++;
					}
				}
			}
			return 0f;
		}

		private static void NormalizeValues(IESData iesData, bool squashHistogram)
		{
			iesData.NormalizedValues = new List<List<float>>();
			float num = iesData.CandelaValues.SelectMany((List<float> v) => v).Max();
			if (squashHistogram)
			{
				num = Mathf.Log(num);
			}
			foreach (List<float> list in iesData.CandelaValues)
			{
				List<float> list2 = new List<float>();
				if (squashHistogram)
				{
					for (int i = 0; i < list.Count; i++)
					{
						list2.Add(Mathf.Log(list[i]));
					}
				}
				else
				{
					list2.AddRange(list);
				}
				for (int j = 0; j < list.Count; j++)
				{
					List<float> list3 = list2;
					int index = j;
					list3[index] /= num;
					list2[j] = Mathf.Clamp01(list2[j]);
				}
				iesData.NormalizedValues.Add(list2);
			}
		}

		private static void EqualizeHistogram(IESData iesData)
		{
			int num = Mathf.Min((int)iesData.CandelaValues.SelectMany((List<float> v) => v).Max(), 10000);
			float[] array = new float[num];
			float[] array2 = new float[num];
			foreach (List<float> list in iesData.NormalizedValues)
			{
				foreach (float num2 in list)
				{
					array[(int)(num2 * (float)(num - 1))] += 1f;
				}
			}
			float num3 = (float)(iesData.HorizontalAngles.Count * iesData.VerticalAngles.Count);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] /= num3;
			}
			for (int j = 0; j < num; j++)
			{
				array2[j] = array.Take(j + 1).Sum();
			}
			foreach (List<float> list2 in iesData.NormalizedValues)
			{
				for (int k = 0; k < list2.Count; k++)
				{
					int num4 = (int)(list2[k] * (float)(num - 1));
					list2[k] = array2[num4] * (float)(num - 1) / (float)num;
				}
			}
		}

		private static void FindNumberOfAnglesLine(string[] lines, ref int lineNumber)
		{
			int i;
			for (i = 0; i < lines.Length; i++)
			{
				if (lines[i].Trim().StartsWith("TILT"))
				{
					try
					{
						if (lines[i].Split(new char[]
						{
							'='
						})[1].Trim() != "NONE")
						{
							i += 5;
							break;
						}
						i++;
						break;
					}
					catch (ArgumentOutOfRangeException)
					{
						throw new IESParseException("No TILT line present.");
					}
				}
			}
			lineNumber = i;
		}

		private static void ReadProperties(string[] lines, ref int lineNumber, out int numberOfVerticalAngles, out int numberOfHorizontalAngles, out PhotometricType photometricType)
		{
			List<float> list = ParseIES.ReadValues(lines, 13, ref lineNumber);
			numberOfVerticalAngles = (int)list[3];
			numberOfHorizontalAngles = (int)list[4];
			photometricType = (PhotometricType)list[5];
		}

		private static List<float> ReadValues(string[] lines, int numberOfValuesToFind, ref int lineNumber)
		{
			List<float> list = new List<float>();
			while (list.Count < numberOfValuesToFind)
			{
				if (lineNumber >= lines.Length)
				{
					throw new IESParseException("Reached end of file before the given number of values was read.");
				}
				char[] separator = null;
				if (lines[lineNumber].Contains(","))
				{
					separator = new char[]
					{
						','
					};
				}
				foreach (string s in lines[lineNumber].Split(separator, StringSplitOptions.RemoveEmptyEntries))
				{
					try
					{
						list.Add(float.Parse(s));
					}
					catch (Exception inner)
					{
						throw new IESParseException("Invalid value declaration.", inner);
					}
				}
				lineNumber++;
			}
			return list;
		}

		private const float SpotlightCutoff = 0.1f;
	}
}
