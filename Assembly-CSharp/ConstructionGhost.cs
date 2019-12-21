using System;
using System.Collections.Generic;
using System.Linq;
using CJTools;
using Enums;
using UnityEngine;

public class ConstructionGhost : Trigger, IReplicatedBehaviour
{
	protected override void Awake()
	{
		base.Awake();
		Item component = this.m_ResultPrefab.GetComponent<Item>();
		if (!component)
		{
			DebugUtils.Assert("[ConstructionGhost::Start] Missing Item component in result prefab!", true, DebugUtils.AssertType.Info);
			return;
		}
		this.m_ResultItemID = (ItemID)Enum.Parse(typeof(ItemID), component.m_InfoName);
		this.m_ResultInfo = (ConstructionInfo)ItemsManager.Get().GetInfo(this.m_ResultItemID);
		this.m_ActiveMaterialName = this.m_ActiveMaterial.name;
		this.m_HighlightedMaterialName = this.m_HighlightedMaterial.name;
		this.m_AdditionalPlacingConditions.Add(this.m_PlacingCondition);
		if (ConstructionGhostManager.Get())
		{
			ConstructionGhostManager.Get().RegisterGhost(this);
		}
		Transform transform = base.transform;
		while (transform.parent != null)
		{
			transform = transform.parent;
		}
		if (transform.CompareTag("Challenges"))
		{
			this.m_Challenge = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this.m_Rotate)
		{
			Player.Get().UnblockRotation();
		}
		if (ConstructionGhostManager.Get())
		{
			ConstructionGhostManager.Get().UnregisterGhost(this);
		}
	}

