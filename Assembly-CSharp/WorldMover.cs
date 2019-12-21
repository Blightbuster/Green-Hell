using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldMover : MonoBehaviour
{
	public void Start()
	{
		this.streamerMajor.worldMover = this;
		List<Streamer> list = new List<Streamer>();
		list.AddRange(this.streamerMinors);
		list.Remove(this.streamerMajor);
		this.streamerMinors = list.ToArray();
		this.worldSize = new Vector3((float)(this.streamerMajor.sceneCollection.xSize * (this.streamerMajor.sceneCollection.xLimitsy - this.streamerMajor.sceneCollection.xLimitsx + 1)), (float)(this.streamerMajor.sceneCollection.ySize * (this.streamerMajor.sceneCollection.yLimitsy - this.streamerMajor.sceneCollection.yLimitsx + 1)), (float)(this.streamerMajor.sceneCollection.zSize * (this.streamerMajor.sceneCollection.zLimitsy - this.streamerMajor.sceneCollection.zLimitsx + 1)));
	}

	public void Update()
	{
		this.playerPositionMovedLooped = this.streamerMajor.player.position - this.currentMove;
		if (this.streamerMajor.looping)
		{
			this.playerPositionMovedLooped = new Vector3((this.worldSize.x != 0f) ? (this.modf(this.playerPositionMovedLooped.x + (float)Mathf.Abs(this.streamerMajor.sceneCollection.xSize * this.streamerMajor.sceneCollection.xLimitsx), this.worldSize.x) + (float)(this.streamerMajor.sceneCollection.xSize * this.streamerMajor.sceneCollection.xLimitsx)) : this.playerPositionMovedLooped.x, (this.worldSize.y != 0f) ? (this.modf(this.playerPositionMovedLooped.y + (float)Mathf.Abs(this.streamerMajor.sceneCollection.ySize * this.streamerMajor.sceneCollection.yLimitsx), this.worldSize.y) + (float)(this.streamerMajor.sceneCollection.ySize * this.streamerMajor.sceneCollection.yLimitsx)) : this.playerPositionMovedLooped.y, (this.worldSize.z != 0f) ? (this.modf(this.playerPositionMovedLooped.z + (float)Mathf.Abs(this.streamerMajor.sceneCollection.zSize * this.streamerMajor.sceneCollection.zLimitsx), this.worldSize.z) + (float)(this.streamerMajor.sceneCollection.zSize * this.streamerMajor.sceneCollection.zLimitsx)) : this.playerPositionMovedLooped.z);
		}
	}

	public void CheckMoverDistance(int xPosCurrent, int yPosCurrent, int zPosCurrent)
	{
		if (Mathf.Abs((float)xPosCurrent - this.xCurrentTile) > this.xTileRange || Mathf.Abs((float)yPosCurrent - this.yCurrentTile) > this.yTileRange || Mathf.Abs((float)zPosCurrent - this.zCurrentTile) > this.zTileRange)
		{
			this.MoveWorld(xPosCurrent, yPosCurrent, zPosCurrent);
		}
	}

	private void MoveWorld(int xPosCurrent, int yPosCurrent, int zPosCurrent)
	{
		Vector3 b = new Vector3(((float)xPosCurrent - this.xCurrentTile) * (float)this.streamerMajor.sceneCollection.xSize, ((float)yPosCurrent - this.yCurrentTile) * (float)this.streamerMajor.sceneCollection.ySize, ((float)zPosCurrent - this.zCurrentTile) * (float)this.streamerMajor.sceneCollection.zSize);
		this.currentMove -= b;
		this.streamerMajor.player.position -= b;
		foreach (SceneSplit sceneSplit in this.streamerMajor.loadedScenes)
		{
			if (sceneSplit.loaded && sceneSplit.sceneGo != null)
			{
				sceneSplit.sceneGo.transform.position -= b;
			}
		}
		foreach (Transform transform in this.objectsToMove)
		{
			if (transform != null)
			{
				transform.position -= b;
			}
		}
		this.xCurrentTile = (float)xPosCurrent;
		this.yCurrentTile = (float)yPosCurrent;
		this.zCurrentTile = (float)zPosCurrent;
		this.streamerMajor.currentMove = this.currentMove;
		foreach (Streamer streamer in this.streamerMinors)
		{
			streamer.currentMove = this.currentMove;
			foreach (SceneSplit sceneSplit2 in streamer.loadedScenes)
			{
				if (sceneSplit2.loaded && sceneSplit2.sceneGo != null)
				{
					sceneSplit2.sceneGo.transform.position -= b;
				}
			}
		}
	}

	public void MoveObject(Transform objectTransform)
	{
		objectTransform.position += this.currentMove;
	}

	public void AddObjectToMove(Transform objectToMove)
	{
		base.transform.position += this.currentMove;
		this.objectsToMove.Add(objectToMove);
	}

	private float modf(float x, float m)
	{
		return (x % m + m) % m;
	}

	public static string WORLDMOVERTAG = "WorldMover";

	[Tooltip("Frequency distance of world position restarting, distance in is grid elements.")]
	public float xTileRange = 2f;

	[Tooltip("Frequency distance of world position restarting, distance in is grid elements.")]
	public float yTileRange = 2f;

	[Tooltip("Frequency distance of world position restarting, distance in is grid elements.")]
	public float zTileRange = 2f;

	[HideInInspector]
	public float xCurrentTile;

	[HideInInspector]
	public float yCurrentTile;

	[HideInInspector]
	public float zCurrentTile;

	[Tooltip("Drag and drop here, your _Streamer_Major prefab from scene hierarchy.")]
	public Streamer streamerMajor;

	[Tooltip("Drag and drop here, your all _Streamer_Minors prefabs from scene hierarchy.")]
	public Streamer[] streamerMinors;

	[Tooltip("Differences between real  and restarted player position. Useful in AI and network communications.")]
	public Vector3 currentMove = Vector3.zero;

	[HideInInspector]
	public List<Transform> objectsToMove = new List<Transform>();

	[Tooltip("Debug value used for client-server communication it's position without floating point fix and looping")]
	public Vector3 playerPositionMovedLooped;

	private Vector3 worldSize;
}
