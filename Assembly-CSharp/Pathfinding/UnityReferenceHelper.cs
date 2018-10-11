using System;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_unity_reference_helper.php")]
	[ExecuteInEditMode]
	public class UnityReferenceHelper : MonoBehaviour
	{
		public string GetGUID()
		{
			return this.guid;
		}

		public void Awake()
		{
			this.Reset();
		}

		public void Reset()
		{
			if (string.IsNullOrEmpty(this.guid))
			{
				this.guid = Pathfinding.Util.Guid.NewGuid().ToString();
				Debug.Log("Created new GUID - " + this.guid);
			}
			else
			{
				foreach (UnityReferenceHelper unityReferenceHelper in UnityEngine.Object.FindObjectsOfType(typeof(UnityReferenceHelper)) as UnityReferenceHelper[])
				{
					if (unityReferenceHelper != this && this.guid == unityReferenceHelper.guid)
					{
						this.guid = Pathfinding.Util.Guid.NewGuid().ToString();
						Debug.Log("Created new GUID - " + this.guid);
						return;
					}
				}
			}
		}

		[SerializeField]
		[HideInInspector]
		private string guid;
	}
}
