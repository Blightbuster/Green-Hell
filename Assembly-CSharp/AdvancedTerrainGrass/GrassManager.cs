using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

namespace AdvancedTerrainGrass
{
	[RequireComponent(typeof(Terrain))]
	public class GrassManager : MonoBehaviour
	{
		private void OnEnable()
		{
			this.Init();
		}

		private void OnDisable()
		{
			this.cullingGroup.Dispose();
			for (int i = 0; i < this.CellContent.Length; i++)
			{
				this.CellContent[i].ReleaseCompleteCellContent();
			}
			this.WorkerThread.Abort();
			this.WorkerThread = null;
			this.Cells = null;
			this.CellContent = null;
			this.mapByte = null;
			this.TerrainHeights = null;
			this.v_mesh = null;
			this.v_mat = null;
			this.tempMatrixArray = null;
			this.SoftlyMergedLayers = null;
		}

		private void Update()
		{
			this.DrawGrass();
		}

		public static float GetATGRandomNext()
		{
			GrassManager.ATGSeed = (uint)((ulong)GrassManager.ATGSeed * 279470273UL % (ulong)-5);
			return GrassManager.ATGSeed * GrassManager.OneOverInt32MaxVal;
		}

		public void RefreshGrassRenderingSettings(float t_DetailDensity, float t_CullDistance, float t_FadeLength, float t_CacheDistance, float t_DetailFadeStart, float t_DetailFadeLength, float t_ShadowStart, float t_ShadowFadeLength, float t_ShadowStartFoliage, float t_ShadowFadeLengthFoliage)
		{
			this.CellsOrCellContentsToInit.Clear();
			float[] boundingDistances = new float[]
			{
				t_CullDistance,
				t_CacheDistance
			};
			this.cullingGroup.SetBoundingDistances(boundingDistances);
			this.CurrentDetailDensity = t_DetailDensity;
			Shader.SetGlobalVector(this.GrassFadePropsPID, new Vector4((t_CullDistance - t_FadeLength) * (t_CullDistance - t_FadeLength), 1f / (t_FadeLength * t_FadeLength), t_DetailFadeStart * t_DetailFadeStart, 1f / (t_DetailFadeLength * t_DetailFadeLength)));
			Shader.SetGlobalVector(this.GrassShadowFadePropsPID, new Vector4(t_ShadowStart * t_ShadowStart, 1f / (t_ShadowFadeLength * t_ShadowFadeLength), t_ShadowStartFoliage * t_ShadowStartFoliage, 1f / (t_ShadowFadeLengthFoliage * t_ShadowFadeLengthFoliage)));
			for (int i = 0; i < this.Cells.Length; i++)
			{
				GrassCell grassCell = this.Cells[i];
				if (grassCell.state == 3)
				{
					int cellContentCount = grassCell.CellContentCount;
					for (int j = 0; j < cellContentCount; j++)
					{
						this.CellContent[grassCell.CellContentIndexes[j]].ReleaseCellContent();
					}
				}
				grassCell.state = 0;
			}
			int num = Mathf.CeilToInt((float)(this.NumberOfBucketsPerCell * this.NumberOfBucketsPerCell * this.maxBucketDensity) * this.CurrentDetailDensity);
			this.tempMatrixArray = new Matrix4x4[num];
			if (this.useBurst)
			{
				this.BurstInit();
			}
		}

		public void InitCellsFast()
		{
			this.mapByte = new byte[this.SavedTerrainData.DensityMaps.Count][];
			for (int i = 0; i < this.SavedTerrainData.DensityMaps.Count; i++)
			{
				this.mapByte[i] = this.SavedTerrainData.DensityMaps[i].mapByte;
			}
			this.Cells = new GrassCell[this.SavedTerrainData.Cells.Length];
			this.CellContent = new GrassCellContent[this.SavedTerrainData.CellContent.Length];
			this.maxBucketDensity = this.SavedTerrainData.maxBucketDensity;
			int num = 0;
			int num2 = this.SavedTerrainData.Cells.Length;
			for (int j = 0; j < num2; j++)
			{
				this.Cells[j] = new GrassCell();
				this.Cells[j].index = j;
				this.Cells[j].Center = this.SavedTerrainData.Cells[j].Center;
				this.Cells[j].CellContentIndexes = this.SavedTerrainData.Cells[j].CellContentIndexes;
				this.Cells[j].CellContentCount = this.SavedTerrainData.Cells[j].CellContentCount;
				int cellContentCount = this.Cells[j].CellContentCount;
				for (int k = 0; k < cellContentCount; k++)
				{
					this.CellContent[num] = new GrassCellContent();
					GrassCellContent grassCellContent = this.CellContent[num];
					GrassCellContent grassCellContent2 = this.SavedTerrainData.CellContent[num];
					grassCellContent.index = num;
					grassCellContent.Layer = grassCellContent2.Layer;
					if (grassCellContent2.SoftlyMergedLayers.Length > 0)
					{
						grassCellContent.SoftlyMergedLayers = grassCellContent2.SoftlyMergedLayers;
					}
					else
					{
						grassCellContent.SoftlyMergedLayers = null;
					}
					grassCellContent.GrassMatrixBufferPID = this.GrassMatrixBufferPID;
					grassCellContent.Center = grassCellContent2.Center;
					grassCellContent.Pivot = grassCellContent2.Pivot;
					grassCellContent.PatchOffsetX = grassCellContent2.PatchOffsetX;
					grassCellContent.PatchOffsetZ = grassCellContent2.PatchOffsetZ;
					grassCellContent.Instances = grassCellContent2.Instances;
					grassCellContent.block = new MaterialPropertyBlock();
					grassCellContent.argsBuffer = new ComputeBuffer(1, 20, ComputeBufferType.DrawIndirect);
					num++;
				}
			}
		}

