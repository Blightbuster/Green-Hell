using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[HelpURL("http://indago.homenko.pl/world-streamer/")]
public class Streamer : MonoBehaviour
{
	public float LoadingProgress
	{
		get
		{
			return (this.tilesToLoad <= 0) ? 1f : ((float)this.tilesLoaded / (float)this.tilesToLoad);
		}
	}

	private void Awake()
	{
		int qualityLevel = QualitySettings.GetQualityLevel();
		if (base.gameObject.name == "_Streamer_Major_medium")
		{
			switch (qualityLevel)
			{
			case 0:
				this.loadingRange.Set(1f, 0f, 1f);
				this.deloadingRange.Set(1f, 0f, 1f);
				break;
			case 1:
				this.loadingRange.Set(1f, 0f, 1f);
				this.deloadingRange.Set(1f, 0f, 1f);
				break;
			case 2:
				this.loadingRange.Set(2f, 0f, 2f);
				this.deloadingRange.Set(2f, 0f, 2f);
				break;
			case 3:
				this.loadingRange.Set(2f, 0f, 2f);
				this.deloadingRange.Set(2f, 0f, 2f);
				break;
			case 4:
				this.loadingRange.Set(3f, 0f, 3f);
				this.deloadingRange.Set(3f, 0f, 3f);
				break;
			}
		}
		else if (base.gameObject.name == "_Streamer_Minor_big")
		{
			switch (qualityLevel)
			{
			case 0:
				this.loadingRange.Set(1f, 0f, 1f);
				this.deloadingRange.Set(1f, 0f, 1f);
				break;
			case 1:
				this.loadingRange.Set(2f, 0f, 2f);
				this.deloadingRange.Set(2f, 0f, 2f);
				break;
			case 2:
				this.loadingRange.Set(3f, 0f, 3f);
				this.deloadingRange.Set(3f, 0f, 3f);
				break;
			case 3:
				this.loadingRange.Set(3f, 0f, 3f);
				this.deloadingRange.Set(3f, 0f, 3f);
				break;
			case 4:
				this.loadingRange.Set(4f, 0f, 4f);
				this.deloadingRange.Set(4f, 0f, 4f);
				break;
			}
		}
		else if (base.gameObject.name == "_Streamer_Minor_small")
		{
			switch (qualityLevel)
			{
			case 0:
				this.loadingRange.Set(1f, 0f, 1f);
				this.deloadingRange.Set(1f, 0f, 1f);
				break;
			case 1:
				this.loadingRange.Set(1f, 0f, 1f);
				this.deloadingRange.Set(1f, 0f, 1f);
				break;
			case 2:
				this.loadingRange.Set(1f, 0f, 1f);
				this.deloadingRange.Set(1f, 0f, 1f);
				break;
			case 3:
				this.loadingRange.Set(1f, 0f, 1f);
				this.deloadingRange.Set(1f, 0f, 1f);
				break;
			case 4:
				this.loadingRange.Set(1f, 0f, 1f);
				this.deloadingRange.Set(1f, 0f, 1f);
				break;
			}
		}
		else if (base.gameObject.name == "_Streamer_Minor_horizon")
		{
			switch (qualityLevel)
			{
			case 0:
				this.loadingRange.Set(1f, 0f, 1f);
				this.deloadingRange.Set(1f, 0f, 1f);
				break;
			case 1:
				this.loadingRange.Set(2f, 0f, 2f);
				this.deloadingRange.Set(2f, 0f, 2f);
				break;
			case 2:
				this.loadingRange.Set(4f, 0f, 4f);
				this.deloadingRange.Set(4f, 0f, 4f);
				break;
			case 3:
				this.loadingRange.Set(6f, 0f, 6f);
				this.deloadingRange.Set(6f, 0f, 6f);
				break;
			case 4:
				this.loadingRange.Set(6f, 0f, 6f);
				this.deloadingRange.Set(6f, 0f, 6f);
				break;
			}
		}
		this.ParseDebugScript();
		if (this.spawnedPlayer)
		{
			this.player = null;
		}
		this.xPos = int.MinValue;
		this.yPos = int.MinValue;
		this.zPos = int.MinValue;
	}

