using System;
using UnityEngine;

public class SimpleGPUInstancingExample : MonoBehaviour
{
	private void Awake()
	{
		this.InstancedMaterial.enableInstancing = true;
		int num = 5;
		for (int i = 0; i < 1000; i++)
		{
			Transform transform = UnityEngine.Object.Instantiate<Transform>(this.Prefab, new Vector3((float)UnityEngine.Random.Range(-num, num), (float)(num + UnityEngine.Random.Range(-num, num)), (float)UnityEngine.Random.Range(-num, num)), Quaternion.identity);
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			Color value = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
			materialPropertyBlock.SetColor("_Color", value);
			transform.GetComponent<MeshRenderer>().SetPropertyBlock(materialPropertyBlock);
		}
	}

	public Transform Prefab;

	public Material InstancedMaterial;
}