		public void InitCells()
		{
			this.NumberOfLayers = this.terData.detailPrototypes.Length;
			this.OrigNumberOfLayers = this.NumberOfLayers;
			int[][] array = new int[this.OrigNumberOfLayers][];
			int[][] array2 = new int[this.OrigNumberOfLayers][];
			for (int i = 0; i < this.OrigNumberOfLayers; i++)
			{
				int num = this.LayerToMergeWith[i];
				int num2 = num - 1;
				if (num != 0 && num != i + 1 && this.LayerToMergeWith[num2] == 0)
				{
					if (array[num2] == null)
					{
						array[num2] = new int[this.OrigNumberOfLayers - 1];
					}
					if (this.DoSoftMerge[i] && array2[num2] == null)
					{
						array2[num2] = new int[this.OrigNumberOfLayers - 1];
					}
					for (int j = 0; j < this.OrigNumberOfLayers - 1; j++)
					{
						if (array[num2][j] == 0)
						{
							array[num2][j] = i + 1;
							break;
						}
					}
					if (this.DoSoftMerge[i])
					{
						for (int k = 0; k < this.OrigNumberOfLayers - 1; k++)
						{
							if (array2[num2][k] == 0)
							{
								array2[num2][k] = i + 1;
								break;
							}
						}
					}
				}
			}
			this.Cells = new GrassCell[this.NumberOfCells * this.NumberOfCells];
			List<GrassCellContent> list = new List<GrassCellContent>();
			list.Capacity = this.NumberOfCells * this.NumberOfCells * this.NumberOfLayers;
			this.mapByte = new byte[this.NumberOfLayers][];
			for (int l = 0; l < this.NumberOfLayers; l++)
			{
				if (this.LayerToMergeWith[l] == 0 || this.DoSoftMerge[l])
				{
					this.mapByte[l] = new byte[(int)(this.TerrainDetailSize.x * this.TerrainDetailSize.y)];
					bool flag = false;
					if (array[l] != null && !this.DoSoftMerge[l])
					{
						flag = true;
					}
					for (int m = 0; m < (int)this.TerrainDetailSize.x; m++)
					{
						for (int n = 0; n < (int)this.TerrainDetailSize.y; n++)
						{
							int[,] detailLayer = this.terData.GetDetailLayer(m, n, 1, 1, l);
							this.mapByte[l][m * (int)this.TerrainDetailSize.y + n] = Convert.ToByte(detailLayer[0, 0]);
							if (flag)
							{
								for (int num3 = 0; num3 < this.OrigNumberOfLayers - 1; num3++)
								{
									if (array[l][num3] == 0)
									{
										break;
									}
									detailLayer = this.terData.GetDetailLayer(m, n, 1, 1, array[l][num3] - 1);
									this.mapByte[l][m * (int)this.TerrainDetailSize.y + n] = Convert.ToByte((int)this.mapByte[l][m * (int)this.TerrainDetailSize.y + n] + detailLayer[0, 0]);
								}
							}
						}
					}
				}
			}
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			for (int num7 = 0; num7 < this.NumberOfCells; num7++)
			{
				for (int num8 = 0; num8 < this.NumberOfCells; num8++)
				{
					int num9 = num7 * this.NumberOfCells + num8;
					Vector2 vector;
					vector.x = ((float)num7 * this.CellSize + 0.5f * this.CellSize) * this.OneOverTerrainSize.x;
					vector.y = ((float)num8 * this.CellSize + 0.5f * this.CellSize) * this.OneOverTerrainSize.z;
					float interpolatedHeight = this.terData.GetInterpolatedHeight(vector.x, vector.y);
					this.Cells[num9] = new GrassCell();
					this.Cells[num9].index = num9;
					Vector3 center;
					center.x = (float)num7 * this.CellSize + 0.5f * this.CellSize + this.TerrainPosition.x;
					center.y = interpolatedHeight + this.TerrainPosition.y;
					center.z = (float)num8 * this.CellSize + 0.5f * this.CellSize + this.TerrainPosition.z;
					this.Cells[num9].Center = center;
					for (int num10 = 0; num10 < this.NumberOfLayers; num10++)
					{
						if (this.LayerToMergeWith[num10] == 0)
						{
							for (int num11 = 0; num11 < this.NumberOfBucketsPerCell; num11++)
							{
								for (int num12 = 0; num12 < this.NumberOfBucketsPerCell; num12++)
								{
									int num13 = (int)this.mapByte[num10][num7 * this.NumberOfBucketsPerCell * (int)this.TerrainDetailSize.y + num11 * (int)this.TerrainDetailSize.y + num8 * this.NumberOfBucketsPerCell + num12];
									if (num13 > this.maxBucketDensity)
									{
										this.maxBucketDensity = num13;
									}
									num4 += num13;
								}
							}
							int num14 = 0;
							if (array2[num10] != null)
							{
								for (int num15 = 0; num15 < this.OrigNumberOfLayers - 1; num15++)
								{
									if (array2[num10][num15] == 0)
									{
										break;
									}
									int num16 = array2[num10][num15] - 1;
									int num17 = 0;
									for (int num18 = 0; num18 < this.NumberOfBucketsPerCell; num18++)
									{
										for (int num19 = 0; num19 < this.NumberOfBucketsPerCell; num19++)
										{
											int num13 = (int)this.mapByte[num16][num7 * this.NumberOfBucketsPerCell * (int)this.TerrainDetailSize.y + num18 * (int)this.TerrainDetailSize.y + num8 * this.NumberOfBucketsPerCell + num19];
											num17 += num13;
										}
									}
									if (num17 > 0)
									{
										num14++;
										num4 += num17;
									}
								}
								if (num14 * 16 > this.maxBucketDensity)
								{
									this.maxBucketDensity = num14 * 16 + 32;
								}
							}
							if (num4 > 0)
							{
								this.Cells[num9].CellContentIndexes.Add(num5);
								this.Cells[num9].CellContentCount++;
								GrassCellContent grassCellContent = new GrassCellContent();
								grassCellContent.index = num5;
								grassCellContent.Layer = num10;
								grassCellContent.GrassMatrixBufferPID = this.GrassMatrixBufferPID;
								grassCellContent.Center = center;
								grassCellContent.Pivot = new Vector3((float)num7 * this.CellSize, interpolatedHeight, (float)num8 * this.CellSize);
								grassCellContent.PatchOffsetX = num7 * this.NumberOfBucketsPerCell * (int)this.TerrainDetailSize.y;
								grassCellContent.PatchOffsetZ = num8 * this.NumberOfBucketsPerCell;
								grassCellContent.Instances = num4;
								grassCellContent.block = new MaterialPropertyBlock();
								grassCellContent.argsBuffer = new ComputeBuffer(1, 20, ComputeBufferType.DrawIndirect);
								if (num14 > 0)
								{
									List<int> list2 = new List<int>();
									for (int num20 = 0; num20 < this.OrigNumberOfLayers - 1; num20++)
									{
										if (array2[num10][num20] != 0)
										{
											list2.Add(array2[num10][num20] - 1);
										}
									}
									grassCellContent.SoftlyMergedLayers = list2.ToArray();
								}
								list.Add(grassCellContent);
								num5++;
							}
							num4 = 0;
						}
					}
				}
				num6 += (int)this.TerrainDetailSize.x;
			}
			this.CellContent = list.ToArray();
			list.Clear();
		}

