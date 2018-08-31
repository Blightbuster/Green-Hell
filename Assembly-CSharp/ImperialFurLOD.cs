using System;
using UnityEngine;

public class ImperialFurLOD : MonoBehaviour
{
	private void Start()
	{
		this.material = base.gameObject.GetComponent<Renderer>().material;
		int num = this.material.shader.name.LastIndexOf('/');
		this.shaderBase = this.material.shader.name.Substring(0, num + 1);
		this.shaders = new Shader[6];
		this.shaders[0] = Shader.Find(this.shaderBase + "40 Shell");
		this.shaders[1] = Shader.Find(this.shaderBase + "20 Shell");
		this.shaders[2] = Shader.Find(this.shaderBase + "10 Shell");
		this.shaders[3] = Shader.Find(this.shaderBase + " 5 Shell");
		this.shaders[4] = Shader.Find(this.shaderBase + " 2 Shell");
		this.shaders[5] = Shader.Find(this.shaderBase + " 1 Shell");
		this.lodLevel = -1;
		this.physicsScript = base.GetComponent<ImperialFurPhysics>();
	}

	private void Update()
	{
		Vector3 lhs = base.transform.position - Camera.main.transform.position;
		float num = Vector3.Dot(lhs, Camera.main.transform.forward);
		if (num > this.from2To1)
		{
			if (this.lodLevel != 5)
			{
				this.lodLevel = 5;
				this.material.shader = this.shaders[5];
				if (this.physicsScript != null)
				{
					this.physicsScript.UpdatePhysics();
				}
			}
		}
		else if (num > this.from5To2)
		{
			if (this.lodLevel != 4)
			{
				this.lodLevel = 4;
				this.material.shader = this.shaders[4];
				if (this.physicsScript != null)
				{
					this.physicsScript.UpdatePhysics();
				}
			}
		}
		else if (num > this.from10To5)
		{
			if (this.lodLevel != 3)
			{
				this.lodLevel = 3;
				this.material.shader = this.shaders[3];
				if (this.physicsScript != null)
				{
					this.physicsScript.UpdatePhysics();
				}
			}
		}
		else if (num > this.from20To10)
		{
			if (this.lodLevel != 2)
			{
				this.lodLevel = 2;
				this.material.shader = this.shaders[2];
				if (this.physicsScript != null)
				{
					this.physicsScript.UpdatePhysics();
				}
			}
		}
		else if (num > this.from40To20)
		{
			if (this.lodLevel != 1)
			{
				this.lodLevel = 1;
				this.material.shader = this.shaders[1];
				if (this.physicsScript != null)
				{
					this.physicsScript.UpdatePhysics();
				}
			}
		}
		else if (this.lodLevel != 0)
		{
			this.lodLevel = 0;
			this.material.shader = this.shaders[0];
			if (this.physicsScript != null)
			{
				this.physicsScript.UpdatePhysics();
			}
		}
	}

	public void Reset()
	{
		this.lodLevel = -1;
	}

	public float from40To20;

	public float from20To10;

	public float from10To5;

	public float from5To2;

	public float from2To1;

	private Material material;

	private string shaderBase;

	private Shader[] shaders;

	private int lodLevel;

	private ImperialFurPhysics physicsScript;
}
