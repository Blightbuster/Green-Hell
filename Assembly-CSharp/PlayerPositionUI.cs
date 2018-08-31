using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPositionUI : MonoBehaviour
{
	public void Start()
	{
		if (this.player == null)
		{
			Debug.LogError("Player is not connected to Position Gizmo");
		}
		this.text.text = "Player position: Player is not connected to Position Gizmo";
	}

	public void Update()
	{
		if (this.player != null)
		{
			if (this.worldMover != null)
			{
				this.text.text = string.Concat(new object[]
				{
					"Player position: ",
					this.player.transform.position,
					"\nPlayer real position: ",
					this.worldMover.playerPositionMovedLooped
				});
			}
			else
			{
				this.text.text = "Player position: " + this.player.transform.position + "\nPlayer real position: Not Connected to World Mover";
			}
		}
	}

	[Tooltip("Object that should be moved during respawn/teleport process. It must be the same as object that streamer fallows during streaming process.")]
	public Transform player;

	[Tooltip("If you use Floating Point fix system drag and drop world mover prefab from your scene hierarchy.")]
	public WorldMover worldMover;

	public Text text;
}