		public void Init()
		{
			Shader.SetGlobalFloat("_AtgWindGust", 0f);
			Shader.SetGlobalVector("_AtgWindDirSize", new Vector4(1f, 0f, 0f, 0f));
			Shader.SetGlobalVector("_AtgWindStrengthMultipliers", new Vector4(0f, 0f, 0f, 0f));
			this.ter = base.GetComponent<Terrain>();
			this.terData = this.ter.terrainData;
			this.ter.detailObjectDistance = 0f;
			this.CurrentDetailDensity = this.DetailDensity;
			this.TerrainPosition = this.ter.GetPosition();
			this.TerrainSize = this.terData.size;
			this.OneOverTerrainSize.x = 1f / this.TerrainSize.x;
			this.OneOverTerrainSize.y = 1f / this.TerrainSize.y;
			this.OneOverTerrainSize.z = 1f / this.TerrainSize.z;
			this.TerrainDetailSize.x = (float)this.terData.detailWidth;
			this.TerrainDetailSize.y = (float)this.terData.detailHeight;
			this.BucketSize = this.TerrainSize.x / this.TerrainDetailSize.x;
			this.NumberOfBucketsPerCell = (int)this.NumberOfBucketsPerCellEnum;
			this.CellSize = (float)this.NumberOfBucketsPerCell * this.BucketSize;
			this.NumberOfCells = (int)(this.TerrainSize.x / this.CellSize);
			this.TotalCellCount = this.NumberOfCells * this.NumberOfCells;
			this.sh = Shader.Find("AdvancedTerrainGrass/Grass Base Shader");
			this.GrassMatrixBufferPID = Shader.PropertyToID("GrassMatrixBuffer");
			this.GrassFadePropsPID = Shader.PropertyToID("_AtgGrassFadeProps");
			this.GrassShadowFadePropsPID = Shader.PropertyToID("_AtgGrassShadowFadeProps");
			float num = this.CellSize * 0.5f * (this.CellSize * 0.5f);
			float num2 = Mathf.Sqrt(num * 2f);
			float num3 = this.CullDistance - this.FadeLength;
			Shader.SetGlobalVector(this.GrassFadePropsPID, new Vector4(num3 * num3, 1f / (this.FadeLength * this.FadeLength * (num3 / this.FadeLength * 2f)), this.DetailFadeStart * this.DetailFadeStart, 1f / (this.DetailFadeLength * this.DetailFadeLength)));
			Shader.SetGlobalVector(this.GrassShadowFadePropsPID, new Vector4(this.ShadowStart * this.ShadowStart, 1f / (this.ShadowFadeLength * this.ShadowFadeLength), this.ShadowStartFoliage * this.ShadowStartFoliage, 1f / (this.ShadowFadeLengthFoliage * this.ShadowFadeLengthFoliage)));
			if (this.SavedTerrainData != null)
			{
				this.InitCellsFast();
				this.TotalCellCount = this.Cells.Length;
			}
			else
			{
				this.InitCells();
			}
			this.TerrainHeightmapWidth = this.terData.heightmapWidth;
			this.TerrainHeightmapHeight = this.terData.heightmapHeight;
			this.TerrainHeights = new float[this.TerrainHeightmapWidth * this.TerrainHeightmapHeight];
			for (int i = 0; i < this.TerrainHeightmapWidth; i++)
			{
				for (int j = 0; j < this.TerrainHeightmapHeight; j++)
				{
					this.TerrainHeights[i * this.TerrainHeightmapWidth + j] = this.terData.GetHeight(i, j);
				}
			}
			this.OneOverHeightmapWidth = 1f / (float)this.TerrainHeightmapWidth;
			this.OneOverHeightmapHeight = 1f / (float)this.TerrainHeightmapHeight;
			this.TerrainSizeOverHeightmap = this.TerrainSize.x / (float)this.TerrainHeightmapWidth;
			this.OneOverHeightmapWidthRight = this.TerrainSize.x - 2f * (this.TerrainSize.x / (float)(this.TerrainHeightmapWidth - 1)) - 1f;
			this.OneOverHeightmapHeightUp = this.TerrainSize.z - 2f * (this.TerrainSize.z / (float)(this.TerrainHeightmapHeight - 1)) - 1f;
			this.cullingGroup = new CullingGroup();
			this.boundingSpheres = new BoundingSphere[this.TotalCellCount];
			this.resultIndices = new int[this.TotalCellCount];
			this.isVisibleBoundingSpheres = new bool[this.TotalCellCount];
			num = this.CellSize * 0.75f * (this.CellSize * 0.75f);
			float rad = Mathf.Sqrt(num * 2f);
			for (int k = 0; k < this.TotalCellCount; k++)
			{
				this.boundingSpheres[k] = new BoundingSphere(this.Cells[k].Center, rad);
				this.isVisibleBoundingSpheres[k] = false;
			}
			this.cullingGroup.SetBoundingSpheres(this.boundingSpheres);
			this.cullingGroup.SetBoundingSphereCount(this.TotalCellCount);
			float[] boundingDistances = new float[]
			{
				this.CullDistance,
				this.CacheDistance
			};
			this.cullingGroup.SetBoundingDistances(boundingDistances);
			this.cullingGroup.onStateChanged = new CullingGroup.StateChanged(this.StateChangedMethod);
			this.SqrTerrainCullingDist = Mathf.Sqrt(this.TerrainSize.x * this.TerrainSize.x + this.TerrainSize.z * this.TerrainSize.z) + this.CullDistance;
			this.SqrTerrainCullingDist *= this.SqrTerrainCullingDist;
			Debug.Log("Max Bucket Density: " + this.maxBucketDensity);
			int num4 = Mathf.CeilToInt((float)(this.NumberOfBucketsPerCell * this.NumberOfBucketsPerCell * this.maxBucketDensity) * this.CurrentDetailDensity);
			this.tempMatrixArray = new Matrix4x4[num4];
			Debug.Log("Max Instances per Layer per Cell: " + num4);
			if (this.useBurst)
			{
				this.BurstInit();
			}
			this.WorkerThread = new Thread(new ThreadStart(this.InitCellContentOnThread));
			this.WorkerThread.Start();
		}

