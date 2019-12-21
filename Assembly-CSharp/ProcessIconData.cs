using System;
using UnityEngine;
using UnityEngine.UI;

public class ProcessIconData
{
	public GameObject obj;

	public CanvasGroup canvas_group;

	public Trigger trigger;

	public Image icon;

	public string icon_name;

	public Image progress;

	public IProcessor processor;

	public bool allow_enabled;
}
