using System;
using UnityEngine;

public class DetachChildrenAndDestroy : MonoBehaviour
{
	private void Start()
	{
		int i = 0;
		while (i < base.transform.childCount)
		{
			Transform child = base.transform.GetChild(i);
			child.parent = null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
