using System;

[Serializable]
public class GameVersion
{
	public GameVersion(int major, int minor)
	{
		this.m_Major = major;
		this.m_Minor = minor;
	}

	public GameVersion(GameVersion gv)
	{
		this.m_Major = gv.m_Major;
		this.m_Minor = gv.m_Minor;
	}

	public static bool operator ==(GameVersion lhs, GameVersion rhs)
	{
		return lhs.m_Minor == rhs.m_Minor && lhs.m_Major == rhs.m_Major;
	}

	public static bool operator !=(GameVersion lhs, GameVersion rhs)
	{
		return lhs.m_Minor != rhs.m_Minor || lhs.m_Major != rhs.m_Major;
	}

	public static bool operator <=(GameVersion lhs, GameVersion rhs)
	{
		return lhs.m_Minor * 10 + lhs.m_Major * 1000 <= rhs.m_Minor * 10 + rhs.m_Major * 1000;
	}

	public static bool operator >=(GameVersion lhs, GameVersion rhs)
	{
		return lhs.m_Minor * 10 + lhs.m_Major * 1000 >= rhs.m_Minor * 10 + rhs.m_Major * 1000;
	}

	public static bool operator <(GameVersion lhs, GameVersion rhs)
	{
		return lhs.m_Minor * 10 + lhs.m_Major * 1000 < rhs.m_Minor * 10 + rhs.m_Major * 1000;
	}

	public static bool operator >(GameVersion lhs, GameVersion rhs)
	{
		return lhs.m_Minor * 10 + lhs.m_Major * 1000 > rhs.m_Minor * 10 + rhs.m_Major * 1000;
	}

	public int m_Major;

	public int m_Minor = 9;
}