	protected override void Start()
	{
		base.Start();
		GameObject gameObject = base.gameObject.FindChild("Colliders");
		this.m_TestColliders = (gameObject ? gameObject.GetComponentsInChildren<BoxCollider>() : null);
		this.m_BoxCollider = base.gameObject.GetComponent<BoxCollider>();
		DebugUtils.Assert(this.m_BoxCollider, "[ConstructionGhost::Start] Can't find box collider!", true, DebugUtils.AssertType.Info);
		Physics.IgnoreCollision(Player.Get().m_Collider, this.m_BoxCollider);
		if (this.m_State == ConstructionGhost.GhostState.None)
		{
			this.SetState(ConstructionGhost.GhostState.Building);
		}
		this.m_ItemLayer = LayerMask.NameToLayer("Item");
		this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("NoCollisionWithPlayer"));
		this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("SmallPlant"));
		this.m_LayerMasksToIgnore.Add(this.m_ItemLayer);
		if (this.m_IgnoreTerrain)
		{
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("Terrain"));
		}
		this.m_ShaderOfCollidingPlant = Shader.Find("Shader Forge/select_yellow_shader");
		this.SetupAudio();
		this.m_ShaderPropertyFresnelColor = Shader.PropertyToID("_fresnelcolor");
		this.m_AvailableCol = Color.white;
		this.m_NotAvailableHardCol = new Color(0.854f, 0.149f, 0.149f, 0.627f);
		this.m_NotAvailableSoftCol = Color.yellow;
		this.m_AvailableColorFresnel = new Color(1f, 1f, 1f, 1f);
		this.m_NotAvailableHardColorFresnel = new Color(1f, 0.196f, 0.196f, 0f);
		this.m_NotAvailableSoftColorFresnel = new Color(1f, 0.92f, 0.016f, 1f);
	}

	private void SetupAudio()
	{
		this.m_DeconsructSounds.Add("Sounds/Constructions/construction_deconstruct_crash_01");
		this.m_DeconsructSounds.Add("Sounds/Constructions/construction_deconstruct_crash_02");
		this.m_DeconsructSounds.Add("Sounds/Constructions/construction_deconstruct_crash_04");
		this.m_DeconsructSounds.Add("Sounds/Constructions/construction_deconstruct_crash_05");
		this.m_DeconsructSounds.Add("Sounds/Constructions/construction_deconstruct_crash_06");
		this.m_InsertItemClipsDict[2] = new List<AudioClip>();
		AudioClip item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_log_01");
		this.m_InsertItemClipsDict[2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_log_02");
		this.m_InsertItemClipsDict[2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_log_04");
		this.m_InsertItemClipsDict[2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_log_05");
		this.m_InsertItemClipsDict[2].Add(item);
		this.m_InsertItemClipsDict[1] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_long_stick_01");
		this.m_InsertItemClipsDict[1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_long_stick_02");
		this.m_InsertItemClipsDict[1].Add(item);
		this.m_InsertItemClipsDict[314] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_rope_01");
		this.m_InsertItemClipsDict[314].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_rope_02");
		this.m_InsertItemClipsDict[314].Add(item);
		this.m_InsertItemClipsDict[5] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_branches_01");
		this.m_InsertItemClipsDict[5].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_branches_02");
		this.m_InsertItemClipsDict[5].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_branches_03");
		this.m_InsertItemClipsDict[5].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_branches_04");
		this.m_InsertItemClipsDict[5].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_branches_05");
		this.m_InsertItemClipsDict[5].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_branches_06");
		this.m_InsertItemClipsDict[5].Add(item);
		this.m_InsertItemClipsDict[637] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Items/mud_insert");
		this.m_InsertItemClipsDict[637].Add(item);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		int i = 0;
		while (i < this.m_CollidingPlants.Count)
		{
			this.RemoveCollidingPlant(this.m_CollidingPlants.Keys.ElementAt(i));
		}
	}

	public void RegisterObserver(IGhostObserver observer)
	{
		this.m_Observers.Add(observer);
	}

	public void Reset()
	{
		foreach (GhostStep ghostStep in this.m_Steps)
		{
			foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
			{
				ghostSlot.Reset();
			}
		}
		this.m_CurrentStep = 0;
		this.SetState(ConstructionGhost.GhostState.Building);
	}

	public ConstructionGhost.GhostState GetState()
	{
		return this.m_State;
	}

	public void SetState(ConstructionGhost.GhostState state)
	{
		this.m_State = state;
		this.OnEnterState();
	}

	private void OnEnterState()
	{
		switch (this.m_State)
		{
		case ConstructionGhost.GhostState.Dragging:
			this.UpdateState();
			return;
		case ConstructionGhost.GhostState.Building:
			if (this.IsOnUpperLevel())
			{
				this.m_UpperLevel = true;
			}
			foreach (GhostStep ghostStep in this.m_Steps)
			{
				foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
				{
					ghostSlot.Init();
				}
			}
			this.SetupCurrentStep();
			HUDGather.Get().Setup();
			return;
		case ConstructionGhost.GhostState.Ready:
			this.CreateConstruction();
			HUDGather.Get().Setup();
			return;
		default:
			return;
		}
	}

	private void SetupCurrentStep()
	{
		if (this.m_CurrentStep >= this.m_Steps.Count)
		{
			return;
		}
		foreach (GhostSlot ghostSlot in this.m_Steps[this.m_CurrentStep].m_Slots)
		{
			ghostSlot.Activate();
		}
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateState();
		if (this.m_DebugBuild)
		{
			this.SetState(ConstructionGhost.GhostState.Ready);
			this.m_DebugBuild = false;
		}
	}

	public void UpdateState()
	{
		ConstructionGhost.GhostState state = this.m_State;
		if (state == ConstructionGhost.GhostState.Dragging)
		{
			this.UpdateRotation();
			this.UpdateTransform();
			this.UpdateColor();
			this.UpdateShaderProps();
			return;
		}
		if (state != ConstructionGhost.GhostState.Building)
		{
			return;
		}
		this.UpdateShaderProps();
		if (this.IsReady())
		{
			this.SetState(ConstructionGhost.GhostState.Ready);
		}
	}

	private void FixedUpdate()
	{
		if (this.m_State == ConstructionGhost.GhostState.Dragging)
		{
			this.UpdateProhibitionType();
		}
	}

	private bool IsInAir(Vector3 pos, Vector3 corner)
	{
		DebugRender.DrawLine(pos, corner, Color.blue, 0f);
		if (MainLevel.GetTerrainY(corner) >= corner.y)
		{
			return false;
		}
		float maxDistance = Mathf.Abs(pos.y - corner.y);
		int num = Physics.RaycastNonAlloc(pos, Vector3.down, ConstructionGhost.s_RaycastHitsTmp, maxDistance);
		for (int i = 0; i < num; i++)
		{
			if (!(ConstructionGhost.s_RaycastHitsTmp[i].collider == this.m_BoxCollider))
			{
				if (ConstructionGhost.s_RaycastHitsTmp[i].collider.gameObject == Terrain.activeTerrain.gameObject)
				{
					return false;
				}
				if (ConstructionGhost.s_RaycastHitsTmp[i].collider.gameObject.isStatic)
				{
					return false;
				}
				if (!ConstructionGhost.s_RaycastHitsTmp[i].collider.isTrigger)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool IsCornerInAir(Vector3 pos, Vector3 corner)
	{
		DebugRender.DrawLine(pos, corner, Color.blue, 0f);
		if (MainLevel.GetTerrainY(corner) >= corner.y)
		{
			return false;
		}
		float maxDistance = Mathf.Abs(pos.y - corner.y);
		int num = Physics.RaycastNonAlloc(pos, Vector3.down, ConstructionGhost.s_RaycastHitsTmp, maxDistance);
		for (int i = 0; i < num; i++)
		{
			if (!(ConstructionGhost.s_RaycastHitsTmp[i].collider == this.m_BoxCollider) && !ConstructionGhost.s_RaycastHitsTmp[i].collider.isTrigger)
			{
				return false;
			}
		}
		return true;
	}

	public void UpdateProhibitionType()
	{
		if (!this.m_CanBePlacedOnTopOfConstruction && this.m_OnTopOfConstruction)
		{
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
			return;
		}
		if (this.m_AdditionalPlacingConditions.Contains(ConstructionGhost.GhostPlacingCondition.IsSnapped) && !this.m_ConstructionToAttachTo)
		{
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
			return;
		}
		if (this.m_SelectedSlot)
		{
			foreach (ConstructionSlot constructionSlot in this.m_SelectedSlot.gameObject.GetComponents<ConstructionSlot>())
			{
				if (constructionSlot.m_Construction && (!constructionSlot.m_Construction.m_Info.IsWall() || this.m_ResultInfo.IsWall()) && (constructionSlot.m_Construction.m_Info.IsWall() || !this.m_ResultInfo.IsWall()))
				{
					this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
					return;
				}
			}
		}
		if (ItemInfo.IsStoneRing(this.m_ResultItemID) && this.m_Firecamp && this.m_Firecamp.m_StoneRing)
		{
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
			return;
		}
		if (this.m_Corners != null)
		{
			foreach (BoxCollider boxCollider in this.m_Corners)
			{
				bool enabled = boxCollider.enabled;
				boxCollider.enabled = true;
				bool flag = this.IsInAir(boxCollider.bounds.center, boxCollider.bounds.min);
				boxCollider.enabled = enabled;
				if (flag)
				{
					this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
					return;
				}
			}
		}
		if (this.IsOnUpperLevel() && this.m_UpperLevelCorners != null)
		{
			foreach (BoxCollider boxCollider2 in this.m_UpperLevelCorners)
			{
				bool enabled2 = boxCollider2.enabled;
				boxCollider2.enabled = true;
				Vector3 center = boxCollider2.bounds.center;
				center.y += boxCollider2.bounds.max.y;
				bool flag2 = this.IsCornerInAir(center, boxCollider2.bounds.min);
				boxCollider2.enabled = enabled2;
				if (flag2)
				{
					this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
					return;
				}
			}
		}
		this.m_ProhibitionType = ConstructionGhost.ProhibitionType.None;
		if (this.m_AllignToTerrain && !this.m_Smoker && this.m_FirecampRacks.Count == 0 && Vector3.Angle(base.transform.up, Vector3.up) > this.m_MaxAllignAngle)
		{
			this.ClearCollidingPlants();
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
			return;
		}
		Vector3 center2 = this.m_BoxCollider.bounds.center;
		Vector3 vector = this.m_BoxCollider.size * 0.5f;
		center2.y += this.m_ColliderShrinkBottom * 0.5f;
		vector.y -= this.m_ColliderShrinkBottom;
		vector.y = Mathf.Max(vector.y, 0f);
		this.m_Colliders.Clear();
		if (this.m_TestColliders != null)
		{
			foreach (BoxCollider boxCollider3 in this.m_TestColliders)
			{
				int num = Physics.OverlapBoxNonAlloc(boxCollider3.bounds.center, boxCollider3.size * 0.5f, ConstructionGhost.s_CollidersTemp, boxCollider3.transform.rotation);
				for (int j = 0; j < num; j++)
				{
					Collider collider = ConstructionGhost.s_CollidersTemp[j];
					if (!this.m_Colliders.Contains(collider) && !this.m_TestColliders.Contains(collider))
					{
						if (collider.isTrigger && !collider.gameObject.GetComponent<ConstructionGhost>())
						{
							UnityEngine.Object component = collider.gameObject.GetComponent<Spikes>();
							Acre exists = collider.gameObject.transform.parent ? collider.gameObject.transform.parent.GetComponent<Acre>() : null;
							FramePlacingObstacle component2 = collider.gameObject.GetComponent<FramePlacingObstacle>();
							if (!component && !exists && !component2)
							{
								goto IL_458;
							}
						}
						if (!(collider.gameObject == Terrain.activeTerrain.gameObject) && !(collider.transform.parent == base.transform))
						{
							this.m_Colliders.Add(collider);
						}
					}
					IL_458:;
				}
			}
		}
		else
		{
			int num2 = Physics.OverlapBoxNonAlloc(center2, vector, ConstructionGhost.s_CollidersTemp, this.m_BoxCollider.transform.rotation);
			this.m_Colliders.Resize(num2);
			for (int k = 0; k < num2; k++)
			{
				this.m_Colliders[k] = ConstructionGhost.s_CollidersTemp[k];
			}
		}
		this.m_Colliders.Remove(Player.Get().m_Collider);
		this.m_Colliders.Remove(this.m_BoxCollider);
		if (this.m_AdditionalPlacingConditions.Contains(ConstructionGhost.GhostPlacingCondition.NeedFirecamp) && this.m_Firecamp)
		{
			this.m_Colliders.Remove(this.m_Firecamp.m_Collider);
			if (this.m_Firecamp.m_StoneRing)
			{
				this.m_Colliders.Remove(this.m_Firecamp.m_StoneRing.m_Collider);
			}
		}
		if ((ItemInfo.IsFirecamp(this.m_ResultItemID) || ItemInfo.IsStoneRing(this.m_ResultItemID)) && this.m_FirecampRacks.Count > 0)
		{
			foreach (FirecampRack firecampRack in this.m_FirecampRacks)
			{
				this.m_Colliders.Remove(firecampRack.m_Collider);
			}
		}
		if ((ItemInfo.IsFirecamp(this.m_ResultItemID) || ItemInfo.IsStoneRing(this.m_ResultItemID)) && this.m_Smoker)
		{
			this.m_Colliders.Remove(this.m_Smoker.m_Collider);
		}
		if (this.m_ResultItemID != ItemID.Stick_Fish_Trap && this.m_ResultItemID != ItemID.Big_Stick_Fish_Trap)
		{
			foreach (Collider collider2 in this.m_Colliders)
			{
				Item component3 = collider2.gameObject.GetComponent<Item>();
				if (!(component3 != null) || component3.m_Info == null || component3.m_Info.IsConstruction() || component3.m_IsPlant || component3.m_IsTree)
				{
					if (collider2.isTrigger)
					{
						if ((this.m_ResultItemID == ItemID.building_frame || this.m_ResultItemID == ItemID.building_bamboo_frame) && collider2.gameObject.GetComponent<FramePlacingObstacle>())
						{
							this.ClearCollidingPlants();
							this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
							return;
						}
						UnityEngine.Object component4 = collider2.gameObject.GetComponent<Spikes>();
						Acre exists2 = collider2.gameObject.transform.parent ? collider2.gameObject.transform.parent.GetComponent<Acre>() : null;
						if (component4 || exists2)
						{
							this.ClearCollidingPlants();
							this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
							return;
						}
						ConstructionGhost component5 = collider2.gameObject.GetComponent<ConstructionGhost>();
						if (!(component5 != null))
						{
							continue;
						}
						if (component5.m_ResultItemID == ItemID.mud_mixer)
						{
							this.ClearCollidingPlants();
							this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
							return;
						}
						using (List<GhostStep>.Enumerator enumerator4 = component5.m_Steps.GetEnumerator())
						{
							while (enumerator4.MoveNext())
							{
								GhostStep ghostStep = enumerator4.Current;
								foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
								{
									if (base.m_Collider.bounds.Intersects(ghostSlot.m_Collider.bounds))
									{
										this.ClearCollidingPlants();
										this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
										return;
									}
								}
							}
							continue;
						}
					}
					if (!this.m_ConstructionToAttachTo || (!(collider2.gameObject == this.m_ConstructionToAttachTo.gameObject) && !(collider2.gameObject == Terrain.activeTerrain.gameObject)))
					{
						ConstructionGhost component5 = collider2.gameObject.GetComponent<ConstructionGhost>();
						if (component5 != null)
						{
							foreach (GhostStep ghostStep2 in component5.m_Steps)
							{
								foreach (GhostSlot ghostSlot2 in ghostStep2.m_Slots)
								{
									if (base.m_Collider.bounds.Intersects(ghostSlot2.m_Collider.bounds))
									{
										this.ClearCollidingPlants();
										this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
										return;
									}
								}
							}
						}
						Construction component6 = collider2.gameObject.GetComponent<Construction>();
						if (!component6 && collider2.gameObject.transform.parent)
						{
							component6 = collider2.gameObject.transform.parent.GetComponent<Construction>();
						}
						if (component6)
						{
							if (!(component6 == this.m_ConstructionToAttachTo) && (!this.m_ConstructionToAttachTo || !this.m_ConstructionToAttachTo.IsConstructionConnected(component6)))
							{
								this.ClearCollidingPlants();
								this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
								return;
							}
						}
						else if (!collider2.gameObject.IsWater() && !this.m_LayerMasksToIgnore.Contains(collider2.gameObject.layer) && !collider2.isTrigger)
						{
							this.ClearCollidingPlants();
							this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
							return;
						}
					}
				}
			}
		}
		Collider collider3 = null;
		foreach (Collider collider4 in this.m_Colliders)
		{
			if (collider4.gameObject.IsWater())
			{
				collider3 = collider4;
				break;
			}
		}
		if (this.m_AdditionalPlacingConditions.Contains(ConstructionGhost.GhostPlacingCondition.MustBeInWater) && !collider3)
		{
			if (this.m_Hook)
			{
				center2 = this.m_Hook.bounds.center;
				vector = this.m_Hook.size * 0.5f;
				int num3 = Physics.OverlapBoxNonAlloc(center2, vector, ConstructionGhost.s_CollidersTemp, this.m_Hook.transform.rotation);
				for (int l = 0; l < num3; l++)
				{
					if (ConstructionGhost.s_CollidersTemp[l].gameObject.IsWater())
					{
						return;
					}
				}
			}
			this.ClearCollidingPlants();
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
			return;
		}
		if (this.m_AdditionalPlacingConditions.Contains(ConstructionGhost.GhostPlacingCondition.MustBeInWater) && collider3 && this.m_Hook)
		{
			this.ClearCollidingPlants();
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
			return;
		}
		if ((this.m_AdditionalPlacingConditions.Contains(ConstructionGhost.GhostPlacingCondition.CantBeInWater) || this.m_AdditionalPlacingConditions.Contains(ConstructionGhost.GhostPlacingCondition.NeedFirecamp)) && collider3)
		{
			this.ClearCollidingPlants();
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
			return;
		}
		if (collider3 && this.m_MaxDepthOfWater > 0f && collider3.bounds.max.y - this.m_BoxCollider.bounds.min.y > this.m_MaxDepthOfWater)
		{
			this.ClearCollidingPlants();
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Depth;
			return;
		}
		if (this.m_AdditionalPlacingConditions.Contains(ConstructionGhost.GhostPlacingCondition.Whatever) || this.m_AdditionalPlacingConditions.Contains(ConstructionGhost.GhostPlacingCondition.CantBeInWater))
		{
			List<GameObject> list = new List<GameObject>();
			foreach (Collider collider5 in this.m_Colliders)
			{
				if (this.m_LayerMasksToIgnore.Contains(collider5.gameObject.layer) && collider5.gameObject.layer != this.m_ItemLayer)
				{
					if (!this.m_CollidingPlants.ContainsKey(collider5.gameObject))
					{
						this.AddCollidingPlant(collider5.gameObject);
					}
					list.Add(collider5.gameObject);
				}
			}
			int m = 0;
			while (m < this.m_CollidingPlants.Keys.Count)
			{
				if (!list.Contains(this.m_CollidingPlants.ElementAt(m).Key))
				{
					this.RemoveCollidingPlant(this.m_CollidingPlants.ElementAt(m).Key);
				}
				else
				{
					m++;
				}
			}
		}
		if (this.m_CollidingPlants.Count > 0)
		{
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Soft;
			return;
		}
	}

	private void ClearCollidingPlants()
	{
		int i = 0;
		while (i < this.m_CollidingPlants.Keys.Count)
		{
			this.RemoveCollidingPlant(this.m_CollidingPlants.ElementAt(i).Key);
		}
	}

	private void UpdateRotation()
	{
		if (!this.m_Rotate && InputsManager.Get().IsActionActive(InputsManager.InputAction.ConstructionRotate))
		{
			this.m_Rotate = true;
			this.m_Rotation = 0f;
			Player.Get().BlockRotation();
		}
		else if (this.m_Rotate && !InputsManager.Get().IsActionActive(InputsManager.InputAction.ConstructionRotate))
		{
			this.m_Rotate = false;
			Player.Get().UnblockRotation();
		}
		if (this.m_Rotate)
		{
			float num = Mathf.Clamp(InputHelpers.GetLookInput(1f, 1f, 150f).x, -this.m_MaxRot, this.m_MaxRot) * this.m_RotationSpeed * Time.deltaTime;
			this.m_Rotation -= num;
			this.m_RotationSnap = (int)(this.m_Rotation / 10f);
		}
	}

	private void UpdateColor()
	{
		Color color = this.m_AvailableCol;
		Color color_fresnel = this.m_AvailableColorFresnel;
		if (this.m_ProhibitionType == ConstructionGhost.ProhibitionType.Hard || this.m_ProhibitionType == ConstructionGhost.ProhibitionType.Depth)
		{
			color = this.m_NotAvailableHardCol;
			color_fresnel = this.m_NotAvailableHardColorFresnel;
		}
		else if (this.m_ProhibitionType == ConstructionGhost.ProhibitionType.Soft)
		{
			color = this.m_NotAvailableSoftCol;
			color_fresnel = this.m_NotAvailableSoftColorFresnel;
		}
		this.SetColor(color, color_fresnel);
	}

	private void UpdateShaderProps()
	{
		Component[] componentsInChildren = base.gameObject.GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (Material material in ((Renderer)componentsInChildren[i]).materials)
			{
				material.SetVector(this.m_ShaderObjectWorldPos, base.transform.position);
				if (this.IsOnUpperLevel())
				{
					material.SetFloat(this.m_ShaderCutLevel, this.m_UpperLevelCutLevel);
				}
				else
				{
					material.SetFloat(this.m_ShaderCutLevel, this.m_LowerLevelCutLevel);
				}
			}
		}
	}

	private bool IsOnUpperLevel()
	{
		ConstructionSlot selectedSlot = this.m_SelectedSlot;
		return this.m_ConstructionBelow || (selectedSlot && selectedSlot.m_UpperLevelSlot) || (selectedSlot && selectedSlot.m_ParentConstruction.m_Level > 0) || this.m_UpperLevel;
	}

	private void UpdateTransform()
	{
		this.m_SelectedSlot = null;
		this.m_OnTopOfConstruction = false;
		this.m_ConstructionBelow = null;
		if (this.UpdateAttaching())
		{
			return;
		}
		this.m_ConstructionToAttachTo = null;
		base.gameObject.transform.rotation = Player.Get().transform.rotation;
		base.gameObject.transform.Rotate(Vector3.up, this.m_Rotation);
		bool enabled = this.m_BoxCollider.enabled;
		this.m_BoxCollider.enabled = false;
		Vector3 position = Player.Get().transform.position;
		Vector3 normalized2D = Player.Get().transform.forward.GetNormalized2D();
		Vector3 vector = position + normalized2D * CJTools.Math.GetProportionalClamp(this.m_PositionOffsetMin, this.m_PositionOffsetMax, Camera.main.transform.forward.y, -0.5f, 0f);
		vector.y += 0.2f;
		float terrainY = MainLevel.GetTerrainY(vector);
		if (terrainY >= vector.y)
		{
			vector.y = terrainY;
		}
		else
		{
			float num = float.MinValue;
			bool onTopOfConstruction = false;
			int num2 = Physics.RaycastNonAlloc(vector, -Vector3.up, ConstructionGhost.s_RaycastHitsTmp);
			for (int i = 0; i < num2; i++)
			{
				RaycastHit raycastHit = ConstructionGhost.s_RaycastHitsTmp[i];
				if (!(raycastHit.collider.gameObject.GetComponent<WaterCollider>() != null))
				{
					Item item = null;
					if (raycastHit.collider.GetType() != typeof(TerrainCollider))
					{
						item = raycastHit.collider.gameObject.GetComponent<Item>();
						if (!item || !item.m_Info.m_CanPlaceGhostOnTop)
						{
							goto IL_212;
						}
					}
					if (raycastHit.point.y > num)
					{
						onTopOfConstruction = (item && item.m_Info.m_CanPlaceGhostOnTop);
						if (item && item.m_Info.IsConstruction())
						{
							this.m_ConstructionBelow = (Construction)item;
						}
						vector = raycastHit.point;
						num = raycastHit.point.y;
					}
				}
				IL_212:;
			}
			this.m_OnTopOfConstruction = onTopOfConstruction;
		}
		float num3 = vector.y;
		this.m_BoxCollider.enabled = enabled;
		num3 -= this.m_BoxCollider.bounds.min.y - base.gameObject.transform.position.y;
		if (this.m_ResultItemID == ItemID.Small_Fire || this.m_ResultItemID == ItemID.Stone_Ring || this.m_ResultItemID == ItemID.Campfire || this.m_ResultItemID == ItemID.Campfire_Rack || this.m_ResultItemID == ItemID.Dryer || this.m_ResultItemID == ItemID.Bamboo_Dryer || this.m_ResultItemID == ItemID.mud_charcoal_furnace || this.m_ResultItemID == ItemID.mud_mixer || this.m_ResultItemID == ItemID.Weapon_Rack || this.m_ResultItemID == ItemID.Acre || this.m_ResultItemID == ItemID.Acre_Small || this.m_ResultItemID == ItemID.Logs_Bed)
		{
			num3 += 0.1f;
		}
		vector.y = num3;
		base.gameObject.transform.position = vector;
		if (this.UpdateAttachingToSlot())
		{
			return;
		}
		this.AllignToTerrain();
		this.UpdateSnapToFirecamp();
		this.UpdateSnapToRack();
		this.UpdateSnapToSmoker();
	}

	private void AllignToTerrain()
	{
		if (!this.m_AllignToTerrain || this.m_OnTopOfConstruction)
		{
			return;
		}
		bool enabled = this.m_BoxCollider.enabled;
		this.m_BoxCollider.enabled = false;
		RaycastHit raycastHit = default(RaycastHit);
		Vector3 a = base.transform.forward.GetNormalized2D() * 0.5f;
		Vector3 vector = this.m_BoxCollider.bounds.center + a * this.m_BoxCollider.size.z + Vector3.up * this.m_BoxCollider.size.y;
		this.GetRaycastHit(vector, ref raycastHit);
		vector = raycastHit.point;
		Vector3 vector2 = this.m_BoxCollider.bounds.center - a * this.m_BoxCollider.size.z + Vector3.up * this.m_BoxCollider.size.y;
		this.GetRaycastHit(vector2, ref raycastHit);
		vector2 = raycastHit.point;
		Vector3 vector3 = vector - vector2;
		Vector3 a2 = base.transform.right.GetNormalized2D() * 0.5f;
		Vector3 vector4 = Vector3.zero;
		Vector3 vector5 = this.m_BoxCollider.bounds.center + a2 * this.m_BoxCollider.size.x + Vector3.up * this.m_BoxCollider.size.y;
		this.GetRaycastHit(vector5, ref raycastHit);
		vector5 = raycastHit.point;
		Vector3 vector6 = this.m_BoxCollider.bounds.center - a2 * this.m_BoxCollider.size.x + Vector3.up * this.m_BoxCollider.size.y;
		this.GetRaycastHit(vector6, ref raycastHit);
		vector6 = raycastHit.point;
		Vector3 vector7 = vector5 - vector6;
		vector4 = Vector3.Cross(vector3.normalized, vector7.normalized);
		if (!vector4.IsZero())
		{
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(vector3.normalized, vector4.normalized), 1f);
		}
		Vector3 position = base.transform.position;
		this.GetRaycastHit(position + Vector3.up * this.m_BoxCollider.size.y, ref raycastHit);
		position.y = Mathf.Max(raycastHit.point.y, MainLevel.GetTerrainY(position)) + (this.m_BoxCollider.size.y * 0.5f - this.m_BoxCollider.center.y) + 0.1f;
		base.transform.position = position;
		this.m_BoxCollider.enabled = enabled;
	}

	private int CompareListByDist(RaycastHit i1, RaycastHit i2)
	{
		float num = Vector3.Distance(this.m_RaycastOrig, i1.point);
		float num2 = Vector3.Distance(this.m_RaycastOrig, i2.point);
		if (num > num2)
		{
			return 1;
		}
		if (num < num2)
		{
			return -1;
		}
		return 0;
	}

	private void SortHitsByDist(RaycastHit[] hits, int hits_cnt)
	{
		ConstructionGhost.s_CompareHitsByDist.m_Parent = this;
		Array.Sort<RaycastHit>(ConstructionGhost.s_RaycastHitsTmp, 0, hits_cnt, ConstructionGhost.s_CompareHitsByDist);
	}

	private void GetRaycastHit(Vector3 pos, ref RaycastHit hit)
	{
		pos.y = Mathf.Max(pos.y, MainLevel.GetTerrainY(pos) + 0.1f);
		int hits_cnt = Physics.RaycastNonAlloc(pos, -Vector3.up, ConstructionGhost.s_RaycastHitsTmp);
		this.m_RaycastOrig = pos;
		this.SortHitsByDist(ConstructionGhost.s_RaycastHitsTmp, hits_cnt);
		foreach (RaycastHit raycastHit in ConstructionGhost.s_RaycastHitsTmp)
		{
			if (!(raycastHit.collider.gameObject == base.gameObject) && !raycastHit.collider.isTrigger && !this.m_LayerMasksToIgnore.Contains(raycastHit.collider.gameObject.layer))
			{
				hit = raycastHit;
				return;
			}
		}
	}

	private bool UpdateSnapToFirecamp()
	{
		if (this.m_PlacingCondition != ConstructionGhost.GhostPlacingCondition.NeedFirecamp)
		{
			return false;
		}
		this.m_Firecamp = null;
		float num = float.MaxValue;
		Firecamp firecamp = null;
		foreach (Firecamp firecamp2 in Firecamp.s_Firecamps)
		{
			if (!ItemInfo.IsStoneRing(this.m_ResultItemID) || firecamp2.GetInfoID() != ItemID.Campfire)
			{
				float num2 = Vector3.Distance(base.transform.position, firecamp2.transform.position);
				if (num2 < num)
				{
					firecamp = firecamp2;
					num = num2;
				}
			}
		}
		if (!firecamp)
		{
			return false;
		}
		if (num < this.m_FirecampSnapDist)
		{
			Vector3 position = firecamp.transform.position;
			position.y -= this.m_BoxCollider.bounds.min.y - base.gameObject.transform.position.y;
			base.transform.position = position;
			this.m_Firecamp = firecamp;
			this.AllignToTerrain();
		}
		return this.m_Firecamp != null;
	}

	private bool UpdateSnapToRack()
	{
		if ((!ItemInfo.IsFirecamp(this.m_ResultItemID) || this.m_ResultItemID == ItemID.Campfire) && !ItemInfo.IsStoneRing(this.m_ResultItemID))
		{
			return false;
		}
		this.m_FirecampRacks.Clear();
		foreach (FirecampRack firecampRack in FirecampRack.s_FirecampRacks)
		{
			if ((!ItemInfo.IsStoneRing(this.m_ResultItemID) || !ItemInfo.IsStoneRing(firecampRack.GetInfoID())) && Vector3.Distance(base.transform.position, firecampRack.transform.position) < this.m_FirecampSnapDist)
			{
				this.m_FirecampRacks.Add(firecampRack);
			}
		}
		if (this.m_FirecampRacks.Count > 0)
		{
			if (this.m_FirecampRacks[0].m_FirecampDummy)
			{
				base.transform.rotation = this.m_FirecampRacks[0].m_FirecampDummy.rotation;
				base.transform.position = this.m_FirecampRacks[0].m_FirecampDummy.position;
			}
			else
			{
				Vector3 position = this.m_FirecampRacks[0].transform.position;
				position.y -= this.m_BoxCollider.bounds.min.y - base.gameObject.transform.position.y;
				base.transform.position = position;
				this.AllignToTerrain();
			}
			return true;
		}
		return false;
	}

	private bool UpdateSnapToSmoker()
	{
		if (!ItemInfo.IsFirecamp(this.m_ResultItemID) && !ItemInfo.IsStoneRing(this.m_ResultItemID))
		{
			return false;
		}
		this.m_Smoker = null;
		float num = float.MaxValue;
		Construction construction = null;
		for (int i = 0; i < Construction.s_EnabledConstructions.Count; i++)
		{
			if (ItemInfo.IsSmoker(Construction.s_EnabledConstructions[i].GetInfoID()))
			{
				float num2 = Vector3.Distance(base.transform.position, Construction.s_EnabledConstructions[i].transform.position);
				if (num2 < num)
				{
					construction = Construction.s_EnabledConstructions[i];
					num = num2;
				}
			}
		}
		if (!construction)
		{
			return false;
		}
		if (num < this.m_SmokerSnapDist)
		{
			Vector3 position = construction.transform.position;
			position.y -= this.m_BoxCollider.bounds.min.y - base.gameObject.transform.position.y;
			base.transform.position = position;
			this.m_Smoker = construction;
			this.AllignToTerrain();
		}
		return this.m_Smoker != null;
	}

	private bool UpdateAttaching()
	{
		if (!Camera.main)
		{
			return false;
		}
		Construction construction = null;
		float num = float.MaxValue;
		Vector3 forward = Camera.main.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		Quaternion rotation = Quaternion.identity;
		Vector3 position = Vector3.zero;
		int i = 0;
		while (i < Construction.s_EnabledConstructions.Count)
		{
			Construction construction2 = Construction.s_EnabledConstructions[i];
			if (!(construction2 == null))
			{
				goto IL_94;
			}
			if (Construction.s_EnabledConstructions[i])
			{
				construction2 = Construction.s_EnabledConstructions[i];
			}
			if (!(construction2 == null))
			{
				goto IL_94;
			}
			IL_1C9:
			i++;
			continue;
			IL_94:
			Vector3 vector = construction2.gameObject.transform.position;
			if ((vector - Player.Get().transform.position).magnitude > this.m_PositionOffsetMax)
			{
				goto IL_1C9;
			}
			IPlaceToAttach component = construction2.gameObject.GetComponent<IPlaceToAttach>();
			if (component != null)
			{
				List<Transform> placesToAttach = component.GetPlacesToAttach();
				for (int j = 0; j < placesToAttach.Count; j++)
				{
					Transform transform = placesToAttach[j];
					if (this.m_ResultInfo != null)
					{
						for (int k = 0; k < this.m_ResultInfo.m_PlaceToAttachToNames.Count; k++)
						{
							if (transform.name == this.m_ResultInfo.m_PlaceToAttachToNames[k])
							{
								vector = transform.position;
								vector -= Player.Get().transform.position;
								vector.y = 0f;
								vector.Normalize();
								float num2 = Vector3.Dot(forward, vector);
								if (num2 < num && num2 > 0.95f)
								{
									construction = construction2;
									num = num2;
									position = transform.position;
									rotation = transform.rotation;
								}
							}
						}
					}
				}
				goto IL_1C9;
			}
			goto IL_1C9;
		}
		if (construction != null)
		{
			base.gameObject.transform.rotation = rotation;
			base.gameObject.transform.position = position;
			this.m_ConstructionToAttachTo = construction;
			this.AllignToTerrain();
			foreach (FirecampRack firecampRack in FirecampRack.s_FirecampRacks)
			{
				if (construction == firecampRack)
				{
					this.m_FirecampRacks.Add(firecampRack);
				}
			}
			return true;
		}
		return false;
	}

	private bool UpdateAttachingToSlot()
	{
		if (!Camera.main)
		{
			return false;
		}
		ConstructionSlot constructionSlot = null;
		Construction construction = null;
		Quaternion rotation = Quaternion.identity;
		Vector3 position = Vector3.zero;
		Camera mainCamera = CameraManager.Get().m_MainCamera;
		if (!mainCamera)
		{
			return false;
		}
		Vector3 position2 = mainCamera.transform.position;
		Vector3 forward = mainCamera.transform.forward;
		this.m_AttachmentData.Clear();
		float num = 1.5f;
		int i = 0;
		while (i < Construction.s_EnabledConstructions.Count)
		{
			Construction construction2 = Construction.s_EnabledConstructions[i];
			if (!(construction2 == null))
			{
				goto IL_B1;
			}
			if (Construction.s_EnabledConstructions[i])
			{
				construction2 = Construction.s_EnabledConstructions[i];
			}
			if (!(construction2 == null))
			{
				goto IL_B1;
			}
			IL_1A4:
			i++;
			continue;
			IL_B1:
			foreach (ConstructionSlot constructionSlot2 in construction2.m_ConstructionSlots)
			{
				if (!constructionSlot2.m_Construction && constructionSlot2.m_MatchingItemIDs.Contains(this.m_ResultItemID))
				{
					Vector3 position3 = base.transform.position;
					position3.y = constructionSlot2.transform.position.y;
					Vector3 vector = constructionSlot2.transform.position - position2;
					float num2 = constructionSlot2.transform.position.Distance(position3);
					if (num2 <= num)
					{
						ConstructionSlotData item;
						item.con = construction2;
						item.slot = constructionSlot2;
						item.dot = Vector3.Dot(forward, vector.normalized);
						item.dist = ((num2 > 0.01f) ? num2 : 0.01f);
						this.m_AttachmentData.Add(item);
					}
				}
			}
			goto IL_1A4;
		}
		if (this.m_AttachmentData.Count > 0)
		{
			this.m_AttachmentData.Sort(ConstructionGhost.s_DotComparer);
			ConstructionSlotData constructionSlotData = this.m_AttachmentData[0];
			construction = constructionSlotData.con;
			position = constructionSlotData.slot.transform.position;
			rotation = constructionSlotData.slot.transform.rotation;
			constructionSlot = constructionSlotData.slot;
		}
		if (construction != null && constructionSlot.m_ChangeToID != ItemID.None && constructionSlot.m_ChangeToID != this.m_ResultItemID)
		{
			ConstructionController.Get().ReplaceGhost(constructionSlot.m_ChangeTo + "Ghost");
		}
		if (construction != null && construction.m_Level >= 2 && constructionSlot.m_UpperLevelSlot)
		{
			return false;
		}
		if (construction != null)
		{
			if (this.m_CanBeRotatedWhenSnapped)
			{
				base.gameObject.transform.rotation = rotation;
				Quaternion quaternion = base.gameObject.transform.localRotation;
				float num3 = (float)this.m_RotationSnap;
				if (this.m_SnappedRotationAngle == ConstructionGhost.SnappedRotationAngle.Angle90)
				{
					num3 *= 90f;
				}
				else if (this.m_SnappedRotationAngle == ConstructionGhost.SnappedRotationAngle.Angle180)
				{
					num3 *= 180f;
				}
				if (this.m_SnappedRotationAxis == ConstructionGhost.SnappedRotationAxis.X)
				{
					quaternion *= Quaternion.Euler(num3, 0f, 0f);
				}
				else if (this.m_SnappedRotationAxis == ConstructionGhost.SnappedRotationAxis.Y)
				{
					quaternion *= Quaternion.Euler(0f, num3, 0f);
				}
				else if (this.m_SnappedRotationAxis == ConstructionGhost.SnappedRotationAxis.Z)
				{
					quaternion *= Quaternion.Euler(0f, 0f, num3);
				}
				base.gameObject.transform.localRotation = quaternion;
			}
			else
			{
				base.gameObject.transform.rotation = rotation;
			}
			base.gameObject.transform.position = position;
			this.m_ConstructionToAttachTo = construction;
			this.m_SelectedSlot = constructionSlot;
			this.AllignToTerrain();
			foreach (FirecampRack firecampRack in FirecampRack.s_FirecampRacks)
			{
				if (construction == firecampRack)
				{
					this.m_FirecampRacks.Add(firecampRack);
				}
			}
			return true;
		}
		if (this.m_RestoreIfNotAttached)
		{
			ConstructionController.Get().RestoreGhost();
		}
		return false;
	}

	private void SetColor(Color color, Color color_fresnel)
	{
		if (this.m_Color == color && this.m_ColorFresnel == color_fresnel)
		{
			return;
		}
		this.m_Color = color;
		this.m_ColorFresnel = color_fresnel;
		Component[] componentsInChildren = base.gameObject.GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (Material material in ((Renderer)componentsInChildren[i]).materials)
			{
				material.color = this.m_Color;
				material.SetColor(this.m_ShaderPropertyFresnelColor, this.m_ColorFresnel);
			}
		}
	}

	public void OnGhostFulfill(ItemID item_id, bool from_save)
	{
		ItemInfo info = ItemsManager.Get().GetInfo(item_id);
		bool flag = true;
		using (List<GhostSlot>.Enumerator enumerator = this.m_Steps[this.m_CurrentStep].m_Slots.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (!enumerator.Current.m_Fulfilled)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			this.m_CurrentStep++;
			this.SetupCurrentStep();
		}
		if (!from_save)
		{
			this.PlayInsertSound(info);
		}
		if (this.ReplIsOwner())
		{
			this.ReplSetDirty();
		}
	}

	private void PlayInsertSound(ItemInfo info)
	{
		if (info != null)
		{
			List<AudioClip> list = null;
			if (this.m_InsertItemClipsDict.TryGetValue((int)info.m_ID, out list))
			{
				if (this.m_AudioSource == null)
				{
					this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
					this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
				}
				this.m_AudioSource.PlayOneShot(list[UnityEngine.Random.Range(0, list.Count)]);
				return;
			}
			if (this.m_AudioSource == null)
			{
				this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
				this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
			}
			if (this.m_InsertItemClipsDict.TryGetValue(314, out list))
			{
				if (this.m_AudioSource == null)
				{
					this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
					this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
				}
				this.m_AudioSource.PlayOneShot(list[UnityEngine.Random.Range(0, list.Count)]);
			}
		}
	}

	public bool IsReady()
	{
		return this.m_CurrentStep >= this.m_Steps.Count;
	}

	private void CreateConstruction()
	{
		Item item = null;
		item = ItemsManager.Get().CreateItem(this.m_ResultItemID, true, base.transform.position, base.transform.rotation);
		if (this.m_PlacingCondition == ConstructionGhost.GhostPlacingCondition.NeedFirecamp)
		{
			IFirecampAttach[] components = item.gameObject.GetComponents<IFirecampAttach>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].SetFirecamp(this.m_Firecamp);
			}
		}
		if (this.m_FirecampRacks.Count > 0 && ItemInfo.IsFirecamp(item.GetInfoID()))
		{
			foreach (FirecampRack firecampRack in this.m_FirecampRacks)
			{
				IFirecampAttach[] components = firecampRack.gameObject.GetComponents<IFirecampAttach>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].SetFirecamp((Firecamp)item);
				}
			}
		}
		if (this.m_Smoker && ItemInfo.IsFirecamp(item.GetInfoID()))
		{
			IFirecampAttach[] components = this.m_Smoker.gameObject.GetComponents<IFirecampAttach>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].SetFirecamp((Firecamp)item);
			}
		}
		if (this.m_ConstructionObjectName.Length > 0)
		{
			item.gameObject.name = this.m_ConstructionObjectName;
		}
		if (this.m_SelectedSlot)
		{
			this.m_SelectedSlot.SetConstruction((Construction)item);
		}
		ScenarioManager.Get().OnItemCreated(item.gameObject);
		EventsManager.OnEvent(Enums.Event.Build, 1, (int)this.m_ResultItemID);
		((HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages))).AddMessage(GreenHellGame.Instance.GetLocalization().Get(this.m_ResultItemID.ToString(), true) + " " + GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_Created", true), null, HUDMessageIcon.None, "", null);
		PlayerAudioModule.Get().PlayBuildCompletedSound();
		ItemsManager.Get().OnCreateItem(this.m_ResultItemID);
		if (item.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
		{
			item.SetLayer(item.transform, LayerMask.NameToLayer("Item"));
		}
		foreach (IGhostObserver ghostObserver in this.m_Observers)
		{
			ghostObserver.OnCreateConstruction(this, item);
		}
		HintsManager.Get().ShowHint("Destroy_Construction", 10f);
		foreach (string hint_name in this.m_CreateConstructionHints)
		{
			HintsManager.Get().ShowHint(hint_name, 10f);
		}
		if (this.m_DestroyOnCreateConstruction)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			base.gameObject.SetActive(false);
		}
		if (this.m_SelectedSlot && this.m_SelectedSlot.m_ParentConstruction.m_Level > 0)
		{
			bool set = this.m_SelectedSlot.m_ParentConstruction.m_Level > 0;
			((Construction)item).SetUpperLevel(set, this.m_SelectedSlot.m_UpperLevelSlot ? (this.m_SelectedSlot.m_ParentConstruction.m_Level + 1) : this.m_SelectedSlot.m_ParentConstruction.m_Level);
		}
		else if (this.m_SelectedSlot && this.m_SelectedSlot.m_UpperLevelSlot)
		{
			((Construction)item).SetUpperLevel(true, this.m_SelectedSlot.m_ParentConstruction.m_Level + 1);
		}
		else if (this.m_ConstructionBelow)
		{
			((Construction)item).SetUpperLevel(true, this.m_ConstructionBelow.m_Level + 1);
		}
		((Construction)item).m_ConstructionBelow = this.m_ConstructionBelow;
	}

	public virtual bool CanBePlaced()
	{
		return this.m_ProhibitionType == ConstructionGhost.ProhibitionType.None;
	}

	public void Deconstruct()
	{
		foreach (GhostStep ghostStep in this.m_Steps)
		{
			foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
			{
				ghostSlot.Deconstruct();
			}
		}
		GameObject gameObject = new GameObject();
		OneShotSoundObject oneShotSoundObject = gameObject.AddComponent<OneShotSoundObject>();
		gameObject.name = "OneShotSoundObject from Construction ghost";
		oneShotSoundObject.m_SoundNameWithPath = this.m_DeconsructSounds[UnityEngine.Random.Range(0, this.m_DeconsructSounds.Count)];
		oneShotSoundObject.gameObject.transform.position = base.transform.position;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Deconstruct)
		{
			this.Deconstruct();
		}
	}

	public override bool CheckDot()
	{
		return false;
	}

	public override bool OnlyInCrosshair()
	{
		return true;
	}

	public override bool CheckInsideCollider()
	{
		return true;
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !GreenHellGame.TWITCH_DEMO;
	}

	public override bool IsAdditionalTrigger()
	{
		return true;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		base.GetActions(actions);
		if (this.m_CanDeconstruct)
		{
			actions.Add(TriggerAction.TYPE.Deconstruct);
		}
	}

	public override string GetName()
	{
		return EnumUtils<ItemID>.GetName((int)this.m_ResultItemID);
	}

	private void AddCollidingPlant(GameObject obj)
	{
		foreach (Renderer renderer in General.GetComponentsDeepChild<Renderer>(obj))
		{
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				Material material = renderer.materials[j];
				Texture texture = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
				Texture texture2 = material.HasProperty("_BumpMap") ? material.GetTexture("_BumpMap") : null;
				Texture texture3 = material.HasProperty("_RoughTex") ? material.GetTexture("_RoughTex") : null;
				if (!this.m_CollidingPlants.ContainsKey(obj))
				{
					this.m_CollidingPlants[obj] = new List<Shader>();
				}
				this.m_CollidingPlants[obj].Add(material.shader);
				material.shader = this.m_ShaderOfCollidingPlant;
				if (texture != null)
				{
					material.SetTexture("_Diffuse", texture);
				}
				if (texture2 != null)
				{
					material.SetTexture("_Normal", texture2);
				}
				if (texture3 != null)
				{
					material.SetTexture("_Smoothness", texture3);
				}
			}
		}
	}

	private void RemoveCollidingPlant(GameObject obj)
	{
		if (obj == null)
		{
			this.m_CollidingPlants.Remove(obj);
			return;
		}
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(obj);
		int num = 0;
		foreach (Renderer renderer in componentsDeepChild)
		{
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				renderer.materials[j].shader = this.m_CollidingPlants[obj][num];
				num++;
			}
		}
		this.m_CollidingPlants.Remove(obj);
	}

	public override bool TriggerThrough()
	{
		return true;
	}

	public void Save(int index)
	{
		SaveGame.SaveVal("GhostPos" + index, base.transform.position);
		SaveGame.SaveVal("GhostRot" + index, base.transform.rotation);
		for (int i = 0; i < this.m_Steps.Count; i++)
		{
			for (int j = 0; j < this.m_Steps[i].m_Slots.Count; j++)
			{
				SaveGame.SaveVal("GhostSlot" + (index * this.m_SaveLoadIndexMul).ToString() + i.ToString() + j.ToString(), this.m_Steps[i].m_Slots[j].m_Fulfilled);
			}
		}
		SaveGame.SaveVal("GhostSelectedSlot" + index, this.m_SelectedSlot ? this.m_SelectedSlot.transform.position : Vector3.zero);
		SaveGame.SaveVal("GhostUpperLevel" + index, this.IsOnUpperLevel());
		SaveGame.SaveVal("GhostConstructionBelow" + index, this.m_ConstructionBelow ? this.m_ConstructionBelow.gameObject.transform.position : Vector3.zero);
		SaveGame.SaveVal("GhostConstructionBelowName" + index, this.m_ConstructionBelow ? this.m_ConstructionBelow.name : "None");
	}

	public void Load(int index)
	{
		base.transform.position = SaveGame.LoadV3Val("GhostPos" + index);
		base.transform.rotation = SaveGame.LoadQVal("GhostRot" + index);
		float num = (float)((SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate11) ? this.m_SaveLoadIndexMul : 1);
		for (int i = 0; i < this.m_Steps.Count; i++)
		{
			for (int j = 0; j < this.m_Steps[i].m_Slots.Count; j++)
			{
				if (SaveGame.LoadBVal("GhostSlot" + ((float)index * num).ToString() + i.ToString() + j.ToString()))
				{
					this.m_Steps[i].m_Slots[j].Fulfill(true);
				}
			}
		}
		this.m_Firecamp = null;
		this.m_FirecampRacks.Clear();
		this.m_Smoker = null;
		if (this.m_PlacingCondition == ConstructionGhost.GhostPlacingCondition.NeedFirecamp)
		{
			int num2 = Physics.OverlapBoxNonAlloc(base.transform.TransformPoint(this.m_BoxCollider.center), this.m_BoxCollider.size * 0.5f, ConstructionGhost.s_CollidersTemp);
			for (int k = 0; k < num2; k++)
			{
				this.m_Firecamp = ConstructionGhost.s_CollidersTemp[k].gameObject.GetComponent<Firecamp>();
				if (this.m_Firecamp != null)
				{
					break;
				}
			}
		}
		else if (ItemInfo.IsFirecamp(this.m_ResultItemID))
		{
			int num3 = Physics.OverlapBoxNonAlloc(base.transform.TransformPoint(this.m_BoxCollider.center), this.m_BoxCollider.size * 0.5f, ConstructionGhost.s_CollidersTemp);
			for (int l = 0; l < num3; l++)
			{
				FirecampRack component = ConstructionGhost.s_CollidersTemp[l].gameObject.GetComponent<FirecampRack>();
				if (component)
				{
					this.m_FirecampRacks.Add(component);
				}
				else
				{
					FoodProcessor component2 = ConstructionGhost.s_CollidersTemp[l].gameObject.GetComponent<FoodProcessor>();
					if (component2 && component2.m_Type == FoodProcessor.Type.Smoker)
					{
						this.m_Smoker = ConstructionGhost.s_CollidersTemp[l].gameObject.GetComponent<Construction>();
						break;
					}
				}
			}
		}
		Vector3 vector = SaveGame.LoadV3Val("GhostSelectedSlot" + index);
		this.m_UpperLevel = SaveGame.LoadBVal("GhostUpperLevel" + index);
		if (vector != Vector3.zero)
		{
			for (int m = 0; m < Construction.s_AllConstructions.Count; m++)
			{
				Construction construction = Construction.s_AllConstructions[m];
				if (!(construction == null))
				{
					ConstructionSlot[] constructionSlots = construction.m_ConstructionSlots;
					int n = 0;
					while (n < constructionSlots.Length)
					{
						ConstructionSlot constructionSlot = constructionSlots[n];
						if (constructionSlot.m_MatchingItemIDs.Contains(this.m_ResultItemID) && (vector - constructionSlot.transform.position).sqrMagnitude < 0.01f)
						{
							this.m_SelectedSlot = constructionSlot;
							if (!constructionSlot.m_ParentConstruction)
							{
								break;
							}
							FirecampRack component3 = constructionSlot.m_ParentConstruction.gameObject.GetComponent<FirecampRack>();
							if (component3 && !this.m_FirecampRacks.Contains(component3))
							{
								this.m_FirecampRacks.Add(component3);
							}
							FoodProcessor component4 = constructionSlot.m_ParentConstruction.gameObject.GetComponent<FoodProcessor>();
							if (component4 && component4.m_Type == FoodProcessor.Type.Smoker && this.m_Smoker == null)
							{
								this.m_Smoker = constructionSlot.m_ParentConstruction.gameObject.GetComponent<Construction>();
								break;
							}
							break;
						}
						else
						{
							n++;
						}
					}
				}
			}
		}
		Vector3 vector2 = SaveGame.LoadV3Val("GhostConstructionBelow" + index);
		string text = SaveGame.LoadSVal("GhostConstructionBelowName" + index);
		if (vector2 != Vector3.zero && text != "None")
		{
			for (int num4 = 0; num4 < Construction.s_AllConstructions.Count; num4++)
			{
				Construction construction2 = Construction.s_AllConstructions[num4];
				if (!(construction2 == null) && (vector2 - construction2.transform.position).sqrMagnitude < 0.01f && construction2.name == text)
				{
					this.m_ConstructionBelow = construction2;
					return;
				}
			}
		}
	}

	public ConstructionGhost.ProhibitionType GetProhibitionType()
	{
		return this.m_ProhibitionType;
	}

	public int GetInsertedItemsCountInCurrentStep(ItemID id)
	{
		int num = 0;
		foreach (GhostSlot ghostSlot in this.m_Steps[this.m_CurrentStep].m_Slots)
		{
			if (ghostSlot.m_Fulfilled && ghostSlot.m_ItemID == id)
			{
				num++;
			}
		}
		return num;
	}

	public int GetItemsCountInCurrentStep(ItemID id)
	{
		int num = 0;
		using (List<GhostSlot>.Enumerator enumerator = this.m_Steps[this.m_CurrentStep].m_Slots.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_ItemID == id)
				{
					num++;
				}
			}
		}
		return num;
	}

	public void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		if (this.m_ReplStepsState == null)
		{
			this.m_ReplStepsState = new int[this.m_Steps.Count];
		}
		for (int i = 0; i < this.m_Steps.Count; i++)
		{
			for (int j = 0; j < this.m_Steps[i].m_Slots.Count; j++)
			{
				if (this.m_Steps[i].m_Slots[j].m_Fulfilled)
				{
					this.m_ReplStepsState[i] |= 1 << j;
				}
			}
		}
		writer.Write(this.m_ReplStepsState);
	}

	public void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		this.m_ReplStepsState = reader.ReadIntArray();
		DebugUtils.Assert(this.m_ReplStepsState.Length == this.m_Steps.Count, true);
		if (this.m_ReplStepsState.Length == this.m_Steps.Count)
		{
			for (int i = 0; i < this.m_Steps.Count; i++)
			{
				for (int j = 0; j < this.m_Steps[i].m_Slots.Count; j++)
				{
					bool flag = (this.m_ReplStepsState[i] & 1 << j) != 0;
					if (flag && flag != this.m_Steps[i].m_Slots[j].m_Fulfilled)
					{
						this.m_Steps[i].m_Slots[j].Fulfill(false);
					}
				}
			}
		}
	}

	public void OnReplicationPrepare()
	{
	}

	public void OnReplicationResolve()
	{
	}

	public void ReplOnChangedOwner(bool was_owner)
	{
	}

	public void ReplOnSpawned()
	{
	}

	public ConstructionGhost.GhostState m_State;

	private ConstructionGhost.ProhibitionType m_ProhibitionType;

	private Color m_Color = Color.clear;

	private Color m_ColorFresnel = Color.clear;

	private Color m_AvailableCol = Color.white;

	private Color m_NotAvailableHardCol = Color.red;

	private Color m_NotAvailableSoftCol = Color.yellow;

	private Color m_AvailableColorFresnel = Color.white;

	private Color m_NotAvailableHardColorFresnel = Color.red;

	private Color m_NotAvailableSoftColorFresnel = Color.yellow;

	public Material m_ActiveMaterial;

	public Material m_InactiveMaterial;

	public Material m_HighlightedMaterial;

	[HideInInspector]
	public string m_ActiveMaterialName;

	[HideInInspector]
	public string m_HighlightedMaterialName;

	[HideInInspector]
	public Material m_PrevMaterial;

	public float m_PositionOffsetMin = 2f;

	public float m_PositionOffsetMax = 6f;

	private Construction m_ConstructionToAttachTo;

	[HideInInspector]
	public ConstructionSlot m_SelectedSlot;

	public float m_Rotation;

	private bool m_Rotate;

	private float m_RotationSpeed = 150f;

	private float m_MaxRot = 5f;

	[HideInInspector]
	public ItemID m_ResultItemID = ItemID.None;

	private ConstructionInfo m_ResultInfo;

	public GameObject m_ResultPrefab;

	public List<GhostStep> m_Steps = new List<GhostStep>();

	private int m_CurrentStep;

	public float m_FirecampSnapDist = 1f;

	private Firecamp m_Firecamp;

	public float m_FirecampRackSnapDist = 1f;

	private List<FirecampRack> m_FirecampRacks = new List<FirecampRack>();

	public float m_SmokerSnapDist = 1f;

	private Construction m_Smoker;

	public float m_ColliderShrinkBottom;

	public ConstructionGhost.GhostPlacingCondition m_PlacingCondition = ConstructionGhost.GhostPlacingCondition.Whatever;

	public List<ConstructionGhost.GhostPlacingCondition> m_AdditionalPlacingConditions = new List<ConstructionGhost.GhostPlacingCondition>();

	public float m_RayCheckLength = 0.3f;

	public float m_RayCheckUpOffset = 0.1f;

	public float m_MaxDepthOfWater;

	public string m_ConstructionObjectName = string.Empty;

	private List<int> m_LayerMasksToIgnore = new List<int>();

	private int m_ItemLayer = -1;

	private Dictionary<GameObject, List<Shader>> m_CollidingPlants = new Dictionary<GameObject, List<Shader>>();

	private Shader m_ShaderOfCollidingPlant;

	public bool m_CanDeconstruct = true;

	public bool m_AllignToTerrain;

	public float m_MaxAllignAngle;

	public bool m_DebugBuild;

	private List<string> m_DeconsructSounds = new List<string>();

	private Dictionary<int, List<AudioClip>> m_InsertItemClipsDict = new Dictionary<int, List<AudioClip>>();

	private AudioSource m_AudioSource;

	protected Vector3 m_RaycastOrig = Vector3.zero;

	private List<IGhostObserver> m_Observers = new List<IGhostObserver>();

	public List<BoxCollider> m_Corners;

	public BoxCollider m_Hook;

	public bool m_IgnoreTerrain;

	[HideInInspector]
	public bool m_DestroyOnCreateConstruction = true;

	private BoxCollider[] m_TestColliders;

	public bool m_RestoreIfNotAttached;

	private int[] m_ReplStepsState;

	public List<string> m_CreateConstructionHints = new List<string>();

	private static RaycastHit[] s_RaycastHitsTmp = new RaycastHit[20];

	[HideInInspector]
	public bool m_Challenge;

	public bool m_CanBeRotatedWhenSnapped;

	public ConstructionGhost.SnappedRotationAxis m_SnappedRotationAxis;

	public ConstructionGhost.SnappedRotationAngle m_SnappedRotationAngle;

	public float m_LowerLevelCutLevel = -100f;

	public float m_UpperLevelCutLevel = -10f;

	private int m_ShaderCutLevel = Shader.PropertyToID("_CutLevel");

	private int m_ShaderObjectWorldPos = Shader.PropertyToID("_ObjectWorldPos");

	public List<BoxCollider> m_UpperLevelCorners = new List<BoxCollider>();

	public bool m_CanBePlacedOnTopOfConstruction = true;

	private bool m_UpperLevel;

	private static Collider[] s_CollidersTemp = new Collider[20];

	private List<Collider> m_Colliders = new List<Collider>();

	private int m_RotationSnap;

	private bool m_OnTopOfConstruction;

	[HideInInspector]
	public Construction m_ConstructionBelow;

	private static ConstructionGhost.CompareHitsByDist s_CompareHitsByDist = new ConstructionGhost.CompareHitsByDist();

	private List<ConstructionSlotData> m_AttachmentData = new List<ConstructionSlotData>(10);

	public static CompareSlotsByDot s_DotComparer = new CompareSlotsByDot();

	private int m_ShaderPropertyFresnelColor = -1;

	private int m_SaveLoadIndexMul = 100000;

	public enum GhostState
	{
		None,
		Dragging,
		Building,
		Ready
	}

	public enum GhostPlacingCondition
	{
		MustBeInWater,
		CantBeInWater,
		Whatever,
		NeedFirecamp,
		IsSnapped
	}

	public enum ProhibitionType
	{
		None,
		Hard,
		Soft,
		Depth
	}

	public enum SnappedRotationAxis
	{
		X,
		Y,
		Z
	}

	public enum SnappedRotationAngle
	{
		Angle90,
		Angle180
	}

	private class CompareHitsByDist : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit i1, RaycastHit i2)
		{
			DebugUtils.Assert(this.m_Parent != null, true);
			float num = Vector3.Distance(this.m_Parent.m_RaycastOrig, i1.point);
			float num2 = Vector3.Distance(this.m_Parent.m_RaycastOrig, i2.point);
			if (num > num2)
			{
				return 1;
			}
			if (num < num2)
			{
				return -1;
			}
			return 0;
		}

		public ConstructionGhost m_Parent;
	}
}
