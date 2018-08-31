using System;
using UnityEngine;

public class HUDBase : MonoBehaviour
{
	protected virtual void Start()
	{
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
		}
		else
		{
			this.OnHide();
		}
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
		bool flag = this.ShouldShow();
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
		return (this.m_Group & group) != HUDManager.HUDGroup.None;
	}

	protected GameObject AddElement(string name)
	{
		GameObject prefab = GreenHellGame.Instance.GetPrefab(name);
		return UnityEngine.Object.Instantiate<GameObject>(prefab, base.transform.position, base.transform.rotation);
	}

	protected void RemoveElement(GameObject obj)
	{
		UnityEngine.Object.Destroy(obj);
	}

	private HUDManager.HUDGroup m_Group;
}
