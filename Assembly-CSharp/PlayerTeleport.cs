using System;
using UnityEngine;

public class PlayerTeleport : MonoBehaviour
{
	public void Teleport(bool showLoadingScreen)
	{
		if (this.player != null)
		{
			this.player.position = base.transform.position + ((!(this.worldMover == null)) ? this.worldMover.currentMove : Vector3.zero);
			this.player.rotation = base.transform.rotation;
			foreach (Streamer streamer in this.streamers)
			{
				streamer.showLoadingScreen = showLoadingScreen;
				streamer.CheckPositionTiles();
			}
			if (this.uiLoadingStreamer != null)
			{
				this.uiLoadingStreamer.Show();
			}
			if (this.playerMover != null)
			{
				this.playerMover.MovePlayer();
			}
		}
		else if (this.streamers[0] != null && this.streamers[0].player != null)
		{
			this.player = this.streamers[0].player;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.5f);
		Gizmos.DrawSphere(base.transform.position, 1f);
	}

	[Tooltip("If teleport/respawn should initiate loading screen, drag and drop here your \"Loading Screen UI\" object  from your scene hierarchy or an object that contain \"UI Loading Streamer\" script.")]
	public UILoadingStreamer uiLoadingStreamer;

	[Tooltip("If teleport/respawn should initiate player move to safe position.")]
	public PlayerMover playerMover;

	[Tooltip("List of streamers. Drag and drop here all your streamer objects from scene hierarchy.")]
	public Streamer[] streamers;

	[Tooltip("Object that should be moved during respawn/teleport process. It must be the same as object that streamer fallows during streaming process.")]
	public Transform player;

	[Tooltip("If you use Floating Point fix system drag and drop world mover prefab from your scene hierarchy.")]
	public WorldMover worldMover;
}
