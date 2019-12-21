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

	public override string ToString()
	{
		return "V" + this.m_Major.ToString() + "." + ((float)this.m_Minor * 0.1f).ToString("0.0");
	}

	public string ToStringOfficial()
	{
		if (this.m_Major == 0)
		{
			if (this.m_Minor < 18)
			{
				return "pre V.0.4.3";
			}
			if (this.m_Minor == 18)
			{
				return "V.0.4.3";
			}
			if (this.m_Minor == 19)
			{
				return "V.0.5.0";
			}
			if (this.m_Minor == 20)
			{
				return "V.0.5.6";
			}
			if (this.m_Minor == 50)
			{
				return "V.0.9";
			}
		}
		return this.ToString();
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

	public static bool NoVersion(GameVersion gv)
	{
		return gv.m_Major == 0 && gv.m_Minor == 0;
	}

	public override bool Equals(object obj)
	{
		GameVersion gameVersion = obj as GameVersion;
		return gameVersion != null && this.m_Major == gameVersion.m_Major && this.m_Minor == gameVersion.m_Minor;
	}

	public override int GetHashCode()
	{
		return (696028640 * -1521134295 + this.m_Major.GetHashCode()) * -1521134295 + this.m_Minor.GetHashCode();
	}

	public int m_Major;

	public int m_Minor = 9;
}
