using System;
using System.IO;
using System.Text;
using UnityEngine;

public class RTPObjExporter
{
	public static string MeshToString(MeshFilter mf)
	{
		Mesh sharedMesh = mf.sharedMesh;
		Material[] sharedMaterials = mf.GetComponent<Renderer>().sharedMaterials;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("g ").Append(mf.name).Append("\n");
		foreach (Vector3 vector in sharedMesh.vertices)
		{
			stringBuilder.Append(string.Format("v {0} {1} {2}\n", -vector.x, vector.y, vector.z));
		}
		stringBuilder.Append("\n");
		foreach (Vector3 vector2 in sharedMesh.normals)
		{
			stringBuilder.Append(string.Format("vn {0} {1} {2}\n", -vector2.x, vector2.y, vector2.z));
		}
		stringBuilder.Append("\n");
		Vector2[] uv = sharedMesh.uv;
		for (int i = 0; i < uv.Length; i++)
		{
			Vector3 vector3 = uv[i];
			stringBuilder.Append(string.Format("vt {0} {1}\n", vector3.x, vector3.y));
		}
		for (int j = 0; j < sharedMesh.subMeshCount; j++)
		{
			stringBuilder.Append("\n");
			stringBuilder.Append("usemtl ").Append(sharedMaterials[j].name).Append("\n");
			stringBuilder.Append("usemap ").Append(sharedMaterials[j].name).Append("\n");
			int[] triangles = sharedMesh.GetTriangles(j);
			for (int k = 0; k < triangles.Length; k += 3)
			{
				stringBuilder.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles[k + 2] + 1, triangles[k + 1] + 1, triangles[k] + 1));
			}
		}
		return stringBuilder.ToString();
	}

	public static void MeshToFile(MeshFilter mf, string filename)
	{
		using (StreamWriter streamWriter = new StreamWriter(filename))
		{
			streamWriter.Write(RTPObjExporter.MeshToString(mf));
		}
	}
}
