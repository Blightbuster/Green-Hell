using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding.RVO.Sampled
{
	public class Agent : IAgent
	{
		public Agent(Vector2 pos, float elevationCoordinate)
		{
			this.AgentTimeHorizon = 2f;
			this.ObstacleTimeHorizon = 2f;
			this.Height = 5f;
			this.Radius = 5f;
			this.MaxNeighbours = 10;
			this.Locked = false;
			this.Position = pos;
			this.ElevationCoordinate = elevationCoordinate;
			this.Layer = RVOLayer.DefaultAgent;
			this.CollidesWith = (RVOLayer)(-1);
			this.Priority = 0.5f;
			this.CalculatedTargetPoint = pos;
			this.CalculatedSpeed = 0f;
			this.SetTarget(pos, 0f, 0f);
		}

		public Vector2 Position { get; set; }

		public float ElevationCoordinate { get; set; }

		public Vector2 CalculatedTargetPoint { get; private set; }

		public float CalculatedSpeed { get; private set; }

		public bool Locked { get; set; }

		public float Radius { get; set; }

		public float Height { get; set; }

		public float AgentTimeHorizon { get; set; }

		public float ObstacleTimeHorizon { get; set; }

		public int MaxNeighbours { get; set; }

		public int NeighbourCount { get; private set; }

		public RVOLayer Layer { get; set; }

		public RVOLayer CollidesWith { get; set; }

		public bool DebugDraw
		{
			get
			{
				return this.debugDraw;
			}
			set
			{
				this.debugDraw = (value && this.simulator != null && !this.simulator.Multithreading);
			}
		}

		public float Priority { get; set; }

		public Action PreCalculationCallback { private get; set; }

		public void SetTarget(Vector2 targetPoint, float desiredSpeed, float maxSpeed)
		{
			maxSpeed = Math.Max(maxSpeed, 0f);
			desiredSpeed = Math.Min(Math.Max(desiredSpeed, 0f), maxSpeed);
			this.nextTargetPoint = targetPoint;
			this.nextDesiredSpeed = desiredSpeed;
			this.nextMaxSpeed = maxSpeed;
		}

		public void SetCollisionNormal(Vector2 normal)
		{
			this.collisionNormal = normal;
		}

		public void ForceSetVelocity(Vector2 velocity)
		{
			this.CalculatedTargetPoint = this.position + velocity * 1000f;
			this.CalculatedSpeed = velocity.magnitude;
			this.manuallyControlled = true;
		}

		public List<ObstacleVertex> NeighbourObstacles
		{
			get
			{
				return null;
			}
		}

		public void BufferSwitch()
		{
			this.radius = this.Radius;
			this.height = this.Height;
			this.maxSpeed = this.nextMaxSpeed;
			this.desiredSpeed = this.nextDesiredSpeed;
			this.agentTimeHorizon = this.AgentTimeHorizon;
			this.obstacleTimeHorizon = this.ObstacleTimeHorizon;
			this.maxNeighbours = this.MaxNeighbours;
			this.locked = this.Locked;
			this.position = this.Position;
			this.elevationCoordinate = this.ElevationCoordinate;
			this.collidesWith = this.CollidesWith;
			this.layer = this.Layer;
			this.desiredTargetPointInVelocitySpace = this.nextTargetPoint - this.position;
			this.currentVelocity = (this.CalculatedTargetPoint - this.position).normalized * this.CalculatedSpeed;
			this.desiredVelocity = this.desiredTargetPointInVelocitySpace.normalized * this.desiredSpeed;
			if (this.collisionNormal != Vector2.zero)
			{
				this.collisionNormal.Normalize();
				float num = Vector2.Dot(this.currentVelocity, this.collisionNormal);
				if (num < 0f)
				{
					this.currentVelocity -= this.collisionNormal * num;
				}
				this.collisionNormal = Vector2.zero;
			}
		}

		public void PreCalculation()
		{
			if (this.PreCalculationCallback != null)
			{
				this.PreCalculationCallback();
			}
		}

		public void PostCalculation()
		{
			if (!this.manuallyControlled)
			{
				this.CalculatedTargetPoint = this.calculatedTargetPoint;
				this.CalculatedSpeed = this.calculatedSpeed;
			}
			List<ObstacleVertex> list = this.obstaclesBuffered;
			this.obstaclesBuffered = this.obstacles;
			this.obstacles = list;
			this.manuallyControlled = false;
		}

		public void CalculateNeighbours()
		{
			this.neighbours.Clear();
			this.neighbourDists.Clear();
			if (this.MaxNeighbours > 0 && !this.locked)
			{
				this.simulator.Quadtree.Query(this.position, this.maxSpeed, this.agentTimeHorizon, this.radius, this);
			}
			this.NeighbourCount = this.neighbours.Count;
		}

		private static float Sqr(float x)
		{
			return x * x;
		}

		internal float InsertAgentNeighbour(Agent agent, float rangeSq)
		{
			if (this == agent || (agent.layer & this.collidesWith) == (RVOLayer)0)
			{
				return rangeSq;
			}
			float sqrMagnitude = (agent.position - this.position).sqrMagnitude;
			if (sqrMagnitude < rangeSq)
			{
				if (this.neighbours.Count < this.maxNeighbours)
				{
					this.neighbours.Add(null);
					this.neighbourDists.Add(float.PositiveInfinity);
				}
				int num = this.neighbours.Count - 1;
				if (sqrMagnitude < this.neighbourDists[num])
				{
					while (num != 0 && sqrMagnitude < this.neighbourDists[num - 1])
					{
						this.neighbours[num] = this.neighbours[num - 1];
						this.neighbourDists[num] = this.neighbourDists[num - 1];
						num--;
					}
					this.neighbours[num] = agent;
					this.neighbourDists[num] = sqrMagnitude;
				}
				if (this.neighbours.Count == this.maxNeighbours)
				{
					rangeSq = this.neighbourDists[this.neighbourDists.Count - 1];
				}
			}
			return rangeSq;
		}

		private static Vector3 FromXZ(Vector2 p)
		{
			return new Vector3(p.x, 0f, p.y);
		}

		private static Vector2 ToXZ(Vector3 p)
		{
			return new Vector2(p.x, p.z);
		}

		private Vector2 To2D(Vector3 p, out float elevation)
		{
			if (this.simulator.movementPlane == MovementPlane.XY)
			{
				elevation = p.z;
				return new Vector2(p.x, p.y);
			}
			elevation = p.y;
			return new Vector2(p.x, p.z);
		}

		private static void DrawVO(Vector2 circleCenter, float radius, Vector2 origin)
		{
			float num = Mathf.Atan2((origin - circleCenter).y, (origin - circleCenter).x);
			float num2 = radius / (origin - circleCenter).magnitude;
			float num3 = (num2 > 1f) ? 0f : Mathf.Abs(Mathf.Acos(num2));
			Draw.Debug.CircleXZ(Agent.FromXZ(circleCenter), radius, Color.black, num - num3, num + num3, 40);
			Vector2 vector = new Vector2(Mathf.Cos(num - num3), Mathf.Sin(num - num3)) * radius;
			Vector2 vector2 = new Vector2(Mathf.Cos(num + num3), Mathf.Sin(num + num3)) * radius;
			Vector2 p = -new Vector2(-vector.y, vector.x);
			Vector2 p2 = new Vector2(-vector2.y, vector2.x);
			vector += circleCenter;
			vector2 += circleCenter;
			Debug.DrawRay(Agent.FromXZ(vector), Agent.FromXZ(p).normalized * 100f, Color.black);
			Debug.DrawRay(Agent.FromXZ(vector2), Agent.FromXZ(p2).normalized * 100f, Color.black);
		}

		internal void CalculateVelocity(Simulator.WorkerContext context)
		{
			if (this.manuallyControlled)
			{
				return;
			}
			if (this.locked)
			{
				this.calculatedSpeed = 0f;
				this.calculatedTargetPoint = this.position;
				return;
			}
			Agent.VOBuffer vos = context.vos;
			vos.Clear();
			this.GenerateObstacleVOs(vos);
			this.GenerateNeighbourAgentVOs(vos);
			if (!Agent.BiasDesiredVelocity(vos, ref this.desiredVelocity, ref this.desiredTargetPointInVelocitySpace, this.simulator.symmetryBreakingBias))
			{
				this.calculatedTargetPoint = this.desiredTargetPointInVelocitySpace + this.position;
				this.calculatedSpeed = this.desiredSpeed;
				if (this.DebugDraw)
				{
					Draw.Debug.CrossXZ(Agent.FromXZ(this.calculatedTargetPoint), Color.white, 1f);
				}
				return;
			}
			Vector2 vector = Vector2.zero;
			vector = this.GradientDescent(vos, this.currentVelocity, this.desiredVelocity);
			if (this.DebugDraw)
			{
				Draw.Debug.CrossXZ(Agent.FromXZ(vector + this.position), Color.white, 1f);
			}
			this.calculatedTargetPoint = this.position + vector;
			this.calculatedSpeed = Mathf.Min(vector.magnitude, this.maxSpeed);
		}

		private static Color Rainbow(float v)
		{
			Color result = new Color(v, 0f, 0f);
			if (result.r > 1f)
			{
				result.g = result.r - 1f;
				result.r = 1f;
			}
			if (result.g > 1f)
			{
				result.b = result.g - 1f;
				result.g = 1f;
			}
			return result;
		}

		private void GenerateObstacleVOs(Agent.VOBuffer vos)
		{
			float num = this.maxSpeed * this.obstacleTimeHorizon;
			for (int i = 0; i < this.simulator.obstacles.Count; i++)
			{
				ObstacleVertex obstacleVertex = this.simulator.obstacles[i];
				ObstacleVertex obstacleVertex2 = obstacleVertex;
				do
				{
					if (obstacleVertex2.ignore || (obstacleVertex2.layer & this.collidesWith) == (RVOLayer)0)
					{
						obstacleVertex2 = obstacleVertex2.next;
					}
					else
					{
						float a;
						Vector2 vector = this.To2D(obstacleVertex2.position, out a);
						float b;
						Vector2 vector2 = this.To2D(obstacleVertex2.next.position, out b);
						Vector2 normalized = (vector2 - vector).normalized;
						float num2 = Agent.VO.SignedDistanceFromLine(vector, normalized, this.position);
						if (num2 >= -0.01f && num2 < num)
						{
							float t = Vector2.Dot(this.position - vector, vector2 - vector) / (vector2 - vector).sqrMagnitude;
							float num3 = Mathf.Lerp(a, b, t);
							float sqrMagnitude = (Vector2.Lerp(vector, vector2, t) - this.position).sqrMagnitude;
							if (sqrMagnitude < num * num && this.elevationCoordinate <= num3 + obstacleVertex2.height && this.elevationCoordinate + this.height >= num3)
							{
								vos.Add(Agent.VO.SegmentObstacle(vector2 - this.position, vector - this.position, Vector2.zero, this.radius * 0.01f, 1f / this.ObstacleTimeHorizon, 1f / this.simulator.DeltaTime));
							}
						}
						obstacleVertex2 = obstacleVertex2.next;
					}
				}
				while (obstacleVertex2 != obstacleVertex && obstacleVertex2 != null && obstacleVertex2.next != null);
			}
		}

		private void GenerateNeighbourAgentVOs(Agent.VOBuffer vos)
		{
			float num = 1f / this.agentTimeHorizon;
			Vector2 a = this.currentVelocity;
			for (int i = 0; i < this.neighbours.Count; i++)
			{
				Agent agent = this.neighbours[i];
				if (agent != this)
				{
					float num2 = Math.Min(this.elevationCoordinate + this.height, agent.elevationCoordinate + agent.height);
					float num3 = Math.Max(this.elevationCoordinate, agent.elevationCoordinate);
					if (num2 - num3 >= 0f)
					{
						float num4 = this.radius + agent.radius;
						Vector2 vector = agent.position - this.position;
						float num5;
						if (agent.locked || agent.manuallyControlled)
						{
							num5 = 1f;
						}
						else if (agent.Priority > 1E-05f || this.Priority > 1E-05f)
						{
							num5 = agent.Priority / (this.Priority + agent.Priority);
						}
						else
						{
							num5 = 0.5f;
						}
						Vector2 b = Vector2.Lerp(agent.currentVelocity, agent.desiredVelocity, 2f * num5 - 1f);
						Vector2 vector2 = Vector2.Lerp(a, b, num5);
						vos.Add(new Agent.VO(vector, vector2, num4, num, 1f / this.simulator.DeltaTime));
						if (this.DebugDraw)
						{
							Agent.DrawVO(this.position + vector * num + vector2, num4 * num, this.position + vector2);
						}
					}
				}
			}
		}

		private Vector2 GradientDescent(Agent.VOBuffer vos, Vector2 sampleAround1, Vector2 sampleAround2)
		{
			float num;
			Vector2 vector = this.Trace(vos, sampleAround1, out num);
			if (this.DebugDraw)
			{
				Draw.Debug.CrossXZ(Agent.FromXZ(vector + this.position), Color.yellow, 0.5f);
			}
			float num2;
			Vector2 vector2 = this.Trace(vos, sampleAround2, out num2);
			if (this.DebugDraw)
			{
				Draw.Debug.CrossXZ(Agent.FromXZ(vector2 + this.position), Color.magenta, 0.5f);
			}
			return (num >= num2) ? vector2 : vector;
		}

		private static bool BiasDesiredVelocity(Agent.VOBuffer vos, ref Vector2 desiredVelocity, ref Vector2 targetPointInVelocitySpace, float maxBiasRadians)
		{
			float magnitude = desiredVelocity.magnitude;
			float num = 0f;
			for (int i = 0; i < vos.length; i++)
			{
				float b;
				vos.buffer[i].Gradient(desiredVelocity, out b);
				num = Mathf.Max(num, b);
			}
			bool result = num > 0f;
			if (magnitude < 0.001f)
			{
				return result;
			}
			float d = Mathf.Min(maxBiasRadians, num / magnitude);
			desiredVelocity += new Vector2(desiredVelocity.y, -desiredVelocity.x) * d;
			targetPointInVelocitySpace += new Vector2(targetPointInVelocitySpace.y, -targetPointInVelocitySpace.x) * d;
			return result;
		}

		private Vector2 EvaluateGradient(Agent.VOBuffer vos, Vector2 p, out float value)
		{
			Vector2 vector = Vector2.zero;
			value = 0f;
			for (int i = 0; i < vos.length; i++)
			{
				float num;
				Vector2 vector2 = vos.buffer[i].ScaledGradient(p, out num);
				if (num > value)
				{
					value = num;
					vector = vector2;
				}
			}
			Vector2 a = this.desiredVelocity - p;
			float magnitude = a.magnitude;
			if (magnitude > 0.0001f)
			{
				vector += a * (0.1f / magnitude);
				value += magnitude * 0.1f;
			}
			float sqrMagnitude = p.sqrMagnitude;
			if (sqrMagnitude > this.desiredSpeed * this.desiredSpeed)
			{
				float num2 = Mathf.Sqrt(sqrMagnitude);
				if (num2 > this.maxSpeed)
				{
					value += 3f * (num2 - this.maxSpeed);
					vector -= 3f * (p / num2);
				}
				float num3 = 0.2f;
				value += num3 * (num2 - this.desiredSpeed);
				vector -= num3 * (p / num2);
			}
			return vector;
		}

		private Vector2 Trace(Agent.VOBuffer vos, Vector2 p, out float score)
		{
			float num = Mathf.Max(this.radius, 0.2f * this.desiredSpeed);
			float num2 = float.PositiveInfinity;
			Vector2 result = p;
			for (int i = 0; i < 50; i++)
			{
				float num3 = 1f - (float)i / 50f;
				num3 = Agent.Sqr(num3) * num;
				float num4;
				Vector2 vector = this.EvaluateGradient(vos, p, out num4);
				if (num4 < num2)
				{
					num2 = num4;
					result = p;
				}
				vector.Normalize();
				vector *= num3;
				Vector2 a = p;
				p += vector;
				if (this.DebugDraw)
				{
					Debug.DrawLine(Agent.FromXZ(a + this.position), Agent.FromXZ(p + this.position), Agent.Rainbow((float)i * 0.1f) * new Color(1f, 1f, 1f, 1f));
				}
			}
			score = num2;
			return result;
		}

		internal float radius;

		internal float height;

		internal float desiredSpeed;

		internal float maxSpeed;

		internal float agentTimeHorizon;

		internal float obstacleTimeHorizon;

		internal bool locked;

		private RVOLayer layer;

		private RVOLayer collidesWith;

		private int maxNeighbours;

		internal Vector2 position;

		private float elevationCoordinate;

		private Vector2 currentVelocity;

		private Vector2 desiredTargetPointInVelocitySpace;

		private Vector2 desiredVelocity;

		private Vector2 nextTargetPoint;

		private float nextDesiredSpeed;

		private float nextMaxSpeed;

		private Vector2 collisionNormal;

		private bool manuallyControlled;

		private bool debugDraw;

		internal Agent next;

		private float calculatedSpeed;

		private Vector2 calculatedTargetPoint;

		internal Simulator simulator;

		private List<Agent> neighbours = new List<Agent>();

		private List<float> neighbourDists = new List<float>();

		private List<ObstacleVertex> obstaclesBuffered = new List<ObstacleVertex>();

		private List<ObstacleVertex> obstacles = new List<ObstacleVertex>();

		private const float DesiredVelocityWeight = 0.1f;

		private const float WallWeight = 5f;

		internal struct VO
		{
			public VO(Vector2 center, Vector2 offset, float radius, float inverseDt, float inverseDeltaTime)
			{
				this.weightFactor = 1f;
				this.weightBonus = 0f;
				this.circleCenter = center * inverseDt + offset;
				this.weightFactor = 4f * Mathf.Exp(-Agent.Sqr(center.sqrMagnitude / (radius * radius))) + 1f;
				if (center.magnitude < radius)
				{
					this.colliding = true;
					this.line1 = center.normalized * (center.magnitude - radius - 0.001f) * 0.3f * inverseDeltaTime;
					Vector2 vector = new Vector2(this.line1.y, -this.line1.x);
					this.dir1 = vector.normalized;
					this.line1 += offset;
					this.cutoffDir = Vector2.zero;
					this.cutoffLine = Vector2.zero;
					this.dir2 = Vector2.zero;
					this.line2 = Vector2.zero;
					this.radius = 0f;
				}
				else
				{
					this.colliding = false;
					center *= inverseDt;
					radius *= inverseDt;
					Vector2 b = center + offset;
					float d = center.magnitude - radius + 0.001f;
					this.cutoffLine = center.normalized * d;
					Vector2 vector2 = new Vector2(-this.cutoffLine.y, this.cutoffLine.x);
					this.cutoffDir = vector2.normalized;
					this.cutoffLine += offset;
					float num = Mathf.Atan2(-center.y, -center.x);
					float num2 = Mathf.Abs(Mathf.Acos(radius / center.magnitude));
					this.radius = radius;
					this.line1 = new Vector2(Mathf.Cos(num + num2), Mathf.Sin(num + num2));
					this.dir1 = new Vector2(this.line1.y, -this.line1.x);
					this.line2 = new Vector2(Mathf.Cos(num - num2), Mathf.Sin(num - num2));
					this.dir2 = new Vector2(this.line2.y, -this.line2.x);
					this.line1 = this.line1 * radius + b;
					this.line2 = this.line2 * radius + b;
				}
				this.segmentStart = Vector2.zero;
				this.segmentEnd = Vector2.zero;
				this.segment = false;
			}

			private static Vector2 ComplexMultiply(Vector2 a, Vector2 b)
			{
				return new Vector2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
			}

			public static Agent.VO SegmentObstacle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 offset, float radius, float inverseDt, float inverseDeltaTime)
			{
				Agent.VO result = default(Agent.VO);
				result.weightFactor = 1f;
				result.weightBonus = Mathf.Max(radius, 1f) * 40f;
				Vector3 vector = VectorMath.ClosestPointOnSegment(segmentStart, segmentEnd, Vector2.zero);
				if (vector.magnitude <= radius)
				{
					result.colliding = true;
					result.line1 = vector.normalized * (vector.magnitude - radius) * 0.3f * inverseDeltaTime;
					Vector2 vector2 = new Vector2(result.line1.y, -result.line1.x);
					result.dir1 = vector2.normalized;
					result.line1 += offset;
					result.cutoffDir = Vector2.zero;
					result.cutoffLine = Vector2.zero;
					result.dir2 = Vector2.zero;
					result.line2 = Vector2.zero;
					result.radius = 0f;
					result.segmentStart = Vector2.zero;
					result.segmentEnd = Vector2.zero;
					result.segment = false;
				}
				else
				{
					result.colliding = false;
					segmentStart *= inverseDt;
					segmentEnd *= inverseDt;
					radius *= inverseDt;
					Vector2 normalized = (segmentEnd - segmentStart).normalized;
					result.cutoffDir = normalized;
					result.cutoffLine = segmentStart + new Vector2(-normalized.y, normalized.x) * radius;
					result.cutoffLine += offset;
					float sqrMagnitude = segmentStart.sqrMagnitude;
					Vector2 a = -Agent.VO.ComplexMultiply(segmentStart, new Vector2(radius, Mathf.Sqrt(Mathf.Max(0f, sqrMagnitude - radius * radius)))) / sqrMagnitude;
					float sqrMagnitude2 = segmentEnd.sqrMagnitude;
					Vector2 a2 = -Agent.VO.ComplexMultiply(segmentEnd, new Vector2(radius, -Mathf.Sqrt(Mathf.Max(0f, sqrMagnitude2 - radius * radius)))) / sqrMagnitude2;
					result.line1 = segmentStart + a * radius + offset;
					result.line2 = segmentEnd + a2 * radius + offset;
					result.dir1 = new Vector2(a.y, -a.x);
					result.dir2 = new Vector2(a2.y, -a2.x);
					result.segmentStart = segmentStart;
					result.segmentEnd = segmentEnd;
					result.radius = radius;
					result.segment = true;
				}
				return result;
			}

			public static float SignedDistanceFromLine(Vector2 a, Vector2 dir, Vector2 p)
			{
				return (p.x - a.x) * dir.y - dir.x * (p.y - a.y);
			}

			public Vector2 ScaledGradient(Vector2 p, out float weight)
			{
				Vector2 vector = this.Gradient(p, out weight);
				if (weight > 0f)
				{
					vector *= 2f * this.weightFactor;
					weight *= 2f * this.weightFactor;
					weight += 1f + this.weightBonus;
				}
				return vector;
			}

			public Vector2 Gradient(Vector2 p, out float weight)
			{
				if (this.colliding)
				{
					float num = Agent.VO.SignedDistanceFromLine(this.line1, this.dir1, p);
					if (num >= 0f)
					{
						weight = num;
						return new Vector2(-this.dir1.y, this.dir1.x);
					}
					weight = 0f;
					return new Vector2(0f, 0f);
				}
				else
				{
					float num2 = Agent.VO.SignedDistanceFromLine(this.cutoffLine, this.cutoffDir, p);
					if (num2 <= 0f)
					{
						weight = 0f;
						return Vector2.zero;
					}
					float num3 = Agent.VO.SignedDistanceFromLine(this.line1, this.dir1, p);
					float num4 = Agent.VO.SignedDistanceFromLine(this.line2, this.dir2, p);
					if (num3 < 0f || num4 < 0f)
					{
						weight = 0f;
						return Vector2.zero;
					}
					Vector2 result;
					if (Vector2.Dot(p - this.line1, this.dir1) > 0f && Vector2.Dot(p - this.line2, this.dir2) < 0f)
					{
						if (!this.segment)
						{
							Vector2 v = p - this.circleCenter;
							float num5;
							result = VectorMath.Normalize(v, out num5);
							weight = this.radius - num5;
							return result;
						}
						if (num2 < this.radius)
						{
							Vector2 b = VectorMath.ClosestPointOnSegment(this.segmentStart, this.segmentEnd, p);
							Vector2 v2 = p - b;
							float num6;
							result = VectorMath.Normalize(v2, out num6);
							weight = this.radius - num6;
							return result;
						}
					}
					if (this.segment && num2 < num3 && num2 < num4)
					{
						weight = num2;
						result = new Vector2(-this.cutoffDir.y, this.cutoffDir.x);
						return result;
					}
					if (num3 < num4)
					{
						weight = num3;
						result = new Vector2(-this.dir1.y, this.dir1.x);
					}
					else
					{
						weight = num4;
						result = new Vector2(-this.dir2.y, this.dir2.x);
					}
					return result;
				}
			}

			private Vector2 line1;

			private Vector2 line2;

			private Vector2 dir1;

			private Vector2 dir2;

			private Vector2 cutoffLine;

			private Vector2 cutoffDir;

			private Vector2 circleCenter;

			private bool colliding;

			private float radius;

			private float weightFactor;

			private float weightBonus;

			private Vector2 segmentStart;

			private Vector2 segmentEnd;

			private bool segment;
		}

		internal class VOBuffer
		{
			public VOBuffer(int n)
			{
				this.buffer = new Agent.VO[n];
				this.length = 0;
			}

			public void Clear()
			{
				this.length = 0;
			}

			public void Add(Agent.VO vo)
			{
				if (this.length >= this.buffer.Length)
				{
					Agent.VO[] array = new Agent.VO[this.buffer.Length * 2];
					this.buffer.CopyTo(array, 0);
					this.buffer = array;
				}
				this.buffer[this.length++] = vo;
			}

			public Agent.VO[] buffer;

			public int length;
		}
	}
}
