using System;
using System.Collections.Generic;
using UnityEngine;

public class HUDBase : MonoBehaviour
{
	protected virtual void Start()
	{
		this.SetupController();
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void Awake()
	{
	}

	public virtual void SetupGroups()
	{
	}

	public virtual void OnSetGroup(bool in_group)
	{
	}

	public virtual void ScenarioBlock()
	{
		this.m_ScenarioBlocked = true;
	}

	public virtual void ScenarioUnblock()
	{
		this.m_ScenarioBlocked = false;
	}

	public void Show(bool show)
	{
		if (base.enabled == show)
		{
			return;
		}
		base.enabled = show;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(show);
		}
		if (show)
		{
			this.OnShow();
			this.SetupController();
			return;
		}
		this.OnHide();
	}

	protected virtual void OnShow()
	{
	}

	protected virtual void OnHide()
	{
	}

	protected virtual bool ShouldShow()
	{
		return false;
	}

	public virtual void UpdateAfterCamera()
	{
	}

	public virtual void ConstantUpdate()
	{
		this.UpdateVisibility();
	}

	protected virtual void Update()
	{
	}

	public void UpdateVisibility()
	{
		bool flag = this.ShouldShow() && !this.m_ScenarioBlocked;
		if (base.enabled != flag)
		{
			this.Show(flag);
		}
	}

	protected virtual void LateUpdate()
	{
	}

	protected void AddToGroup(HUDManager.HUDGroup group)
	{
		this.m_Group |= group;
	}

	public bool IsInGroup(HUDManager.HUDGroup group)
	{
		if (this.m_Group == HUDManager.HUDGroup.None)
		{
			this.SetupGroups();
		}
		return (this.m_Group & group) > HUDManager.HUDGroup.None;
	}

	protected GameObject AddElement(string name)
	{
		return UnityEngine.Object.Instantiate<GameObject>(GreenHellGame.Instance.GetPrefab(name), base.transform.position, base.transform.rotation);
	}

	protected void RemoveElement(GameObject obj)
	{
		UnityEngine.Object.Destroy(obj);
	}

	private void OnDestroy()
	{
		IInputsReceiver component = base.gameObject.GetComponent<IInputsReceiver>();
		if (component != null)
		{
			InputsManager.Get().UnregisterReceiver(component);
		}
	}

	public virtual void SetupController()
	{
		bool flag = GreenHellGame.IsPadControllerActive();
		foreach (GameObject gameObject in this.m_PadDisableElements)
		{
			gameObject.SetActive(!flag && base.enabled);
		}
		foreach (GameObject gameObject2 in this.m_PadEnableElements)
		{
			gameObject2.SetActive(flag && base.enabled);
		}
	}

	private HUDManager.HUDGroup m_Group;

	[HideInInspector]
	public bool m_ScenarioBlocked;

	public List<GameObject> m_PadEnableElements = new List<GameObject>();

	public List<GameObject> m_PadDisableElements = new List<GameObject>();
}
