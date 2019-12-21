using System;
using System.Collections.Generic;
using System.IO;

public class Key
{
	public Key(string name)
	{
		this.m_Name = name;
	}

	public string GetName()
	{
		return this.m_Name;
	}

	public void AddVariable(CJVariable variable)
	{
		this.m_Variables.Add(variable);
	}

	public CJVariable AddVariable()
	{
		CJVariable cjvariable = new CJVariable();
		this.m_Variables.Add(cjvariable);
		return cjvariable;
	}

	public Key AddKey(string key_name)
	{
		Key key = new Key(key_name);
		this.m_Keys.Add(key);
		return key;
	}

	public void AddKey(Key key)
	{
		this.m_Keys.Add(key);
	}

	public Key GetKey(int index)
	{
		if (index < 0 || index >= this.m_Keys.Count)
		{
			return null;
		}
		return this.m_Keys[index];
	}

	public int GetKeysCount()
	{
		return this.m_Keys.Count;
	}

	public CJVariable GetVariable(int index)
	{
		if (index < 0 || index >= this.m_Variables.Count)
		{
			return null;
		}
		return this.m_Variables[index];
	}

	public int GetVariablesCount()
	{
		return this.m_Variables.Count;
	}

	public void Write(StreamWriter stream, int tabs)
	{
		for (int i = 0; i < tabs; i++)
		{
			stream.Write("\t");
		}
		stream.Write(this.GetName().ToCharArray());
		stream.Write("(");
		for (int j = 0; j < this.m_Variables.Count; j++)
		{
			this.m_Variables[j].Write(stream);
			if (j < this.m_Variables.Count - 1)
			{
				stream.Write(", ");
			}
		}
		stream.Write(")");
		if (this.GetKeysCount() > 0)
		{
			stream.Write("\n");
			for (int k = 0; k < tabs; k++)
			{
				stream.Write("\t");
			}
			stream.Write("{\n");
			tabs++;
		}
		for (int l = 0; l < this.GetKeysCount(); l++)
		{
			this.GetKey(l).Write(stream, tabs);
			if (l < this.GetKeysCount() - 1)
			{
				stream.Write("\n");
			}
		}
		if (this.GetKeysCount() > 0)
		{
			stream.Write("\n");
			tabs--;
			for (int m = 0; m < tabs; m++)
			{
				stream.Write("\t");
			}
			stream.Write("}");
		}
	}

	public List<CJVariable> m_Variables = new List<CJVariable>();

	public Key m_Parent;

	public List<Key> m_Keys = new List<Key>();

	private string m_Name;
}
