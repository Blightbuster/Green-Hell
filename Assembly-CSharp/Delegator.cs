using System;
using System.Collections.Generic;

public class Delegator
{
	public void RegisterVoid(VDelegate handler)
	{
		this.m_VDeleates.Add(handler.Method.Name, handler);
	}

	public void RegisterBool(BDelegate handler)
	{
		this.m_BDeleates.Add(handler.Method.Name, handler);
	}

	public void RegisterVoidStr(VDelegateS handler)
	{
		this.m_VStrDeleates.Add(handler.Method.Name, handler);
	}

	public void CallVoid(string name)
	{
		if (!this.m_VDeleates.ContainsKey(name))
		{
			DebugUtils.Assert("[Delegator::CallVoid] Error - method " + name + " is not registered!", true, DebugUtils.AssertType.Info);
			return;
		}
		this.m_VDeleates[name]();
	}

	public void CallVoid(string name, string string_param)
	{
		if (!this.m_VStrDeleates.ContainsKey(name))
		{
			DebugUtils.Assert("[Delegator::CallVoid(string)] Error - method " + name + " is not registered!", true, DebugUtils.AssertType.Info);
			return;
		}
		this.m_VStrDeleates[name](string_param);
	}

	public bool CallBool(string name)
	{
		if (!this.m_BDeleates.ContainsKey(name))
		{
			DebugUtils.Assert("[Delegator::CallVoid(string)] Error - method " + name + " is not registered!", true, DebugUtils.AssertType.Info);
			return false;
		}
		return this.m_BDeleates[name]();
	}

	private Dictionary<string, VDelegate> m_VDeleates = new Dictionary<string, VDelegate>();

	private Dictionary<string, BDelegate> m_BDeleates = new Dictionary<string, BDelegate>();

	private Dictionary<string, VDelegateS> m_VStrDeleates = new Dictionary<string, VDelegateS>();
}
