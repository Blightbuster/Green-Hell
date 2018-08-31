using System;
using UnityEngine;

public class ColliderScene : MonoBehaviour
{
	private void Start()
	{
		GameObject.FindGameObjectWithTag(ColliderStreamerManager.COLLIDERSTREAMERMANAGERTAG).GetComponent<ColliderStreamerManager>().AddColliderScene(this);
		GameObject gameObject = GameObject.FindGameObjectWithTag(WorldMover.WORLDMOVERTAG);
		if (gameObject)
		{
			gameObject.GetComponent<WorldMover>().AddObjectToMove(base.transform);
		}
	}

	public string sceneName;
}
