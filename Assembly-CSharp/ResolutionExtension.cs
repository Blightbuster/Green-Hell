using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ResolutionExtension
{
	public static bool Equals(this Resolution res, Resolution other_res, bool ignore_refresh_rate)
	{
		return res.width == other_res.width && res.height == other_res.height && (ignore_refresh_rate || res.refreshRate == other_res.refreshRate);
	}

	public static string ToString(string res, string refresh_rate)
	{
		return res + " @ " + refresh_rate;
	}

	public static void ToString2(this Resolution res, out string resolution, out string refresh_rate)
	{
		resolution = string.Format("{0} x {1}", res.width, res.height);
		refresh_rate = string.Format("{0} Hz", res.refreshRate);
	}

	public static void FromString(this Resolution res, string resolution_str)
	{
		res = ResolutionExtension.ResolutionFromString(resolution_str);
	}

	public static Resolution ResolutionFromString(string resolution_str)
	{
		Match match = Regex.Match(resolution_str, "^(\\w+) x (\\w+) @ (\\d+)");
		if (!match.Success)
		{
			match = Regex.Match(resolution_str, "^(\\w+) x (\\w+)");
		}
		if (!match.Success)
		{
			return default(Resolution);
		}
		Resolution result = default(Resolution);
		if (match.Groups.Count > 1)
		{
			result.width = int.Parse(match.Groups[1].Value);
		}
		if (match.Groups.Count > 2)
		{
			result.height = int.Parse(match.Groups[2].Value);
		}
		if (match.Groups.Count > 3)
		{
			result.refreshRate = int.Parse(match.Groups[3].Value);
		}
		if (result.refreshRate == 0)
		{
			result.refreshRate = 60;
		}
		return result;
	}

	public static bool IsValid(this Resolution res)
	{
		return res.width >= 800 && res.height >= 600;
	}

	public static Resolution SelectResolution(string res_name)
	{
		return ResolutionExtension.SelectResolution(ResolutionExtension.ResolutionFromString(res_name));
	}

	public static Resolution SelectResolution(Resolution target_res)
	{
		if (!target_res.IsValid())
		{
			return Screen.currentResolution;
		}
		Resolution result = Screen.currentResolution;
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			if (target_res.Equals(Screen.resolutions[i], false))
			{
				return target_res;
			}
			if (target_res.Equals(Screen.resolutions[i], true))
			{
				result = Screen.resolutions[i];
			}
		}
		return result;
	}
}
