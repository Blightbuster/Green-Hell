using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltimateWater
{
	public class ShaderVariant
	{
		public ShaderVariant()
		{
			this._UnityKeywords = new Dictionary<string, bool>();
			this._WaterKeywords = new Dictionary<string, bool>();
			this._SurfaceShaderParts = new Dictionary<string, string>();
			this._VolumeShaderParts = new Dictionary<string, string>();
		}

		public void SetUnityKeyword(string keyword, bool value)
		{
			if (value)
			{
				this._UnityKeywords[keyword] = true;
			}
			else
			{
				this._UnityKeywords.Remove(keyword);
			}
		}

		public void SetWaterKeyword(string keyword, bool value)
		{
			if (value)
			{
				this._WaterKeywords[keyword] = true;
			}
			else
			{
				this._WaterKeywords.Remove(keyword);
			}
		}

		public void SetAdditionalSurfaceCode(string keyword, string code)
		{
			if (code != null)
			{
				this._SurfaceShaderParts[keyword] = code;
			}
			else
			{
				this._SurfaceShaderParts.Remove(keyword);
			}
		}

		public void SetAdditionalVolumeCode(string keyword, string code)
		{
			if (code != null)
			{
				this._VolumeShaderParts[keyword] = code;
			}
			else
			{
				this._VolumeShaderParts.Remove(keyword);
			}
		}

		public bool IsUnityKeywordEnabled(string keyword)
		{
			bool flag;
			return this._UnityKeywords.TryGetValue(keyword, out flag);
		}

		public bool IsWaterKeywordEnabled(string keyword)
		{
			bool flag;
			return this._WaterKeywords.TryGetValue(keyword, out flag);
		}

		public string GetAdditionalSurfaceCode()
		{
			StringBuilder stringBuilder = new StringBuilder(512);
			foreach (string value in this._SurfaceShaderParts.Values)
			{
				stringBuilder.Append(value);
			}
			return stringBuilder.ToString();
		}

		public string GetAdditionalVolumeCode()
		{
			StringBuilder stringBuilder = new StringBuilder(512);
			foreach (string value in this._VolumeShaderParts.Values)
			{
				stringBuilder.Append(value);
			}
			return stringBuilder.ToString();
		}

		public string[] GetUnityKeywords()
		{
			string[] array = new string[this._UnityKeywords.Count];
			int num = 0;
			foreach (string text in this._UnityKeywords.Keys)
			{
				array[num++] = text;
			}
			return array;
		}

		public string[] GetWaterKeywords()
		{
			string[] array = new string[this._WaterKeywords.Count];
			int num = 0;
			foreach (string text in this._WaterKeywords.Keys)
			{
				array[num++] = text;
			}
			return array;
		}

		public string GetKeywordsString()
		{
			StringBuilder stringBuilder = new StringBuilder(512);
			bool flag = false;
			foreach (string value in from k in this._WaterKeywords.Keys
			orderby k
			select k)
			{
				if (flag)
				{
					stringBuilder.Append(' ');
				}
				else
				{
					flag = true;
				}
				stringBuilder.Append(value);
			}
			foreach (string value2 in from k in this._UnityKeywords.Keys
			orderby k
			select k)
			{
				if (flag)
				{
					stringBuilder.Append(' ');
				}
				else
				{
					flag = true;
				}
				stringBuilder.Append(value2);
			}
			foreach (string value3 in from k in this._SurfaceShaderParts.Keys
			orderby k
			select k)
			{
				if (flag)
				{
					stringBuilder.Append(' ');
				}
				else
				{
					flag = true;
				}
				stringBuilder.Append(value3);
			}
			return stringBuilder.ToString();
		}

		private readonly Dictionary<string, bool> _UnityKeywords;

		private readonly Dictionary<string, bool> _WaterKeywords;

		private readonly Dictionary<string, string> _SurfaceShaderParts;

		private readonly Dictionary<string, string> _VolumeShaderParts;
	}
}
