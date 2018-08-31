using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Backpack : MonoBehaviour
{
	public void Initialize()
	{
		this.LoadScript();
	}

	private void LoadScript()
	{
		this.m_PocketPosition = new Vector3[5];
		this.m_PocketRotation = new Quaternion[5];
		TextAsset asset = Resources.Load("Scripts/Backpack") as TextAsset;
		TextAssetParser textAssetParser = new TextAssetParser(asset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "Transform")
			{
				int num = (int)Enum.Parse(typeof(BackpackPocket), key.GetVariable(0).SValue);
				this.m_PocketPosition[num].x = key.GetVariable(1).FValue;
				this.m_PocketPosition[num].y = key.GetVariable(2).FValue;
				this.m_PocketPosition[num].z = key.GetVariable(3).FValue;
				this.m_PocketRotation[num].x = key.GetVariable(4).FValue;
				this.m_PocketRotation[num].y = key.GetVariable(5).FValue;
				this.m_PocketRotation[num].z = key.GetVariable(6).FValue;
				this.m_PocketRotation[num].w = key.GetVariable(7).FValue;
			}
		}
	}

	public List<GameObject> GetObjectToDisable(BackpackPocket pocket)
	{
		switch (pocket)
		{
		case BackpackPocket.Main:
			return this.m_MainPocketDisabledObjs;
		case BackpackPocket.Front:
			return this.m_FrontPocketDisabledObjs;
		case BackpackPocket.Top:
			return this.m_TopPocketDisabledObjs;
		case BackpackPocket.Left:
			return this.m_LeftPocketDisabledObjs;
		case BackpackPocket.Right:
			return this.m_RightPocketDisabledObjs;
		default:
			return null;
		}
	}

	[HideInInspector]
	public Vector3[] m_PocketPosition;

	[HideInInspector]
	public Quaternion[] m_PocketRotation;

	public List<GameObject> m_MainPocketDisabledObjs;

	public List<GameObject> m_FrontPocketDisabledObjs;

	public List<GameObject> m_TopPocketDisabledObjs;

	public List<GameObject> m_LeftPocketDisabledObjs;

	public List<GameObject> m_RightPocketDisabledObjs;

	public GameObject m_GridCellPrefab;
}
