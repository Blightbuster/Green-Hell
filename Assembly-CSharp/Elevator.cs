using System;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
	private void Awake()
	{
		Elevator.s_AllElevators.Add(this);
		this.m_Animator = base.gameObject.GetComponent<Animator>();
	}

	private void OnDestroy()
	{
		Elevator.s_AllElevators.Remove(this);
	}

	public void Save()
	{
		SaveGame.SaveVal("ElevatorPos" + base.name, base.transform.localPosition);
		SaveGame.SaveVal("ElevatorState" + base.name, this.m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
	}

	public void Load()
	{
		int stateHashName = SaveGame.LoadIVal("ElevatorState" + base.name);
		this.m_Animator.CrossFade(stateHashName, 0f, 0, 1f);
		base.transform.localPosition = SaveGame.LoadV3Val("ElevatorPos" + base.name);
	}

	private Animator m_Animator;

	public static List<Elevator> s_AllElevators = new List<Elevator>();
}