		public void BurstInit()
		{
			if (this.Cam == null)
			{
				this.Cam = Camera.main;
				if (this.Cam == null)
				{
					return;
				}
			}
			this.CamTransform = this.Cam.transform;
			if ((this.TerrainPosition - this.CamTransform.position).sqrMagnitude > this.SqrTerrainCullingDist)
			{
				return;
			}
			int num = this.Cells.Length;
			for (int i = 0; i < num; i++)
			{
				GrassCell grassCell = this.Cells[i];
				float num2 = Vector3.Distance(this.CamTransform.position, grassCell.Center);
				if (num2 < this.BurstRadius)
				{
					int cellContentCount = grassCell.CellContentCount;
					for (int j = 0; j < cellContentCount; j++)
					{
						GrassCellContent grassCellContent = this.CellContent[grassCell.CellContentIndexes[j]];
						int layer = grassCellContent.Layer;
						grassCellContent.v_mat = this.v_mat[layer];
						grassCellContent.v_mesh = this.v_mesh[layer];
						grassCellContent.ShadowCastingMode = this.ShadowCastingMode[layer];
					}
					this.InitCellContent(grassCell.index);
					this.ThreadIsRunning = false;
					for (int k = 0; k < cellContentCount; k++)
					{
						GrassCellContent grassCellContent2 = this.CellContent[grassCell.CellContentIndexes[k]];
						grassCellContent2.InitCellContent_Delegated();
					}
					grassCell.state = 3;
				}
			}
		}

		private void StateChangedMethod(CullingGroupEvent evt)
		{
			if (evt.isVisible)
			{
				this.isVisibleBoundingSpheres[evt.index] = true;
			}
			else
			{
				this.isVisibleBoundingSpheres[evt.index] = false;
			}
			if (evt.currentDistance == 2)
			{
				GrassCell grassCell = this.Cells[evt.index];
				if (grassCell.state != 2)
				{
					int cellContentCount = grassCell.CellContentCount;
					for (int i = 0; i < cellContentCount; i++)
					{
						this.CellContent[grassCell.CellContentIndexes[i]].ReleaseCellContent();
					}
					if (grassCell.state == 1)
					{
						this.CellsOrCellContentsToInit.Remove(grassCell.index);
					}
					grassCell.state = 0;
				}
			}
		}

