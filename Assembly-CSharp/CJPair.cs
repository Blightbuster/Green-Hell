using System;

public struct CJPair<T1, T2>
{
	public CJPair(T1 l1, T2 l2)
	{
		this.v1 = l1;
		this.v2 = l2;
	}

	public T1 v1;

	public T2 v2;
}
