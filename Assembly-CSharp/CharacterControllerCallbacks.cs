using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CharacterControllerCallbacks : MonoBehaviour
{
	private void Awake()
	{
		foreach (MonoBehaviour monoBehaviour in this.m_Proxy.GetComponents<MonoBehaviour>())
		{
			if (!(monoBehaviour.GetType() == typeof(CharacterControllerProxy)) && !(monoBehaviour.GetType() == typeof(CharacterController)))
			{
				CharacterControllerCallbacks.s_OnControllerColliderDel s_OnControllerColliderDel = (CharacterControllerCallbacks.s_OnControllerColliderDel)this.GetMethod(monoBehaviour, "OnControllerColliderHit", this.s_OnControllerColliderTypes);
				if (s_OnControllerColliderDel != null)
				{
					this.m_OnControllerColliderDelegates.Add(s_OnControllerColliderDel);
				}
				CharacterControllerCallbacks.s_OnCollisionDel s_OnCollisionDel = (CharacterControllerCallbacks.s_OnCollisionDel)this.GetMethod(monoBehaviour, "OnCollisionEnter", this.s_OnCollisionTypes);
				if (s_OnCollisionDel != null)
				{
					this.m_OnCollisionEnterDelegates.Add(s_OnCollisionDel);
				}
				CharacterControllerCallbacks.s_OnCollisionDel s_OnCollisionDel2 = (CharacterControllerCallbacks.s_OnCollisionDel)this.GetMethod(monoBehaviour, "OnCollisionExit", this.s_OnCollisionTypes);
				if (s_OnCollisionDel2 != null)
				{
					this.m_OnCollisionExitDelegates.Add(s_OnCollisionDel2);
				}
				CharacterControllerCallbacks.s_OnCollisionDel s_OnCollisionDel3 = (CharacterControllerCallbacks.s_OnCollisionDel)this.GetMethod(monoBehaviour, "OnCollisionStay", this.s_OnCollisionTypes);
				if (s_OnCollisionDel3 != null)
				{
					this.m_OnCollisionStayDelegates.Add(s_OnCollisionDel3);
				}
				CharacterControllerCallbacks.s_OnTriggerDel s_OnTriggerDel = (CharacterControllerCallbacks.s_OnTriggerDel)this.GetMethod(monoBehaviour, "OnTriggerEnter", this.s_OnTriggerTypes);
				if (s_OnTriggerDel != null)
				{
					this.m_OnTriggerEnterDelegates.Add(s_OnTriggerDel);
				}
				CharacterControllerCallbacks.s_OnTriggerDel s_OnTriggerDel2 = (CharacterControllerCallbacks.s_OnTriggerDel)this.GetMethod(monoBehaviour, "OnTriggerExit", this.s_OnTriggerTypes);
				if (s_OnTriggerDel2 != null)
				{
					this.m_OnTriggerExitDelegates.Add(s_OnTriggerDel2);
				}
				CharacterControllerCallbacks.s_OnTriggerDel s_OnTriggerDel3 = (CharacterControllerCallbacks.s_OnTriggerDel)this.GetMethod(monoBehaviour, "OnTriggerStay", this.s_OnTriggerTypes);
				if (s_OnTriggerDel3 != null)
				{
					this.m_OnTriggerStayDelegates.Add(s_OnTriggerDel3);
				}
			}
		}
	}

	private Delegate GetMethod(object obj, string name, Tuple<Type, Type[]> types)
	{
		MethodInfo method = obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, types.Item2, null);
		if (method == null)
		{
			return null;
		}
		return method.CreateDelegate(types.Item1, obj);
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.transform.root.gameObject == this.m_Proxy.gameObject)
		{
			return;
		}
		foreach (CharacterControllerCallbacks.s_OnControllerColliderDel s_OnControllerColliderDel in this.m_OnControllerColliderDelegates)
		{
			s_OnControllerColliderDel(hit);
		}
	}

	public void OnCollisionEnter(Collision coll)
	{
		if (coll.transform.root.gameObject == this.m_Proxy.gameObject)
		{
			return;
		}
		foreach (CharacterControllerCallbacks.s_OnCollisionDel s_OnCollisionDel in this.m_OnCollisionEnterDelegates)
		{
			s_OnCollisionDel(coll);
		}
	}

	public void OnCollisionExit(Collision coll)
	{
		if (coll.transform.root.gameObject == this.m_Proxy.gameObject)
		{
			return;
		}
		foreach (CharacterControllerCallbacks.s_OnCollisionDel s_OnCollisionDel in this.m_OnCollisionExitDelegates)
		{
			s_OnCollisionDel(coll);
		}
	}

	public void OnCollisionStay(Collision coll)
	{
		if (coll.transform.root.gameObject == this.m_Proxy.gameObject)
		{
			return;
		}
		foreach (CharacterControllerCallbacks.s_OnCollisionDel s_OnCollisionDel in this.m_OnCollisionStayDelegates)
		{
			s_OnCollisionDel(coll);
		}
	}

	public void OnTriggerEnter(Collider coll)
	{
		if (coll.transform.root.gameObject == this.m_Proxy.gameObject)
		{
			return;
		}
		foreach (CharacterControllerCallbacks.s_OnTriggerDel s_OnTriggerDel in this.m_OnTriggerEnterDelegates)
		{
			s_OnTriggerDel(coll);
		}
	}

	public void OnTriggerExit(Collider coll)
	{
		if (coll.transform.root.gameObject == this.m_Proxy.gameObject)
		{
			return;
		}
		foreach (CharacterControllerCallbacks.s_OnTriggerDel s_OnTriggerDel in this.m_OnTriggerExitDelegates)
		{
			s_OnTriggerDel(coll);
		}
	}

	public void OnTriggerStay(Collider coll)
	{
		if (coll.transform.root.gameObject == this.m_Proxy.gameObject)
		{
			return;
		}
		foreach (CharacterControllerCallbacks.s_OnTriggerDel s_OnTriggerDel in this.m_OnTriggerStayDelegates)
		{
			s_OnTriggerDel(coll);
		}
	}

	public CharacterControllerProxy m_Proxy;

	private readonly Tuple<Type, Type[]> s_OnControllerColliderTypes = new Tuple<Type, Type[]>(typeof(CharacterControllerCallbacks.s_OnControllerColliderDel), new Type[]
	{
		typeof(ControllerColliderHit)
	});

	private readonly Tuple<Type, Type[]> s_OnCollisionTypes = new Tuple<Type, Type[]>(typeof(CharacterControllerCallbacks.s_OnCollisionDel), new Type[]
	{
		typeof(Collision)
	});

	private readonly Tuple<Type, Type[]> s_OnTriggerTypes = new Tuple<Type, Type[]>(typeof(CharacterControllerCallbacks.s_OnTriggerDel), new Type[]
	{
		typeof(Collider)
	});

	private List<CharacterControllerCallbacks.s_OnControllerColliderDel> m_OnControllerColliderDelegates = new List<CharacterControllerCallbacks.s_OnControllerColliderDel>();

	private List<CharacterControllerCallbacks.s_OnCollisionDel> m_OnCollisionEnterDelegates = new List<CharacterControllerCallbacks.s_OnCollisionDel>();

	private List<CharacterControllerCallbacks.s_OnCollisionDel> m_OnCollisionExitDelegates = new List<CharacterControllerCallbacks.s_OnCollisionDel>();

	private List<CharacterControllerCallbacks.s_OnCollisionDel> m_OnCollisionStayDelegates = new List<CharacterControllerCallbacks.s_OnCollisionDel>();

	private List<CharacterControllerCallbacks.s_OnTriggerDel> m_OnTriggerEnterDelegates = new List<CharacterControllerCallbacks.s_OnTriggerDel>();

	private List<CharacterControllerCallbacks.s_OnTriggerDel> m_OnTriggerExitDelegates = new List<CharacterControllerCallbacks.s_OnTriggerDel>();

	private List<CharacterControllerCallbacks.s_OnTriggerDel> m_OnTriggerStayDelegates = new List<CharacterControllerCallbacks.s_OnTriggerDel>();

	private delegate void s_OnControllerColliderDel(ControllerColliderHit c);

	private delegate void s_OnCollisionDel(Collision c);

	private delegate void s_OnTriggerDel(Collider c);
}
