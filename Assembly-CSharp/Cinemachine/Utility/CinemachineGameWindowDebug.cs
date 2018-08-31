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
			Vector2 size = style.CalcSize(new GUIContent(text));
			if (CinemachineGameWindowDebug.mClients != null)
			{
				foreach (UnityEngine.Object x in CinemachineGameWindowDebug.mClients)
				{
					if (x == client)
					{
						break;
					}
					position.y += size.y;
				}
			}
			return new Rect(position, size);
		}

		private static HashSet<UnityEngine.Object> mClients;
	}
}
