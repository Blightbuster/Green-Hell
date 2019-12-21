using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cinemachine.Utility
{
	public class CinemachineGameWindowDebug
	{
		public static void ReleaseScreenPos(UnityEngine.Object client)
		{
			if (CinemachineGameWindowDebug.mClients != null && CinemachineGameWindowDebug.mClients.Contains(client))
			{
				CinemachineGameWindowDebug.mClients.Remove(client);
			}
		}

		public static Rect GetScreenPos(UnityEngine.Object client, string text, GUIStyle style)
		{
			if (CinemachineGameWindowDebug.mClients == null)
			{
				CinemachineGameWindowDebug.mClients = new HashSet<UnityEngine.Object>();
			}
			if (!CinemachineGameWindowDebug.mClients.Contains(client))
			{
				CinemachineGameWindowDebug.mClients.Add(client);
			}
			Vector2 position = new Vector2(0f, 0f);
			Vector2 vector = style.CalcSize(new GUIContent(text));
			if (CinemachineGameWindowDebug.mClients != null)
			{
				using (HashSet<UnityEngine.Object>.Enumerator enumerator = CinemachineGameWindowDebug.mClients.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == client)
						{
							break;
						}
						position.y += vector.y;
					}
				}
			}
			return new Rect(position, vector);
		}

		private static HashSet<UnityEngine.Object> mClients;
	}
}
