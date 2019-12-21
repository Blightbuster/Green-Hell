using System;
using UnityEngine;

public class DebugStorage : LocalStorage
{
	protected override string GetPath()
	{
		return Application.dataPath + "/Resources/Scripts/Debug/";
	}
}