	private void ParseDebugScript()
	{
		if (!File.Exists(Application.dataPath + "/Resources/scripts/Debug/" + base.name))
		{
			return;
		}
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("/Resources/scripts/Debug/" + base.name, false);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "LoadingRange")
			{
				this.loadingRange.Set(key.GetVariable(0).FValue, key.GetVariable(1).FValue, key.GetVariable(2).FValue);
			}
			else if (key.GetName() == "LoadingRangeMin")
			{
				this.loadingRangeMin.Set(key.GetVariable(0).FValue, key.GetVariable(1).FValue, key.GetVariable(2).FValue);
			}
			else if (key.GetName() == "DeloadingRange")
			{
				this.deloadingRange.Set(key.GetVariable(0).FValue, key.GetVariable(1).FValue, key.GetVariable(2).FValue);
			}
		}
	}

	private void Start()
	{
		MainLevel.Instance.RegisterStreamer(this);
		if (this.sceneCollection != null)
		{
			this.PrepareScenesArray();
			this.xLimity = this.sceneCollection.xLimitsy;
			this.xLimitx = this.sceneCollection.xLimitsx;
			this.xRange = this.xLimity + Mathf.Abs(this.xLimitx) + 1;
			this.yLimity = this.sceneCollection.yLimitsy;
			this.yLimitx = this.sceneCollection.yLimitsx;
			this.yRange = this.yLimity + Mathf.Abs(this.yLimitx) + 1;
			this.zLimity = this.sceneCollection.zLimitsy;
			this.zLimitx = this.sceneCollection.zLimitsx;
			this.zRange = this.zLimity + Mathf.Abs(this.zLimitx) + 1;
			base.StartCoroutine(this.PositionChecker());
			Streamer.canUnload = true;
		}
		else
		{
			Debug.LogError("No scene collection in streamer");
		}
	}

	private int mod(int x, int m)
	{
		return (x % m + m) % m;
	}

	public void AddSceneGO(string sceneName, GameObject sceneGO)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		Streamer.SceneNameToPos(this.sceneCollection, sceneName, out num, out num2, out num3);
		int[] key = new int[]
		{
			num,
			num2,
			num3
		};
		if (this.scenesArray.ContainsKey(key))
		{
			this.scenesArray[key].sceneGo = sceneGO;
			sceneGO.transform.position += this.currentMove + new Vector3(this.scenesArray[key].posXLimitMove, this.scenesArray[key].posYLimitMove, this.scenesArray[key].posZLimitMove);
		}
		this.tilesLoaded++;
		this.currentlySceneLoading--;
		if (this.terrainNeighbours)
		{
			this.terrainNeighbours.CreateNeighbours();
		}
	}

	private void Update()
	{
		this.LoadLevelAsyncManage();
	}

	private void LoadLevelAsyncManage()
	{
		if (!this.CanLoad())
		{
			return;
		}
		if (this.scenesToLoad.Count > 0 && this.currentlySceneLoading <= 0)
		{
			if (this.LoadingProgress < 1f || (this.sceneLoadFramesNextWaited && this.sceneLoadFrameNext <= 0))
			{
				this.sceneLoadFramesNextWaited = false;
				this.sceneLoadFrameNext = this.sceneLoadWaitFrames;
				while (this.currentlySceneLoading < this.maxParallelSceneLoading && this.scenesToLoad.Count > 0)
				{
					SceneSplit sceneSplit = this.scenesToLoad[0];
					this.scenesToLoad.Remove(sceneSplit);
					this.currentlySceneLoading++;
					MainLevel.Instance.m_SceneAsyncOperation.Add(SceneManager.LoadSceneAsync(sceneSplit.sceneName, LoadSceneMode.Additive));
				}
			}
			else
			{
				this.sceneLoadFramesNextWaited = true;
				this.sceneLoadFrameNext--;
			}
		}
	}

	private IEnumerator PositionChecker()
	{
		for (;;)
		{
			if (this.spawnedPlayer && this.player == null && !string.IsNullOrEmpty(this.playerTag))
			{
				GameObject gameObject = GameObject.FindGameObjectWithTag(this.playerTag);
				if (gameObject != null)
				{
					this.player = gameObject.transform;
				}
			}
			if (this.streamerActive && this.player != null)
			{
				this.CheckPositionTiles();
			}
			else if (this.loadedScenes.Count > 0)
			{
				this.UnloadAllScenes();
				this.xPos = int.MinValue;
				this.yPos = int.MinValue;
				this.zPos = int.MinValue;
			}
			yield return new WaitForSeconds(this.positionCheckTime);
		}
		yield break;
	}

	public void CheckPositionTiles()
	{
		Vector3 a = this.player.position;
		a -= this.currentMove;
		int num = (this.sceneCollection.xSize == 0) ? 0 : Mathf.FloorToInt(a.x / (float)this.sceneCollection.xSize);
		int num2 = (this.sceneCollection.ySize == 0) ? 0 : Mathf.FloorToInt(a.y / (float)this.sceneCollection.ySize);
		int num3 = (this.sceneCollection.zSize == 0) ? 0 : Mathf.FloorToInt(a.z / (float)this.sceneCollection.zSize);
		if (num != this.xPos || num2 != this.yPos || num3 != this.zPos)
		{
			this.xPos = num;
			this.yPos = num2;
			this.zPos = num3;
			this.SceneLoading();
			base.Invoke("SceneUnloading", this.destroyTileDelay);
			if (this.worldMover != null)
			{
				this.worldMover.CheckMoverDistance(num, num2, num3);
			}
		}
	}

	private void SceneLoading()
	{
		if (this.showLoadingScreen && this.loadingStreamer != null)
		{
			this.showLoadingScreen = false;
			if (this.tilesLoaded >= this.tilesToLoad)
			{
				this.tilesToLoad = int.MaxValue;
				this.tilesLoaded = 0;
			}
		}
		int num = 0;
		if (!this.useLoadingRangeMin)
		{
			int num2 = this.xPos;
			int num3 = this.yPos;
			int num4 = this.zPos;
			int[] array = new int[]
			{
				num2,
				num3,
				num4
			};
			float posXLimitMove = 0f;
			int num5 = 0;
			float posYLimitMove = 0f;
			int num6 = 0;
			float posZLimitMove = 0f;
			int num7 = 0;
			if (this.looping)
			{
				if (this.sceneCollection.xSplitIs)
				{
					int num8 = this.mod(num2 + Mathf.Abs(this.xLimitx), this.xRange) + this.xLimitx;
					num5 = (int)Math.Ceiling((double)((float)(num2 - this.xLimity) / (float)this.xRange)) * this.xRange;
					posXLimitMove = (float)(num5 * this.sceneCollection.xSize);
					array[0] = num8;
				}
				if (this.sceneCollection.ySplitIs)
				{
					int num9 = this.mod(num3 + Mathf.Abs(this.yLimitx), this.yRange) + this.yLimitx;
					num6 = (int)Math.Ceiling((double)((float)(num3 - this.yLimity) / (float)this.yRange)) * this.yRange;
					posYLimitMove = (float)(num6 * this.sceneCollection.ySize);
					array[1] = num9;
				}
				if (this.sceneCollection.zSplitIs)
				{
					int num10 = this.mod(num4 + Mathf.Abs(this.zLimitx), this.zRange) + this.zLimitx;
					num7 = (int)Math.Ceiling((double)((float)(num4 - this.zLimity) / (float)this.zRange)) * this.zRange;
					posZLimitMove = (float)(num7 * this.sceneCollection.zSize);
					array[2] = num10;
				}
			}
			if (this.scenesArray.ContainsKey(array))
			{
				SceneSplit sceneSplit = this.scenesArray[array];
				if (!sceneSplit.loaded)
				{
					sceneSplit.loaded = true;
					sceneSplit.posXLimitMove = posXLimitMove;
					sceneSplit.xDeloadLimit = num5;
					sceneSplit.posYLimitMove = posYLimitMove;
					sceneSplit.yDeloadLimit = num6;
					sceneSplit.posZLimitMove = posZLimitMove;
					sceneSplit.zDeloadLimit = num7;
					this.scenesToLoad.Add(sceneSplit);
					this.loadedScenes.Add(sceneSplit);
					num++;
				}
			}
		}
		for (int i = -(int)this.loadingRange.x + this.xPos; i <= (int)this.loadingRange.x + this.xPos; i++)
		{
			for (int j = -(int)this.loadingRange.y + this.yPos; j <= (int)this.loadingRange.y + this.yPos; j++)
			{
				for (int k = -(int)this.loadingRange.z + this.zPos; k <= (int)this.loadingRange.z + this.zPos; k++)
				{
					if (!this.useLoadingRangeMin || (float)(i - this.xPos) < -this.loadingRangeMin.x || (float)(i - this.xPos) > this.loadingRangeMin.x || (float)(j - this.yPos) < -this.loadingRangeMin.y || (float)(j - this.yPos) > this.loadingRangeMin.y || (float)(k - this.zPos) < -this.loadingRangeMin.z || (float)(k - this.zPos) > this.loadingRangeMin.z)
					{
						int[] array2 = new int[]
						{
							i,
							j,
							k
						};
						float posXLimitMove2 = 0f;
						int num11 = 0;
						float posYLimitMove2 = 0f;
						int num12 = 0;
						float posZLimitMove2 = 0f;
						int num13 = 0;
						if (this.looping)
						{
							if (this.sceneCollection.xSplitIs)
							{
								int num14 = this.mod(i + Mathf.Abs(this.xLimitx), this.xRange) + this.xLimitx;
								num11 = (int)Math.Ceiling((double)((float)(i - this.xLimity) / (float)this.xRange)) * this.xRange;
								posXLimitMove2 = (float)(num11 * this.sceneCollection.xSize);
								array2[0] = num14;
							}
							if (this.sceneCollection.ySplitIs)
							{
								int num15 = this.mod(j + Mathf.Abs(this.yLimitx), this.yRange) + this.yLimitx;
								num12 = (int)Math.Ceiling((double)((float)(j - this.yLimity) / (float)this.yRange)) * this.yRange;
								posYLimitMove2 = (float)(num12 * this.sceneCollection.ySize);
								array2[1] = num15;
							}
							if (this.sceneCollection.zSplitIs)
							{
								int num16 = this.mod(k + Mathf.Abs(this.zLimitx), this.zRange) + this.zLimitx;
								num13 = (int)Math.Ceiling((double)((float)(k - this.zLimity) / (float)this.zRange)) * this.zRange;
								posZLimitMove2 = (float)(num13 * this.sceneCollection.zSize);
								array2[2] = num16;
							}
						}
						if (this.scenesArray.ContainsKey(array2))
						{
							SceneSplit sceneSplit2 = this.scenesArray[array2];
							if (!sceneSplit2.loaded)
							{
								sceneSplit2.loaded = true;
								sceneSplit2.posXLimitMove = posXLimitMove2;
								sceneSplit2.xDeloadLimit = num11;
								sceneSplit2.posYLimitMove = posYLimitMove2;
								sceneSplit2.yDeloadLimit = num12;
								sceneSplit2.posZLimitMove = posZLimitMove2;
								sceneSplit2.zDeloadLimit = num13;
								this.scenesToLoad.Add(sceneSplit2);
								this.loadedScenes.Add(sceneSplit2);
								num++;
							}
						}
					}
				}
			}
		}
		this.tilesToLoad = num;
		this.initialized = true;
	}

	private void SceneUnloading()
	{
		if (!this.CanLoad())
		{
			return;
		}
		List<SceneSplit> list = new List<SceneSplit>();
		foreach (SceneSplit sceneSplit in this.loadedScenes)
		{
			if ((Mathf.Abs(sceneSplit.posX + sceneSplit.xDeloadLimit - this.xPos) > (int)this.deloadingRange.x || Mathf.Abs(sceneSplit.posY + sceneSplit.yDeloadLimit - this.yPos) > (int)this.deloadingRange.y || Mathf.Abs(sceneSplit.posZ + sceneSplit.zDeloadLimit - this.zPos) > (int)this.deloadingRange.x) && sceneSplit.sceneGo != null)
			{
				list.Add(sceneSplit);
			}
			if (this.useLoadingRangeMin && (float)Mathf.Abs(sceneSplit.posX + sceneSplit.xDeloadLimit - this.xPos) <= this.loadingRangeMin.x && (float)Mathf.Abs(sceneSplit.posY + sceneSplit.yDeloadLimit - this.yPos) <= this.loadingRangeMin.y && (float)Mathf.Abs(sceneSplit.posZ + sceneSplit.zDeloadLimit - this.zPos) <= this.loadingRangeMin.z && sceneSplit.sceneGo != null)
			{
				list.Add(sceneSplit);
			}
		}
		foreach (SceneSplit sceneSplit2 in list)
		{
			this.loadedScenes.Remove(sceneSplit2);
			if (sceneSplit2.sceneGo != null)
			{
				Terrain componentInChildren = sceneSplit2.sceneGo.GetComponentInChildren<Terrain>();
				if (componentInChildren)
				{
					GameObject gameObject = componentInChildren.gameObject;
					UnityEngine.Object.Destroy(componentInChildren);
					UnityEngine.Object.Destroy(gameObject);
				}
			}
			try
			{
				MainLevel.Instance.m_SceneAsyncOperation.Add(SceneManager.UnloadSceneAsync(sceneSplit2.sceneGo.scene.name));
			}
			catch (Exception ex)
			{
				Debug.Log(sceneSplit2.sceneName);
				Debug.Log(sceneSplit2.sceneGo.name);
				Debug.Log(sceneSplit2.sceneGo.scene.name);
				Debug.LogError(ex.Message);
			}
			sceneSplit2.sceneGo = null;
			sceneSplit2.loaded = false;
		}
		list.Clear();
		if (this.terrainNeighbours)
		{
			this.terrainNeighbours.CreateNeighbours();
		}
		Streamer.UnloadAssets(this);
	}

	public void UnloadAllScenes()
	{
		foreach (KeyValuePair<int[], SceneSplit> keyValuePair in this.scenesArray)
		{
			if (keyValuePair.Value.sceneGo != null)
			{
				Terrain componentInChildren = keyValuePair.Value.sceneGo.GetComponentInChildren<Terrain>();
				if (componentInChildren)
				{
					GameObject gameObject = componentInChildren.gameObject;
					UnityEngine.Object.Destroy(componentInChildren);
					UnityEngine.Object.Destroy(gameObject);
				}
				try
				{
					SceneManager.UnloadSceneAsync(keyValuePair.Value.sceneGo.scene.name);
				}
				catch (Exception ex)
				{
					Debug.Log(keyValuePair.Value.sceneName);
					Debug.Log(keyValuePair.Value.sceneGo.name);
					Debug.Log(keyValuePair.Value.sceneGo.scene.name);
					Debug.LogError(ex.Message);
				}
			}
			keyValuePair.Value.loaded = false;
			keyValuePair.Value.sceneGo = null;
		}
		this.loadedScenes.Clear();
		if (this.terrainNeighbours)
		{
			this.terrainNeighbours.CreateNeighbours();
		}
		Streamer.UnloadAssets(this);
	}

	public static void UnloadAssets(Streamer streamer)
	{
		if (Streamer.canUnload)
		{
			Streamer.canUnload = false;
			streamer.StartCoroutine(streamer.UnloadAssetsWait());
		}
		else
		{
			Streamer.unloadNext = true;
		}
	}

	public IEnumerator UnloadAssetsWait()
	{
		do
		{
			float time_to_next = Streamer.waitTillNextUnload;
			if (this.CanUnload() && (MainLevel.Instance.m_UnusedAssetsAsyncOperation == null || MainLevel.Instance.m_UnusedAssetsAsyncOperation.isDone))
			{
				Streamer.unloadNext = false;
				MainLevel.Instance.m_UnusedAssetsAsyncOperation = Resources.UnloadUnusedAssets();
			}
			else
			{
				time_to_next = 1f;
			}
			yield return new WaitForSeconds(time_to_next);
		}
		while (Streamer.unloadNext);
		Streamer.canUnload = true;
		yield break;
	}

	private void PrepareScenesArray()
	{
		this.scenesArray = new Dictionary<int[], SceneSplit>(new IntArrayComparer());
		foreach (string text in this.sceneCollection.names)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			Streamer.SceneNameToPos(this.sceneCollection, text, out num, out num2, out num3);
			SceneSplit sceneSplit = new SceneSplit();
			sceneSplit.posX = num;
			sceneSplit.posY = num2;
			sceneSplit.posZ = num3;
			sceneSplit.sceneName = text.Replace(".unity", string.Empty);
			this.scenesArray.Add(new int[]
			{
				num,
				num2,
				num3
			}, sceneSplit);
		}
	}

	public static void SceneNameToPos(SceneCollection sceneCollection, string sceneName, out int posX, out int posY, out int posZ)
	{
		posX = 0;
		posY = 0;
		posZ = 0;
		string[] array = sceneName.Replace(sceneCollection.prefixScene, string.Empty).Replace(".unity", string.Empty).Split(new char[]
		{
			'_'
		}, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			if (text[0] == 'x')
			{
				posX = int.Parse(text.Replace("x", string.Empty));
			}
			if (text[0] == 'y')
			{
				posY = int.Parse(text.Replace("y", string.Empty));
			}
			if (text[0] == 'z')
			{
				posZ = int.Parse(text.Replace("z", string.Empty));
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (this.sceneCollection)
		{
			Gizmos.color = this.sceneCollection.color;
			Vector3 vector = new Vector3((float)((this.sceneCollection.xSize != 0) ? this.sceneCollection.xSize : 2), (float)((this.sceneCollection.ySize != 0) ? this.sceneCollection.ySize : 2), (float)((this.sceneCollection.zSize != 0) ? this.sceneCollection.zSize : 2));
			for (int i = -(int)this.loadingRange.x + this.xPos; i <= (int)this.loadingRange.x + this.xPos; i++)
			{
				for (int j = -(int)this.loadingRange.y + this.yPos; j <= (int)this.loadingRange.y + this.yPos; j++)
				{
					for (int k = -(int)this.loadingRange.z + this.zPos; k <= (int)this.loadingRange.z + this.zPos; k++)
					{
						if (!this.useLoadingRangeMin || (float)(i - this.xPos) < -this.loadingRangeMin.x || (float)(i - this.xPos) > this.loadingRangeMin.x || (float)(j - this.yPos) < -this.loadingRangeMin.y || (float)(j - this.yPos) > this.loadingRangeMin.y || (float)(k - this.zPos) < -this.loadingRangeMin.z || (float)(k - this.zPos) > this.loadingRangeMin.z)
						{
							Gizmos.DrawWireCube(new Vector3((float)i * vector.x, (float)j * vector.y, (float)k * vector.z) + vector * 0.5f + this.currentMove, vector);
						}
					}
				}
			}
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(new Vector3((float)this.xPos * vector.x, (float)this.yPos * vector.y, (float)this.zPos * vector.z) + vector * 0.5f + this.currentMove, vector);
		}
	}

	private void OnDestroy()
	{
		MainLevel.Instance.UnRegisterStreamer(this);
	}

	public bool IsSomethingToLoad()
	{
		return this.scenesToLoad.Count > 0 || this.currentlySceneLoading > 0;
	}

	private bool CanUnload()
	{
		return MainLevel.Instance.m_SceneAsyncOperation.Count == 0;
	}

	private bool CanLoad()
	{
		return MainLevel.Instance.m_UnusedAssetsAsyncOperation == null || MainLevel.Instance.m_UnusedAssetsAsyncOperation.isDone;
	}

	[Tooltip("This checkbox deactivates streamer and unload or doesn't load it's data.")]
	public bool streamerActive = true;

	public static string STREAMERTAG = "SceneStreamer";

	[Tooltip("Drag and drop here your scene collection prefab. You could find it in catalogue with scenes which were generated by scene splitter.")]
	[Header("Scene Collection")]
	public SceneCollection sceneCollection;

	public SceneSplit[] splits;

	[Header("Ranges")]
	[Tooltip("Distance in grid elements that you want hold loaded.")]
	public Vector3 loadingRange = new Vector3(3f, 3f, 3f);

	[Tooltip("Enables ring streaming.")]
	public bool useLoadingRangeMin;

	[Tooltip("Area that you want to cutout from loading range.")]
	public Vector3 loadingRangeMin = new Vector3(2f, 2f, 2f);

	[Tooltip("Distance in grid elements after which you want to unload assets.")]
	public Vector3 deloadingRange = new Vector3(3f, 3f, 3f);

	[Header("Settings")]
	[Tooltip("Frequancy in seconds in which you want to check if grid element is close /far enough to load/unload.")]
	public float positionCheckTime = 0.1f;

	[Tooltip("Time in seconds after which grid element will be unloaded.")]
	public float destroyTileDelay = 2f;

	[Tooltip("Amount of max grid elements that you want to start loading in one frame.")]
	public int maxParallelSceneLoading = 1;

	[Tooltip("Number of empty frames between loading actions.")]
	public int sceneLoadWaitFrames = 2;

	[Tooltip("If you want to fix small holes from LODs system at unity terrain borders, drag and drop object here from scene hierarchy that contains our \"Terrain Neighbours\" script.")]
	[Space(10f)]
	public TerrainNeighbours terrainNeighbours;

	[Tooltip("Enable looping system, each layer is streamed independently, so if you want to synchronize them, they should have the same XYZ size. More info at manual.")]
	[Space(10f)]
	public bool looping;

	[Space(10f)]
	[Header("Player Settings")]
	[Tooltip("Drag and drop here, an object that system have to follow during streaming process.")]
	public Transform player;

	[Tooltip("Streamer will wait for player spawn and fill it automatically")]
	public bool spawnedPlayer;

	[HideInInspector]
	public string playerTag = "Player";

	[HideInInspector]
	public bool showLoadingScreen = true;

	[HideInInspector]
	public UILoadingStreamer loadingStreamer;

	[HideInInspector]
	public bool initialized;

	[HideInInspector]
	public int tilesToLoad = int.MaxValue;

	[HideInInspector]
	public int tilesLoaded;

	[HideInInspector]
	public WorldMover worldMover;

	[HideInInspector]
	public Vector3 currentMove = Vector3.zero;

	private int xPos;

	private int yPos;

	private int zPos;

	public Dictionary<int[], SceneSplit> scenesArray;

	[HideInInspector]
	public List<SceneSplit> loadedScenes = new List<SceneSplit>();

	private int currentlySceneLoading;

	private List<SceneSplit> scenesToLoad = new List<SceneSplit>();

	private int sceneLoadFrameNext;

	private bool sceneLoadFramesNextWaited;

	private int xLimity;

	private int xLimitx;

	private int xRange;

	private int yLimity;

	private int yLimitx;

	private int yRange;

	private int zLimity;

	private int zLimitx;

	private int zRange;

	private static bool canUnload = true;

	private static float waitTillNextUnload = 20f;

	private static bool unloadNext;
}
