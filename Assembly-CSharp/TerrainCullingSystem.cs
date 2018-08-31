using System;
using UnityEngine;

public class TerrainCullingSystem : MonoBehaviour
{
	private void Start()
	{
		this.terrain = base.GetComponent<Terrain>();
		if (this.terrain != null)
		{
			if (this.terrain.terrainData.size.x > this.terrain.terrainData.size.z)
			{
				this.sphereSize = this.terrain.terrainData.size.x * 0.75f;
			}
			else
			{
				this.sphereSize = this.terrain.terrainData.size.z * 0.75f;
			}
			this.offsetVector = new Vector3(this.terrain.terrainData.size.x, 0f, this.terrain.terrainData.size.z) * 0.5f;
			this.group = new CullingGroup();
			this.group.targetCamera = Camera.main;
			this.heightSphereNumber = 2 * (int)(this.terrain.terrainData.size.y / this.sphereSize);
			this.heightSphereNumber = Mathf.Max(1, this.heightSphereNumber);
			this.offsetVectorUp = new Vector3(0f, this.sphereSize * 0.5f, 0f);
			for (int i = 0; i < this.heightSphereNumber; i++)
			{
				this.spheres[i] = new BoundingSphere(base.transform.position + this.offsetVector + (float)i * this.offsetVectorUp, this.sphereSize);
			}
			this.group.SetBoundingSpheres(this.spheres);
			this.group.SetBoundingSphereCount(this.heightSphereNumber);
			this.group.onStateChanged = new CullingGroup.StateChanged(this.StateChangedMethod);
			this.group.SetBoundingDistances(new float[]
			{
				this.renderingDistance
			});
			this.mainCamera = Camera.main;
			this.group.SetDistanceReferencePoint(Camera.main.transform);
			base.Invoke("CheckVisibility", 0.1f);
		}
		else
		{
			Debug.LogError("TerrainCullingSystem: no terrain on game object " + base.gameObject.name);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		for (int i = 0; i < this.heightSphereNumber; i++)
		{
			Gizmos.DrawWireSphere(base.transform.position + this.offsetVector + (float)i * this.offsetVectorUp, this.sphereSize);
		}
	}

	private void CheckVisibility()
	{
		bool flag = false;
		for (int i = 0; i < this.heightSphereNumber; i++)
		{
			if (this.group.IsVisible(i))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			this.terrain.drawHeightmap = false;
			if (this.disableTrees)
			{
				this.terrain.drawTreesAndFoliage = false;
			}
		}
	}

	public void Update()
	{
		for (int i = 0; i < this.heightSphereNumber; i++)
		{
			this.spheres[i] = new BoundingSphere(base.transform.position + this.offsetVector + (float)i * this.offsetVectorUp, this.sphereSize);
		}
		if (this.mainCamera != Camera.main)
		{
			this.mainCamera = Camera.main;
			this.group.SetDistanceReferencePoint(Camera.main.transform);
		}
	}

	private void StateChangedMethod(CullingGroupEvent evt)
	{
		bool flag = false;
		for (int i = 0; i < this.heightSphereNumber; i++)
		{
			if (this.group.IsVisible(i))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			this.terrain.drawHeightmap = true;
			if (this.disableTrees)
			{
				this.terrain.drawTreesAndFoliage = true;
			}
		}
		else
		{
			this.terrain.drawHeightmap = false;
			if (this.disableTrees)
			{
				this.terrain.drawTreesAndFoliage = false;
			}
		}
	}

	private void OnDisable()
	{
		if (this.group != null)
		{
			this.group.Dispose();
			this.group = null;
		}
	}

	[Tooltip("Max view distance is referred from camera to terrain center point")]
	public float renderingDistance = 10000f;

	private float sphereSize = 0.5f;

	private Terrain terrain;

	private CullingGroup group;

	private BoundingSphere[] spheres = new BoundingSphere[1000];

	private Vector3 offsetVector;

	private Vector3 offsetVectorUp;

	private Camera mainCamera;

	private int heightSphereNumber;

	[HideInInspector]
	public bool disableTrees;
}
