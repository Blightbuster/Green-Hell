using System;

internal class FloatConversion
{
	public static float ToSingle(uint value)
	{
		return new UIntFloat
		{
			intValue = value
		}.floatValue;
	}

	public static double ToDouble(ulong value)
	{
		return new UIntFloat
		{
			longValue = value
		}.doubleValue;
	}
}
