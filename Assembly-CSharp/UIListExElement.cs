using System;
using UnityEngine;

public class UIListExElement
{
	public UIListExElement()
	{
	}

	public UIListExElement(GameObject prefab, GameObject parent)
	{
		this.ui_element = UnityEngine.Object.Instantiate<GameObject>(prefab);
		this.ui_element.gameObject.transform.parent = parent.transform;
	}

	public string text;

	public int data = -1;

	public GameObject ui_element;

	public int idx = -1;

	public bool show_delete_button;
}
