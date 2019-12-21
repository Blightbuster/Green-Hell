using System;
using UnityEngine;

[ExecuteInEditMode]
[Serializable]
public class SceneCollection : MonoBehaviour
{
	private void OnEnable()
	{
		for (int i = 0; i < this.names.Length; i++)
		{
			if (this.names[i].Contains("x10") && this.names[i].Contains("z33"))
			{
				int num = 0 + 1;
			}
		}
	}

	public string prefixName = "stream";

	public string prefixScene = "Scene";

	public string path = "Assets/WorldStreamer/SplitScenes/";

	public string[] names;

	public bool xSplitIs = true;

	public bool ySplitIs;

	public bool zSplitIs = true;

	public int xSize = 10;

	public int ySize = 10;

	public int zSize = 10;

	public int xLimitsx = int.MaxValue;

	public int xLimitsy = int.MinValue;

	public int yLimitsx = int.MaxValue;

	public int yLimitsy = int.MinValue;

	public int zLimitsx = int.MaxValue;

	public int zLimitsy = int.MinValue;

	[HideInInspector]
	public bool collapsed = true;

	[HideInInspector]
	public int layerNumber;

	public Color color = Color.red;
}
