using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ColliderStreamer : MonoBehaviour
{
	private void Start()
	{
		this.colliderStreamerManager = GameObject.FindGameObjectWithTag(ColliderStreamerManager.COLLIDERSTREAMERMANAGERTAG).GetComponent<ColliderStreamerManager>();
		this.colliderStreamerManager.AddColliderStreamer(this);
		GameObject gameObject = GameObject.FindGameObjectWithTag(WorldMover.WORLDMOVERTAG);
		if (gameObject)
		{
			gameObject.GetComponent<WorldMover>().AddObjectToMove(base.transform);
		}
	}

	public void SetSceneGameObject(GameObject sceneGameObject)
	{
		this.sceneGameObject = sceneGameObject;
		this.sceneGameObject.transform.position = base.transform.position;
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((!this.playerOnlyActivate || other.transform == this.colliderStreamerManager.player) && !this.loaded)
		{
			this.loaded = true;
			SceneManager.LoadSceneAsync(this.sceneName, LoadSceneMode.Additive);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((!this.playerOnlyActivate || other.transform == this.colliderStreamerManager.player) && this.sceneGameObject)
		{
			this.loaded = false;
			base.Invoke("UnloadScene", this.unloadTimer);
		}
	}

	private void UnloadScene()
	{
		UnityEngine.Object.Destroy(this.sceneGameObject);
	}

	[Tooltip("Scene name that belongs to this collider.")]
	public string sceneName;

	[Tooltip("Path where collider streamer should find scene which has to loaded after collider hit.")]
	public string scenePath;

	[HideInInspector]
	public GameObject sceneGameObject;

	[HideInInspector]
	public ColliderStreamerManager colliderStreamerManager;

	[Tooltip("If it's checkboxed only player could activate collider to start loading, otherwise every physical hit could activate it.")]
	public bool playerOnlyActivate = true;

	[Tooltip("Time in seconds after which scene will be unloaded when \"Player\" or object that activate loading will left collider area.")]
	public float unloadTimer;

	private bool loaded;
}
