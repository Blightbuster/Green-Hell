using System;
using UnityEngine;

public class ObjectToMove : MonoBehaviour
{
	private void Start()
	{
		GameObject.FindGameObjectWithTag(WorldMover.WORLDMOVERTAG).GetComponent<WorldMover>().AddObjectToMove(base.transform);
	}
}