		private void DrawGrass()
		{
			if (this.Cam == null)
			{
				this.Cam = Camera.main;
				if (this.Cam == null)
				{
					return;
				}
			}
			this.CamTransform = this.Cam.transform;
			this.cullingGroup.targetCamera = this.Cam;
			if ((this.TerrainPosition - this.CamTransform.position).sqrMagnitude > this.SqrTerrainCullingDist)
			{
				return;
			}
			if (this.CameraSelection == GrassCameras.AllCameras)
			{
				this.CameraInWichGrassWillBeDrawn = null;
			}
			else
			{
				this.CameraInWichGrassWillBeDrawn = this.Cam;
			}
			this.cullingGroup.SetDistanceReferencePoint(this.CamTransform.position);
			if (this.IngnoreOcclusion)
			{
				this.numResults = this.cullingGroup.QueryIndices(0, this.resultIndices, 0);
			}
			else
			{
				this.numResults = this.cullingGroup.QueryIndices(true, 0, this.resultIndices, 0);
			}
			if (this.numResults == this.TotalCellCount)
			{
				return;
			}
			this.numOfVisibleCells = 0;
			for (int i = 0; i < this.numResults; i++)
			{
				if (!this.IngnoreOcclusion || this.isVisibleBoundingSpheres[this.resultIndices[i]])
				{
					this.numOfVisibleCells++;
					GrassCell grassCell = this.Cells[this.resultIndices[i]];
					int state = grassCell.state;
					int cellContentCount = grassCell.CellContentCount;
					switch (state)
					{
					case 0:
						if (grassCell.CellContentCount > 0)
						{
							grassCell.state = 1;
							this.CellsOrCellContentsToInit.Add(grassCell.index);
						}
						break;
					case 2:
						for (int j = 0; j < cellContentCount; j++)
						{
							this.CellContent[grassCell.CellContentIndexes[j]].InitCellContent_Delegated();
						}
						grassCell.state = 3;
						for (int k = 0; k < cellContentCount; k++)
						{
							this.CellContent[grassCell.CellContentIndexes[k]].DrawCellContent_Delegated(this.CameraInWichGrassWillBeDrawn, this.CameraLayer);
						}
						break;
					case 3:
						for (int l = 0; l < cellContentCount; l++)
						{
							this.CellContent[grassCell.CellContentIndexes[l]].DrawCellContent_Delegated(this.CameraInWichGrassWillBeDrawn, this.CameraLayer);
						}
						break;
					}
				}
			}
			int num = this.CellsOrCellContentsToInit.Count;
			if (!this.ThreadIsRunning)
			{
				this.wt_cellindexList.Clear();
				this.wt_cellindexListCount = 0;
				for (int m = 0; m < this.CellsPerFrame; m++)
				{
					if (num <= 0)
					{
						break;
					}
					if (this.Cells[this.CellsOrCellContentsToInit[0]].state != 1)
					{
						this.CellsOrCellContentsToInit.RemoveAt(0);
						if (num == 1)
						{
							return;
						}
					}
					GrassCell grassCell = this.Cells[this.CellsOrCellContentsToInit[0]];
					int cellContentCount = grassCell.CellContentCount;
					for (int n = 0; n < cellContentCount; n++)
					{
						GrassCellContent grassCellContent = this.CellContent[grassCell.CellContentIndexes[n]];
						int layer = grassCellContent.Layer;
						grassCellContent.v_mat = this.v_mat[layer];
						grassCellContent.v_mesh = this.v_mesh[layer];
						grassCellContent.ShadowCastingMode = this.ShadowCastingMode[layer];
					}
					this.wt_cellindex = this.CellsOrCellContentsToInit[0];
					this.wt_cellindexList.Add(this.CellsOrCellContentsToInit[0]);
					this.CellsOrCellContentsToInit.RemoveAt(0);
					num--;
					this.wt_cellindexListCount++;
				}
				if (this.wt_cellindexListCount > 0)
				{
					this.WorkerThreadWait.Set();
				}
			}
		}

		public float GetfilteredHeight(float normalizedHeightPos_x, float normalizedHeightPos_y)
		{
			int num = (int)normalizedHeightPos_x;
			int num2 = (int)normalizedHeightPos_y;
			int num3 = (int)(normalizedHeightPos_x + 1f);
			int num4 = (int)(normalizedHeightPos_y + 1f);
			float num5 = normalizedHeightPos_x - (float)num;
			float num6 = (float)num3 - normalizedHeightPos_x;
			float num7 = normalizedHeightPos_y - (float)num2;
			float num8 = (float)num4 - normalizedHeightPos_y;
			num *= this.TerrainHeightmapHeight;
			num3 *= this.TerrainHeightmapHeight;
			float num9 = this.TerrainHeights[num + num2] * num6;
			num9 += this.TerrainHeights[num3 + num2] * num5;
			float num10 = this.TerrainHeights[num + num4] * num6;
			num10 += this.TerrainHeights[num3 + num4] * num5;
			return num9 * num8 + num10 * num7;
		}

