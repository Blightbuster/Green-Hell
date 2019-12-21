using System;
using UnityEngine;

public static class ComponentExtention
{
	public static bool CanCopy(this Component component)
	{
		return Array.IndexOf<Type>(ComponentExtention.s_DisallowCopyTypes, component.GetType()) == -1;
	}

	public static void DrawLineGame(this Component owner, int id, Vector3 start, Vector3 end, Color? color = null, float duration = 0f)
	{
	}

	public static void DrawArrowGame(this Component owner, int id, Vector3 start, Vector3 end, Color? color = null, float duration = 0f)
	{
	}

	public static void DrawCircleGame(this Component owner, int id, Vector3 center, float radius, Vector3 forward, Color? color = null, float duration = 0f)
	{
	}

	public static T AddComponentWithEvent<T>(this Component component) where T : Component
	{
		return component.gameObject.AddComponentWithEvent<T>();
	}

	public static void DestroyComponentWithEvent<T>(this Component component, T component_to_remove) where T : Component
	{
		component.gameObject.DestroyComponentWithEvent(component_to_remove);
	}

	private static readonly Type[] s_DisallowCopyTypes = new Type[]
	{
		typeof(GuidComponent),
		typeof(ReplicationComponent)
	};
}
