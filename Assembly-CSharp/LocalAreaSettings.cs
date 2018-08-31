using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalAreaSettings : ScriptableObject
{
	public bool collectionsCollapsed = true;

	public int listSizeCollections;

	public List<SceneCollection> currentCollections = new List<SceneCollection>();

	public bool showLoadingPoint = true;

	public int distanceFromCenter;

	public bool tiles;

	public Vector3 CenterPoint;
}