		private void InitCellContentOnThread()
		{
			this.WorkerThreadWait.Reset();
			this.WorkerThreadWait.WaitOne();
			for (;;)
			{
				this.WorkerThreadWait.Reset();
				this.ThreadIsRunning = true;
				for (int i = 0; i < this.wt_cellindexListCount; i++)
				{
					this.InitCellContent(this.wt_cellindexList[i]);
				}
				this.ThreadIsRunning = false;
				WaitHandle.SignalAndWait(this.MainThreadWait, this.WorkerThreadWait);
			}
		}

		public float InitCellContent(int cellIndex)
		{
			GrassCell grassCell = this.Cells[cellIndex];
			int cellContentCount = grassCell.CellContentCount;
			for (int i = 0; i < cellContentCount; i++)
			{
				int num = grassCell.CellContentIndexes[i];
				GrassCellContent grassCellContent = this.CellContent[num];
				int num2 = grassCellContent.Layer;
				int num3 = 0;
				this.samplePosition.x = grassCellContent.Pivot.x;
				this.samplePosition.y = grassCellContent.Pivot.z;
				this.tempSamplePosition = this.samplePosition;
				int num4 = (int)this.InstanceRotation[num2];
				bool flag = this.WriteNormalBuffer[num2];
				float num5 = this.Noise[num2];
				float num6 = this.MinSize[num2];
				float num7 = this.MaxSize[num2] - num6;
				int num8 = 1;
				if (grassCellContent.SoftlyMergedLayers != null)
				{
					num8 += grassCellContent.SoftlyMergedLayers.Length;
				}
				for (int j = 0; j < num8; j++)
				{
					this.tempSamplePosition = this.samplePosition;
					if (j > 0)
					{
						num2 = grassCellContent.SoftlyMergedLayers[j - 1];
						num5 = this.Noise[num2];
						num6 = this.MinSize[num2];
						num7 = this.MaxSize[num2] - num6;
					}
					for (int k = 0; k < this.NumberOfBucketsPerCell; k++)
					{
						for (int l = 0; l < this.NumberOfBucketsPerCell; l++)
						{
							int num9 = cellIndex + num2 + j * 55;
							GrassManager.ATGSeed = (uint)((float)(num9 - (k * this.NumberOfBucketsPerCell + l)) * 0.0001f * 2.14748365E+09f);
							Vector2 vector;
							vector.x = this.tempSamplePosition.x;
							vector.y = this.tempSamplePosition.y;
							int num10 = (int)this.mapByte[num2][grassCellContent.PatchOffsetX + grassCellContent.PatchOffsetZ + k * (int)this.TerrainDetailSize.y + l];
							num10 = (int)Math.Ceiling((double)((float)num10 * this.CurrentDetailDensity));
							float num11 = (vector.x >= this.TerrainSizeOverHeightmap) ? this.OneOverHeightmapWidth : 0f;
							float num12 = (vector.x < this.OneOverHeightmapWidthRight) ? this.OneOverHeightmapWidth : 0f;
							float num13 = (vector.y >= this.TerrainSizeOverHeightmap) ? this.OneOverHeightmapHeight : 0f;
							float num14 = (vector.y < this.OneOverHeightmapHeightUp) ? this.OneOverHeightmapHeight : 0f;
							for (int m = 0; m < num10; m++)
							{
								float atgrandomNext = GrassManager.GetATGRandomNext();
								float num15 = atgrandomNext * this.BucketSize;
								atgrandomNext = GrassManager.GetATGRandomNext();
								float num16 = atgrandomNext * this.BucketSize;
								Vector2 vector2;
								vector2.x = (vector.x + num15) * this.OneOverTerrainSize.x;
								vector2.y = (vector.y + num16) * this.OneOverTerrainSize.z;
								float num17 = vector2.x * (float)(this.TerrainHeightmapWidth - 1);
								float num18 = vector2.y * (float)(this.TerrainHeightmapHeight - 1);
								int num19 = (int)num17;
								int num20 = (int)num18;
								int num21 = (int)(num17 + 1f);
								int num22 = (int)(num18 + 1f);
								float num23 = num17 - (float)num19;
								float num24 = (float)num21 - num17;
								float num25 = num18 - (float)num20;
								float num26 = (float)num22 - num18;
								num19 *= this.TerrainHeightmapHeight;
								num21 *= this.TerrainHeightmapHeight;
								float num27 = this.TerrainHeights[num19 + num20] * num24;
								num27 += this.TerrainHeights[num21 + num20] * num23;
								float num28 = this.TerrainHeights[num19 + num22] * num24;
								num28 += this.TerrainHeights[num21 + num22] * num23;
								this.tempPosition.y = num27 * num26 + num28 * num25;
								float num29 = this.GetfilteredHeight((vector2.x - num11) * (float)(this.TerrainHeightmapWidth - 1), vector2.y * (float)(this.TerrainHeightmapHeight - 1));
								float num30 = this.GetfilteredHeight((vector2.x + num12) * (float)(this.TerrainHeightmapWidth - 1), vector2.y * (float)(this.TerrainHeightmapHeight - 1));
								float num31 = this.GetfilteredHeight(vector2.x * (float)(this.TerrainHeightmapWidth - 1), (vector2.y + num14) * (float)(this.TerrainHeightmapHeight - 1));
								float num32 = this.GetfilteredHeight(vector2.x * (float)(this.TerrainHeightmapWidth - 1), (vector2.y - num13) * (float)(this.TerrainHeightmapHeight - 1));
								Vector3 vector3;
								vector3.x = -2f * (num30 - num29);
								if (num4 != 2 && num4 != 4)
								{
									vector3.y = 6.283184f * this.TerrainSizeOverHeightmap;
								}
								else
								{
									vector3.y = 4f * this.TerrainSizeOverHeightmap;
								}
								vector3.z = (num31 - num32) * -2f;
								float num33 = vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z;
								float num34 = (float)Math.Sqrt((double)num33);
								float num35 = 1f / num34;
								vector3.x *= num35;
								vector3.y *= num35;
								vector3.z *= num35;
								this.tempPosition.x = this.tempSamplePosition.x + num15 + this.TerrainPosition.x;
								this.tempPosition.z = this.tempSamplePosition.y + num16 + this.TerrainPosition.z;
								float num36 = num6 + Mathf.PerlinNoise(this.tempPosition.x * num5, this.tempPosition.z * num5) * num7;
								this.tempScale.x = num36;
								this.tempScale.y = num36;
								this.tempScale.z = num36;
								Quaternion zeroQuat = this.ZeroQuat;
								if (num4 != 2)
								{
									zeroQuat.x = vector3.z;
									zeroQuat.y = 0f;
									zeroQuat.z = -vector3.x;
									zeroQuat.w = (float)Math.Sqrt((double)(1f + vector3.y));
									float num37 = (float)(1.0 / Math.Sqrt((double)(zeroQuat.w * zeroQuat.w + zeroQuat.x * zeroQuat.x + zeroQuat.y * zeroQuat.y + zeroQuat.z * zeroQuat.z)));
									zeroQuat.w *= num37;
									zeroQuat.x *= num37;
									zeroQuat.y *= num37;
									zeroQuat.z *= num37;
								}
								float num38 = GrassManager.GetATGRandomNext() * 180f;
								float num39 = (float)Math.Cos((double)num38);
								float num40 = (float)Math.Sin((double)num38);
								Quaternion quaternion = zeroQuat;
								zeroQuat.x = quaternion.x * num39 - quaternion.z * num40;
								zeroQuat.y = quaternion.w * num40 + quaternion.y * num39;
								zeroQuat.z = quaternion.z * num39 + quaternion.x * num40;
								zeroQuat.w = quaternion.w * num39 - quaternion.y * num40;
								if (num4 == 1)
								{
									num38 = GrassManager.GetATGRandomNext() * 180f;
									float num41 = (float)Math.Sin((double)num38);
									float num42 = (float)Math.Cos((double)num38);
									quaternion = zeroQuat;
									zeroQuat.x = quaternion.w * num41 + quaternion.x * num42;
									zeroQuat.y = quaternion.y * num42 + quaternion.z * num41;
									zeroQuat.z = quaternion.z * num42 - quaternion.y * num41;
									zeroQuat.w = quaternion.w * num42 - quaternion.x * num41;
								}
								this.tempMatrix.m03 = this.tempPosition.x;
								this.tempMatrix.m13 = this.tempPosition.y + this.TerrainPosition.y;
								this.tempMatrix.m23 = this.tempPosition.z;
								float num43 = 2f * zeroQuat.x * zeroQuat.x;
								float num44 = 2f * zeroQuat.y * zeroQuat.y;
								float num45 = 2f * zeroQuat.z * zeroQuat.z;
								this.tempMatrix.m00 = 1f - num44 - num45;
								this.tempMatrix.m01 = 2f * zeroQuat.x * zeroQuat.y - 2f * zeroQuat.z * zeroQuat.w;
								this.tempMatrix.m02 = 2f * zeroQuat.x * zeroQuat.z + 2f * zeroQuat.y * zeroQuat.w;
								this.tempMatrix.m10 = 2f * zeroQuat.x * zeroQuat.y + 2f * zeroQuat.z * zeroQuat.w;
								this.tempMatrix.m11 = 1f - num43 - num45;
								this.tempMatrix.m12 = 2f * zeroQuat.y * zeroQuat.z - 2f * zeroQuat.x * zeroQuat.w;
								this.tempMatrix.m20 = 2f * zeroQuat.x * zeroQuat.z - 2f * zeroQuat.y * zeroQuat.w;
								this.tempMatrix.m21 = 2f * zeroQuat.y * zeroQuat.z + 2f * zeroQuat.x * zeroQuat.w;
								this.tempMatrix.m22 = 1f - num43 - num44;
								this.tempMatrix.m00 = this.tempMatrix.m00 * this.tempScale.x;
								this.tempMatrix.m01 = this.tempMatrix.m01 * this.tempScale.y;
								this.tempMatrix.m02 = this.tempMatrix.m02 * this.tempScale.z;
								this.tempMatrix.m10 = this.tempMatrix.m10 * this.tempScale.x;
								this.tempMatrix.m11 = this.tempMatrix.m11 * this.tempScale.y;
								this.tempMatrix.m12 = this.tempMatrix.m12 * this.tempScale.z;
								this.tempMatrix.m20 = this.tempMatrix.m20 * this.tempScale.x;
								this.tempMatrix.m21 = this.tempMatrix.m21 * this.tempScale.y;
								this.tempMatrix.m22 = this.tempMatrix.m22 * this.tempScale.z;
								if (num4 == 2 && flag)
								{
									Vector3 vector4 = vector3;
									float num46 = -num38;
									num39 = (float)Math.Cos((double)num46);
									num40 = (float)Math.Sin((double)num46);
									float num47 = num40 * 2f;
									float num48 = num40 * num47;
									float num49 = num39 * num47;
									vector4.x = (1f - num48) * vector3.x + num49 * vector3.z;
									vector4.z = -num49 * vector3.x + (1f - num48) * vector3.z;
									this.tempMatrix.m30 = vector4.x;
									this.tempMatrix.m31 = vector4.y;
									this.tempMatrix.m32 = vector4.z;
								}
								else
								{
									this.tempMatrix.m30 = 0f;
									this.tempMatrix.m31 = 0f;
									this.tempMatrix.m32 = 0f;
								}
								this.tempMatrix.m33 = (float)j + this.tempScale.x * 0.01f;
								this.tempMatrixArray[num3] = this.tempMatrix;
								num3++;
							}
							this.tempSamplePosition.y = this.tempSamplePosition.y + this.BucketSize;
						}
						this.tempSamplePosition.y = this.samplePosition.y;
						this.tempSamplePosition.x = this.tempSamplePosition.x + this.BucketSize;
					}
				}
				grassCellContent.v_matrices = new Matrix4x4[num3];
				Array.Copy(this.tempMatrixArray, grassCellContent.v_matrices, num3);
			}
			grassCell.state = 2;
			return 1f;
		}

