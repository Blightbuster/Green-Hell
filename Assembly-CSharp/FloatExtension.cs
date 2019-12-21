using System;

public static class FloatExtension
{
	public static float IfAlmostZeroGetZero(this float val, float precision = 0.0001f)
	{
		if (Math.Abs(val) > precision)
		{
			return val;
		}
		return 0f;
	}
}
