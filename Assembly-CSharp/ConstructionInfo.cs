using System;
using System.Collections.Generic;
using Enums;

public class ConstructionInfo : ItemInfo
{
	public List<string> m_PlaceToAttachNames { get; set; }

	public List<string> m_PlaceToAttachToNames { get; set; }

	public float m_RestingParamsMul { get; set; }

	public float m_ParamsMulRadius { get; set; }

	public ConstructionType m_ConstructionType { get; set; }

	public string m_MenuIcon { get; set; }

	public int m_HitsCountToDestroy { get; set; }

	public ConstructionInfo()
	{
		this.m_ConstructionType = ConstructionType.None;
		this.m_PlaceToAttachNames = new List<string>();
		this.m_PlaceToAttachToNames = new List<string>();
		this.m_RestingParamsMul = 1f;
		this.m_ParamsMulRadius = -1f;
		this.m_MenuIcon = string.Empty;
		this.m_HitsCountToDestroy = 0;
	}

	public override bool IsConstruction()
	{
		return true;
	}

	protected override void LoadParams(Key key)
	{
		if (key.GetName() == "PlacesToAttach")
		{
			string[] array = key.GetVariable(0).SValue.Split(new char[]
			{
				';'
			});
			for (int i = 0; i < array.Length; i++)
			{
				this.m_PlaceToAttachNames.Add(array[i]);
			}
			return;
		}
		if (key.GetName() == "PlacesToAttachTo")
		{
			string[] array2 = key.GetVariable(0).SValue.Split(new char[]
			{
				';'
			});
			for (int j = 0; j < array2.Length; j++)
			{
				this.m_PlaceToAttachToNames.Add(array2[j]);
			}
			return;
		}
		if (key.GetName() == "RestingParamsMul")
		{
			this.m_RestingParamsMul = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "ParamsMulRadius")
		{
			this.m_ParamsMulRadius = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "ConstructionType")
		{
			this.m_ConstructionType = (ConstructionType)Enum.Parse(typeof(ConstructionType), key.GetVariable(0).SValue);
			return;
		}
		if (key.GetName() == "MenuIcon")
		{
			this.m_MenuIcon = key.GetVariable(0).SValue;
			return;
		}
		if (key.GetName() == "HitsCountToDestroy")
		{
			this.m_HitsCountToDestroy = key.GetVariable(0).IValue;
			return;
		}
		base.LoadParams(key);
	}
}