		public Camera Cam;

		public bool IngnoreOcclusion = true;

		private Transform CamTransform;

		public bool showGrid;

		private GameObject go;

		public Material ProjMat;

		public System.Random random = new System.Random(1234);

		public static uint ATGSeed;

		public static float OneOverInt32MaxVal = 2.32830644E-10f;

		public Terrain ter;

		public TerrainData terData;

		public GrassTerrainDefinitions SavedTerrainData;

		public bool useBurst;

		public float BurstRadius = 50f;

		[Range(1f, 8f)]
		public int CellsPerFrame = 1;

		public float DetailDensity = 1f;

		private float CurrentDetailDensity;

		private Vector3 TerrainPosition;

		private Vector3 TerrainSize;

		private Vector3 OneOverTerrainSize;

		private Vector2 TerrainDetailSize;

		private float SqrTerrainCullingDist;

		private bool ThreadIsRunning;

		private Thread WorkerThread;

		private EventWaitHandle WorkerThreadWait = new EventWaitHandle(true, EventResetMode.ManualReset);

		private EventWaitHandle MainThreadWait = new EventWaitHandle(true, EventResetMode.ManualReset);

		private int wt_cellindex;

		private List<int> wt_cellindexList = new List<int>();

		private int wt_cellindexListCount;

