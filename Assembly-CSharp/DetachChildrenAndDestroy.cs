using System;
using UnityEngine;

public class DetachChildrenAndDestroy : MonoBehaviour
{
	private void Start()
	{
		int i = 0;
		while (i < base.transform.childCount)
		{
			base.transform.GetChild(i).parent = null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
