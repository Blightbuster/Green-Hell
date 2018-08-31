using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	[AddComponentMenu("Pathfinding/Link2")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_node_link2.php")]
	public class NodeLink2 : GraphModifier
	{
		public static NodeLink2 GetNodeLink(GraphNode node)
		{
			NodeLink2 result;
			NodeLink2.reference.TryGetValue(node, out result);
			return result;
		}

		public Transform StartTransform
		{
			get
			{
				return base.transform;
			}
		}

		public Transform EndTransform
		{
			get
			{
				return this.end;
			}
		}

		public PointNode startNode { get; private set; }

		public PointNode endNode { get; private set; }

		[Obsolete("Use startNode instead (lowercase s)")]
		public GraphNode StartNode
		{
			get
			{
				return this.startNode;
			}
		}

		[Obsolete("Use endNode instead (lowercase e)")]
		public GraphNode EndNode
		{
			get
			{
				return this.endNode;
			}
		}

		public override void OnPostScan()
		{
			this.InternalOnPostScan();
		}

		public void InternalOnPostScan()
		{
			if (this.EndTransform == null || this.StartTransform == null)
			{
				return;
			}
			if (AstarPath.active.data.pointGraph == null)
			{
				PointGraph pointGraph = AstarPath.active.data.AddGraph(typeof(PointGraph)) as PointGraph;
				pointGraph.name = "PointGraph (used for node links)";
			}
			if (this.startNode != null && this.startNode.Destroyed)
			{
				NodeLink2.reference.Remove(this.startNode);
				this.startNode = null;
			}
			if (this.endNode != null && this.endNode.Destroyed)
			{
				NodeLink2.reference.Remove(this.endNode);
				this.endNode = null;
			}
			if (this.startNode == null)
			{
				this.startNode = AstarPath.active.data.pointGraph.AddNode((Int3)this.StartTransform.position);
			}
			if (this.endNode == null)
			{
				this.endNode = AstarPath.active.data.pointGraph.AddNode((Int3)this.EndTransform.position);
			}
			this.connectedNode1 = null;
			this.connectedNode2 = null;
			if (this.startNode == null || this.endNode == null)
			{
				this.startNode = null;
				this.endNode = null;
				return;
			}
			this.postScanCalled = true;
			NodeLink2.reference[this.startNode] = this;
			NodeLink2.reference[this.endNode] = this;
			this.Apply(true);
		}

		public override void OnGraphsPostUpdate()
		{
			if (AstarPath.active.isScanning)
			{
				return;
			}
			if (this.connectedNode1 != null && this.connectedNode1.Destroyed)
			{
				this.connectedNode1 = null;
			}
			if (this.connectedNode2 != null && this.connectedNode2.Destroyed)
			{
				this.connectedNode2 = null;
			}
			if (!this.postScanCalled)
			{
				this.OnPostScan();
			}
			else
			{
				this.Apply(false);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (Application.isPlaying && AstarPath.active != null && AstarPath.active.data != null && AstarPath.active.data.pointGraph != null && !AstarPath.active.isScanning)
			{
				AstarPath.active.AddWorkItem(new Action(this.OnGraphsPostUpdate));
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			this.postScanCalled = false;
			if (this.startNode != null)
			{
				NodeLink2.reference.Remove(this.startNode);
			}
			if (this.endNode != null)
			{
				NodeLink2.reference.Remove(this.endNode);
			}
			if (this.startNode != null && this.endNode != null)
			{
				this.startNode.RemoveConnection(this.endNode);
				this.endNode.RemoveConnection(this.startNode);
				if (this.connectedNode1 != null && this.connectedNode2 != null)
				{
					this.startNode.RemoveConnection(this.connectedNode1);
					this.connectedNode1.RemoveConnection(this.startNode);
					this.endNode.RemoveConnection(this.connectedNode2);
					this.connectedNode2.RemoveConnection(this.endNode);
				}
			}
		}

		private void RemoveConnections(GraphNode node)
		{
			node.ClearConnections(true);
		}

		[ContextMenu("Recalculate neighbours")]
		private void ContextApplyForce()
		{
			if (Application.isPlaying)
			{
				this.Apply(true);
				if (AstarPath.active != null)
				{
					AstarPath.active.FloodFill();
				}
			}
		}

		public void Apply(bool forceNewCheck)
		{
			NNConstraint none = NNConstraint.None;
			int graphIndex = (int)this.startNode.GraphIndex;
			none.graphMask = ~(1 << graphIndex);
			this.startNode.SetPosition((Int3)this.StartTransform.position);
			this.endNode.SetPosition((Int3)this.EndTransform.position);
			this.RemoveConnections(this.startNode);
			this.RemoveConnections(this.endNode);
			uint cost = (uint)Mathf.RoundToInt((float)((Int3)(this.StartTransform.position - this.EndTransform.position)).costMagnitude * this.costFactor);
			this.startNode.AddConnection(this.endNode, cost);
			this.endNode.AddConnection(this.startNode, cost);
			if (this.connectedNode1 == null || forceNewCheck)
			{
				NNInfo nearest = AstarPath.active.GetNearest(this.StartTransform.position, none);
				this.connectedNode1 = nearest.node;
				this.clamped1 = nearest.position;
			}
			if (this.connectedNode2 == null || forceNewCheck)
			{
				NNInfo nearest2 = AstarPath.active.GetNearest(this.EndTransform.position, none);
				this.connectedNode2 = nearest2.node;
				this.clamped2 = nearest2.position;
			}
			if (this.connectedNode2 == null || this.connectedNode1 == null)
			{
				return;
			}
			this.connectedNode1.AddConnection(this.startNode, (uint)Mathf.RoundToInt((float)((Int3)(this.clamped1 - this.StartTransform.position)).costMagnitude * this.costFactor));
			if (!this.oneWay)
			{
				this.connectedNode2.AddConnection(this.endNode, (uint)Mathf.RoundToInt((float)((Int3)(this.clamped2 - this.EndTransform.position)).costMagnitude * this.costFactor));
			}
			if (!this.oneWay)
			{
				this.startNode.AddConnection(this.connectedNode1, (uint)Mathf.RoundToInt((float)((Int3)(this.clamped1 - this.StartTransform.position)).costMagnitude * this.costFactor));
			}
			this.endNode.AddConnection(this.connectedNode2, (uint)Mathf.RoundToInt((float)((Int3)(this.clamped2 - this.EndTransform.position)).costMagnitude * this.costFactor));
		}

		public virtual void OnDrawGizmosSelected()
		{
			this.OnDrawGizmos(true);
		}

		public void OnDrawGizmos()
		{
			this.OnDrawGizmos(false);
		}

		public void OnDrawGizmos(bool selected)
		{
			Color color = (!selected) ? NodeLink2.GizmosColor : NodeLink2.GizmosColorSelected;
			if (this.StartTransform != null)
			{
				Draw.Gizmos.CircleXZ(this.StartTransform.position, 0.4f, color, 0f, 6.28318548f, 10);
			}
			if (this.EndTransform != null)
			{
				Draw.Gizmos.CircleXZ(this.EndTransform.position, 0.4f, color, 0f, 6.28318548f, 10);
			}
			if (this.StartTransform != null && this.EndTransform != null)
			{
				Draw.Gizmos.Bezier(this.StartTransform.position, this.EndTransform.position, color);
				if (selected)
				{
					Vector3 normalized = Vector3.Cross(Vector3.up, this.EndTransform.position - this.StartTransform.position).normalized;
					Draw.Gizmos.Bezier(this.StartTransform.position + normalized * 0.1f, this.EndTransform.position + normalized * 0.1f, color);
					Draw.Gizmos.Bezier(this.StartTransform.position - normalized * 0.1f, this.EndTransform.position - normalized * 0.1f, color);
				}
			}
		}

		internal static void SerializeReferences(GraphSerializationContext ctx)
		{
			List<NodeLink2> modifiersOfType = GraphModifier.GetModifiersOfType<NodeLink2>();
			ctx.writer.Write(modifiersOfType.Count);
			foreach (NodeLink2 nodeLink in modifiersOfType)
			{
				ctx.writer.Write(nodeLink.uniqueID);
				ctx.SerializeNodeReference(nodeLink.startNode);
				ctx.SerializeNodeReference(nodeLink.endNode);
				ctx.SerializeNodeReference(nodeLink.connectedNode1);
				ctx.SerializeNodeReference(nodeLink.connectedNode2);
				ctx.SerializeVector3(nodeLink.clamped1);
				ctx.SerializeVector3(nodeLink.clamped2);
				ctx.writer.Write(nodeLink.postScanCalled);
			}
		}

		internal static void DeserializeReferences(GraphSerializationContext ctx)
		{
			int num = ctx.reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				ulong key = ctx.reader.ReadUInt64();
				GraphNode graphNode = ctx.DeserializeNodeReference();
				GraphNode graphNode2 = ctx.DeserializeNodeReference();
				GraphNode graphNode3 = ctx.DeserializeNodeReference();
				GraphNode graphNode4 = ctx.DeserializeNodeReference();
				Vector3 vector = ctx.DeserializeVector3();
				Vector3 vector2 = ctx.DeserializeVector3();
				bool flag = ctx.reader.ReadBoolean();
				GraphModifier graphModifier;
				if (!GraphModifier.usedIDs.TryGetValue(key, out graphModifier))
				{
					throw new Exception("Tried to deserialize a NodeLink2 reference, but the link could not be found in the scene.\nIf a NodeLink2 is included in serialized graph data, the same NodeLink2 component must be present in the scene when loading the graph data.");
				}
				NodeLink2 nodeLink = graphModifier as NodeLink2;
				if (!(nodeLink != null))
				{
					throw new Exception("Tried to deserialize a NodeLink2 reference, but the link was not of the correct type or it has been destroyed.\nIf a NodeLink2 is included in serialized graph data, the same NodeLink2 component must be present in the scene when loading the graph data.");
				}
				if (graphNode != null)
				{
					NodeLink2.reference[graphNode] = nodeLink;
				}
				if (graphNode2 != null)
				{
					NodeLink2.reference[graphNode2] = nodeLink;
				}
				if (nodeLink.startNode != null)
				{
					NodeLink2.reference.Remove(nodeLink.startNode);
				}
				if (nodeLink.endNode != null)
				{
					NodeLink2.reference.Remove(nodeLink.endNode);
				}
				nodeLink.startNode = (graphNode as PointNode);
				nodeLink.endNode = (graphNode2 as PointNode);
				nodeLink.connectedNode1 = graphNode3;
				nodeLink.connectedNode2 = graphNode4;
				nodeLink.postScanCalled = flag;
				nodeLink.clamped1 = vector;
				nodeLink.clamped2 = vector2;
			}
		}

		protected static Dictionary<GraphNode, NodeLink2> reference = new Dictionary<GraphNode, NodeLink2>();

		public Transform end;

		public float costFactor = 1f;

		public bool oneWay;

		private GraphNode connectedNode1;

		private GraphNode connectedNode2;

		private Vector3 clamped1;

		private Vector3 clamped2;

		private bool postScanCalled;

		private static readonly Color GizmosColor = new Color(0.807843149f, 0.533333361f, 0.1882353f, 0.5f);

		private static readonly Color GizmosColorSelected = new Color(0.921568632f, 0.482352942f, 0.1254902f, 1f);
	}
}