		private int TerrainHeightmapWidth;

		private int TerrainHeightmapHeight;

		private float[] TerrainHeights;

		private float OneOverHeightmapWidth;

		private float OneOverHeightmapHeight;

		private float TerrainSizeOverHeightmap;

		private float OneOverHeightmapWidthRight;

		private float OneOverHeightmapHeightUp;

		public GrassCameras CameraSelection;

		private Camera CameraInWichGrassWillBeDrawn;

		public int CameraLayer;

		private CullingGroup cullingGroup;

		private BoundingSphere[] boundingSpheres;

		private int numResults;

		private int[] resultIndices;

		private bool[] isVisibleBoundingSpheres;

		private int numOfVisibleCells;

		public float CullDistance = 80f;

		public float FadeLength = 20f;

		public float CacheDistance = 120f;

		public float DetailFadeStart = 20f;

		public float DetailFadeLength = 30f;

		public float ShadowStart = 10f;

		public float ShadowFadeLength = 20f;

		public float ShadowStartFoliage = 30f;

		public float ShadowFadeLengthFoliage = 20f;

		[Space(12f)]
		private int NumberOfLayers;

		private int OrigNumberOfLayers;

		public Mesh[] v_mesh;

		public Material[] v_mat;

		public RotationMode[] InstanceRotation;

		public bool[] WriteNormalBuffer;

		public ShadowCastingMode[] ShadowCastingMode;

		public float[] MinSize;

		public float[] MaxSize;

		public float[] Noise;

		public int[] LayerToMergeWith;

		public bool[] DoSoftMerge;

		public int[][] SoftlyMergedLayers;

		private byte[][] mapByte;

		private int GrassMatrixBufferPID;

		private int TotalCellCount;

		private int NumberOfCells;

		public BucketsPerCell NumberOfBucketsPerCellEnum = BucketsPerCell._16x16;

		private int NumberOfBucketsPerCell;

		private float CellSize;

		private float BucketSize;

		private int maxBucketDensity;

		private Matrix4x4[] tempMatrixArray;

		private Vector2 samplePosition;

		private Vector2 tempSamplePosition;

		private GrassCell[] Cells;

		private GrassCellContent[] CellContent;

		private List<int> CellsOrCellContentsToInit = new List<int>();

		private Shader sh;

		private int GrassFadePropsPID;

		private Vector4 GrassFadeProps;

		private int GrassShadowFadePropsPID;

		private Vector2 GrassShadowFadeProps;

		private Matrix4x4 tempMatrix = Matrix4x4.identity;

		private Vector3 tempPosition;

		private Quaternion tempRotation;

		private Vector3 tempScale;

		private Quaternion ZeroQuat = Quaternion.identity;

		public bool FreezeSizeAndColor;

		public bool DebugStats;

		public bool DebugCells;

		public bool FirstTimeSynced;

		public int LayerEditMode;

		public int LayerSelection;

		public bool Foldout_Rendersettings = true;

		public bool Foldout_Prototypes = true;
	}
}
