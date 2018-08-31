using System;
using UnityEngine;

[Serializable]
public class SceneSplit
{
	public int posX;

	public int posY;

	public int posZ;

	public string sceneName;

	public GameObject sceneGo;

	public bool loaded;

	public float posXLimitMove;

	public int xDeloadLimit;

	public float posYLimitMove;

	public int yDeloadLimit;

	public float posZLimitMove;

	public int zDeloadLimit;
}
