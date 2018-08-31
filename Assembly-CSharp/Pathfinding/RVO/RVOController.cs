using System;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding.RVO
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_controller.php")]
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Controller")]
	public class RVOController : VersionedMonoBehaviour
	{
		[Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
		public LayerMask mask
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		[Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
		public bool enableRotation
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		[Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
		public float rotationSpeed
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		[Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
		public float maxSpeed
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public MovementPlane movementPlane
		{
			get
			{
				if (this.simulator != null)
				{
					return this.simulator.movementPlane;
				}
				if (RVOSimulator.active)
				{
					return RVOSimulator.active.movementPlane;
				}
				return MovementPlane.XZ;
			}
		}

		public IAgent rvoAgent { get; private set; }

		public Simulator simulator { get; private set; }

		public Vector3 position
		{
			get
			{
				return this.To3D(this.rvoAgent.Position, this.rvoAgent.ElevationCoordinate);
			}
		}

		public Vector3 velocity
		{
			get
			{
				if (Time.deltaTime > 1E-05f)
				{
					return this.CalculateMovementDelta(Time.deltaTime) / Time.deltaTime;
				}
				return Vector3.zero;
			}
		}

		public Vector3 CalculateMovementDelta(float deltaTime)
		{
			if (this.rvoAgent == null)
			{
				return Vector3.zero;
			}
			return this.To3D(Vector2.ClampMagnitude(this.rvoAgent.CalculatedTargetPoint - this.To2D(this.tr.position), this.rvoAgent.CalculatedSpeed * deltaTime), 0f);
		}

		public Vector3 CalculateMovementDelta(Vector3 position, float deltaTime)
		{
			return this.To3D(Vector2.ClampMagnitude(this.rvoAgent.CalculatedTargetPoint - this.To2D(position), this.rvoAgent.CalculatedSpeed * deltaTime), 0f);
		}

		public void SetCollisionNormal(Vector3 normal)
		{
			this.rvoAgent.SetCollisionNormal(this.To2D(normal));
		}

		public void ForceSetVelocity(Vector3 velocity)
		{
			this.rvoAgent.ForceSetVelocity(this.To2D(velocity));
		}

		public Vector2 To2D(Vector3 p)
		{
			float num;
			return this.To2D(p, out num);
		}

		public Vector2 To2D(Vector3 p, out float elevation)
		{
			if (this.movementPlane == MovementPlane.XY)
			{
				elevation = p.z;
				return new Vector2(p.x, p.y);
			}
			elevation = p.y;
			return new Vector2(p.x, p.z);
		}

		public Vector3 To3D(Vector2 p, float elevationCoordinate)
		{
			if (this.movementPlane == MovementPlane.XY)
			{
				return new Vector3(p.x, p.y, elevationCoordinate);
			}
			return new Vector3(p.x, elevationCoordinate, p.y);
		}

		private void OnDisable()
		{
			if (this.simulator == null)
			{
				return;
			}
			this.simulator.RemoveAgent(this.rvoAgent);
		}

		private void OnEnable()
		{
			this.tr = base.transform;
			if (RVOSimulator.active == null)
			{
				Debug.LogError("No RVOSimulator component found in the scene. Please add one.");
			}
			else
			{
				this.simulator = RVOSimulator.active.GetSimulator();
				if (this.rvoAgent != null)
				{
					this.simulator.AddAgent(this.rvoAgent);
				}
				else
				{
					float elevationCoordinate;
					Vector2 position = this.To2D(base.transform.position, out elevationCoordinate);
					this.rvoAgent = this.simulator.AddAgent(position, elevationCoordinate);
					this.rvoAgent.PreCalculationCallback = new Action(this.UpdateAgentProperties);
				}
				this.UpdateAgentProperties();
			}
		}

		protected void UpdateAgentProperties()
		{
			this.rvoAgent.Radius = Mathf.Max(0.001f, this.radius);
			this.rvoAgent.AgentTimeHorizon = this.agentTimeHorizon;
			this.rvoAgent.ObstacleTimeHorizon = this.obstacleTimeHorizon;
			this.rvoAgent.Locked = this.locked;
			this.rvoAgent.MaxNeighbours = this.maxNeighbours;
			this.rvoAgent.DebugDraw = this.debug;
			this.rvoAgent.Layer = this.layer;
			this.rvoAgent.CollidesWith = this.collidesWith;
			this.rvoAgent.Priority = this.priority;
			float num;
			this.rvoAgent.Position = this.To2D(base.transform.position, out num);
			if (this.movementPlane == MovementPlane.XZ)
			{
				this.rvoAgent.Height = this.height;
				this.rvoAgent.ElevationCoordinate = num + this.center - 0.5f * this.height;
			}
			else
			{
				this.rvoAgent.Height = 1f;
				this.rvoAgent.ElevationCoordinate = 0f;
			}
		}

		public void SetTarget(Vector3 pos, float speed, float maxSpeed)
		{
			if (this.simulator == null)
			{
				return;
			}
			this.rvoAgent.SetTarget(this.To2D(pos), speed, maxSpeed);
			if (this.lockWhenNotMoving)
			{
				this.locked = (speed < 0.001f);
			}
		}

		public void Move(Vector3 vel)
		{
			if (this.simulator == null)
			{
				return;
			}
			Vector2 b = this.To2D(vel);
			float magnitude = b.magnitude;
			this.rvoAgent.SetTarget(this.To2D(this.tr.position) + b, magnitude, magnitude);
			if (this.lockWhenNotMoving)
			{
				this.locked = (magnitude < 0.001f);
			}
		}

		[Obsolete("Use transform.position instead, the RVOController can now handle that without any issues.")]
		public void Teleport(Vector3 pos)
		{
			this.tr.position = pos;
		}

		private void OnDrawGizmos()
		{
			Color color = RVOController.GizmoColor * ((!this.locked) ? 1f : 0.5f);
			if (this.movementPlane == MovementPlane.XY)
			{
				Draw.Gizmos.Cylinder(base.transform.position, Vector3.forward, 0f, this.radius, color);
			}
			else
			{
				Draw.Gizmos.Cylinder(base.transform.position + this.To3D(Vector2.zero, this.center - this.height * 0.5f), this.To3D(Vector2.zero, 1f), this.height, this.radius, color);
			}
		}

		[Tooltip("Radius of the agent")]
		public float radius = 0.5f;

		[Tooltip("Height of the agent. In world units")]
		[HideInInspector]
		public float height = 2f;

		[Tooltip("A locked unit cannot move. Other units will still avoid it. But avoidance quality is not the best")]
		public bool locked;

		[Tooltip("Automatically set #locked to true when desired velocity is approximately zero")]
		public bool lockWhenNotMoving = true;

		[Tooltip("How far into the future to look for collisions with other agents (in seconds)")]
		public float agentTimeHorizon = 2f;

		[Tooltip("How far into the future to look for collisions with obstacles (in seconds)")]
		public float obstacleTimeHorizon = 2f;

		[Tooltip("Max number of other agents to take into account.\nA smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.")]
		public int maxNeighbours = 10;

		public RVOLayer layer = RVOLayer.DefaultAgent;

		[AstarEnumFlag]
		public RVOLayer collidesWith = (RVOLayer)(-1);

		[Obsolete]
		[HideInInspector]
		public float wallAvoidForce = 1f;

		[Obsolete]
		[HideInInspector]
		public float wallAvoidFalloff = 1f;

		[Tooltip("How strongly other agents will avoid this agent")]
		[Range(0f, 1f)]
		public float priority = 0.5f;

		[Tooltip("Center of the agent relative to the pivot point of this game object")]
		[HideInInspector]
		public float center = 1f;

		protected Transform tr;

		public bool debug;

		private static readonly Color GizmoColor = new Color(0.9411765f, 0.8352941f, 0.117647059f);
	}
}
