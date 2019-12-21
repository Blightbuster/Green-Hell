using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class DebugRender : MonoBehaviour
{
	public static void DrawBox(Bounds bounds, Color color)
	{
	}

	public static void DrawBounds(Bounds bounds, Quaternion rotation, Color color, float duration = 0f)
	{
	}

	public static void DrawBounds(BoxCollider collider, Color color, float duration = 0f)
	{
	}

	public static void DrawBox(AABB box, Transform transform, Color color)
	{
	}

	public static void DrawBoxGizmos(Vector3 center, Vector3 size, Quaternion q)
	{
	}

	public static void DrawBox(OBB box, Color color, float duration = 0f)
	{
	}

	public static void DrawTriangle(Triangle triangle, Color color, float duration = 0f)
	{
	}

	public static void DrawLine(Vector3 start, Vector3 end, Color color = default(Color), float duration = 0f)
	{
	}

	public static void DrawPoint(Vector3 start, Color color, float size = 0.3f, float duration = 0f)
	{
	}

	public static void DrawTransform(Transform trans, float duration, bool depth_test = false)
	{
	}

	public static void DrawCircle(Vector3 position, float radius, Color color)
	{
	}

	private static DebugRender.DebugShape SetupBasicShape(Type type, Component owner, int id, Color? color = null, float duration = 0f, bool ztest_enabled = false)
	{
		return null;
	}

	public static void DrawLineGame(Vector3 start, Vector3 end, Color? color = null, float duration = 0f, bool ztest_enabled = false)
	{
	}

	public static void DrawArrowGame(Vector3 start, Vector3 end, Color? color = null, float duration = 0f, bool ztest_enabled = false)
	{
	}

	public static void DrawCircleGame(Vector3 center, float radius, Vector3 forward, Color? color = null, float duration = 0f, bool ztest_enabled = false)
	{
	}

	public static void DrawLineGame(Component owner, int id, Vector3 start, Vector3 end, Color? color = null, float duration = 0f, bool ztest_enabled = false)
	{
	}

	public static void DrawArrowGame(Component owner, int id, Vector3 start, Vector3 end, Color? color = null, float duration = 0f, bool ztest_enabled = false)
	{
	}

	public static void DrawCircleGame(Component owner, int id, Vector3 center, float radius, Vector3 forward, Color? color = null, float duration = 0f, bool ztest_enabled = false)
	{
	}

	private Dictionary<Component, List<DebugRender.DebugShape>> m_ObjectDebugShapes = new Dictionary<Component, List<DebugRender.DebugShape>>();

	private List<DebugRender.DebugShape> m_DebugShapes = new List<DebugRender.DebugShape>();

	private abstract class DebugShape
	{
		public bool IsExpired()
		{
			if (this.duration > 0f)
			{
				this.duration -= Time.deltaTime;
			}
			return this.duration <= 0f;
		}

		public virtual void PreDraw()
		{
		}

		public abstract void Draw();

		public virtual void PostDraw()
		{
		}

		public bool ztest_enabled = true;

		public int id = -1;

		public float duration;

		public Color color;
	}
}
