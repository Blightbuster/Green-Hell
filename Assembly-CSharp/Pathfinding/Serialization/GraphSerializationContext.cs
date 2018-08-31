using System;
using System.IO;
using UnityEngine;

namespace Pathfinding.Serialization
{
	public class GraphSerializationContext
	{
		public GraphSerializationContext(BinaryReader reader, GraphNode[] id2NodeMapping, uint graphIndex, GraphMeta meta)
		{
			this.reader = reader;
			this.id2NodeMapping = id2NodeMapping;
			this.graphIndex = graphIndex;
			this.meta = meta;
		}

		public GraphSerializationContext(BinaryWriter writer)
		{
			this.writer = writer;
		}

		public void SerializeNodeReference(GraphNode node)
		{
			this.writer.Write((node != null) ? node.NodeIndex : -1);
		}

		public GraphNode DeserializeNodeReference()
		{
			int num = this.reader.ReadInt32();
			if (this.id2NodeMapping == null)
			{
				throw new Exception("Calling DeserializeNodeReference when serializing");
			}
			if (num == -1)
			{
				return null;
			}
			GraphNode graphNode = this.id2NodeMapping[num];
			if (graphNode == null)
			{
				throw new Exception("Invalid id (" + num + ")");
			}
			return graphNode;
		}

		public void SerializeVector3(Vector3 v)
		{
			this.writer.Write(v.x);
			this.writer.Write(v.y);
			this.writer.Write(v.z);
		}

		public Vector3 DeserializeVector3()
		{
			return new Vector3(this.reader.ReadSingle(), this.reader.ReadSingle(), this.reader.ReadSingle());
		}

		public void SerializeInt3(Int3 v)
		{
			this.writer.Write(v.x);
			this.writer.Write(v.y);
			this.writer.Write(v.z);
		}

		public Int3 DeserializeInt3()
		{
			return new Int3(this.reader.ReadInt32(), this.reader.ReadInt32(), this.reader.ReadInt32());
		}

		public int DeserializeInt(int defaultValue)
		{
			if (this.reader.BaseStream.Position <= this.reader.BaseStream.Length - 4L)
			{
				return this.reader.ReadInt32();
			}
			return defaultValue;
		}

		public float DeserializeFloat(float defaultValue)
		{
			if (this.reader.BaseStream.Position <= this.reader.BaseStream.Length - 4L)
			{
				return this.reader.ReadSingle();
			}
			return defaultValue;
		}

		public UnityEngine.Object DeserializeUnityObject()
		{
			int num = this.reader.ReadInt32();
			if (num == 2147483647)
			{
				return null;
			}
			string text = this.reader.ReadString();
			string text2 = this.reader.ReadString();
			string text3 = this.reader.ReadString();
			Type type = Type.GetType(text2);
			if (type == null)
			{
				Debug.LogError("Could not find type '" + text2 + "'. Cannot deserialize Unity reference");
				return null;
			}
			if (!string.IsNullOrEmpty(text3))
			{
				UnityReferenceHelper[] array = UnityEngine.Object.FindObjectsOfType(typeof(UnityReferenceHelper)) as UnityReferenceHelper[];
				int i = 0;
				while (i < array.Length)
				{
					if (array[i].GetGUID() == text3)
					{
						if (type == typeof(GameObject))
						{
							return array[i].gameObject;
						}
						return array[i].GetComponent(type);
					}
					else
					{
						i++;
					}
				}
			}
			UnityEngine.Object[] array2 = Resources.LoadAll(text, type);
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j].name == text || array2.Length == 1)
				{
					return array2[j];
				}
			}
			return null;
		}

		private readonly GraphNode[] id2NodeMapping;

		public readonly BinaryReader reader;

		public readonly BinaryWriter writer;

		public readonly uint graphIndex;

		public readonly GraphMeta meta;
	}
}
