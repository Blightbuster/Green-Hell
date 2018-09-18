using System;
using System.Collections.Generic;
using System.Linq;
using CJTools;
using Enums;
using UltimateWater;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class ConstructionGhost : Trigger
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
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (ConstructionGhostManager.Get())
		{
			ConstructionGhostManager.Get().RegisterGhost(this);
		}
	}

	protected override void Start()
	{
		base.Start();
		this.m_BoxCollider = base.gameObject.GetComponent<BoxCollider>();
		DebugUtils.Assert(this.m_BoxCollider, "[ConstructionGhost::Start] Can't find box collider!", true, DebugUtils.AssertType.Info);
		Physics.IgnoreCollision(Player.Get().GetComponent<Collider>(), this.m_BoxCollider);
		if (this.m_State == ConstructionGhost.GhostState.None)
		{
			this.SetState(ConstructionGhost.GhostState.Building);
		}
		this.m_ItemLayer = LayerMask.NameToLayer("Item");
		this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("NoCollisionWithPlayer"));
		this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("SmallPlant"));
		this.m_LayerMasksToIgnore.Add(this.m_ItemLayer);
		this.m_ShaderOfCollidingPlant = Shader.Find("Shader Forge/select_yellow_shader");
		this.SetupAudio();
		this.m_ShaderPropertyFresnelColor = Shader.PropertyToID("_fresnelcolor");
		this.m_AvailableCol = Color.white;
		this.m_NotAvailableHardCol = new Color(0.854f, 0.149f, 0.149f, 0.627f);
		this.m_NotAvailableSoftCol = Color.yellow;
		this.m_AvailableColorFresnel = new Color(1f, 1f, 1f, 1f);
		this.m_NotAvailableHardColorFresnel = new Color(1f, 0.196f, 0.196f, 0f);
		this.m_NotAvailableSoftColorFresnel = new Color(1f, 0.92f, 0.016f, 1f);
		ConstructionGhostManager.Get().RegisterGhost(this);
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
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		int i = 0;
		while (i < this.m_CollidingPlants.Count)
		{
			this.RemoveCollidingPlant(this.m_CollidingPlants.Keys.ElementAt(i));
		}
		ConstructionGhostManager.Get().UnregisterGhost(this);
	}

	public void RegisterObserver(IGhostObserver observer)
	{
		this.m_Observers.Add(observer);
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
		ConstructionGhost.GhostState state = this.m_State;
		if (state != ConstructionGhost.GhostState.Dragging)
		{
			if (state != ConstructionGhost.GhostState.Building)
			{
				if (state == ConstructionGhost.GhostState.Ready)
				{
					this.CreateConstruction();
					UnityEngine.Object.Destroy(base.gameObject);
					HUDGather.Get().Setup();
				}
			}
			else
			{
				foreach (GhostStep ghostStep in this.m_Steps)
				{
					foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
					{
						ghostSlot.Init();
					}
				}
				this.SetupCurrentStep();
				HUDGather.Get().Setup();
			}
		}
		else
		{
			this.UpdateState();
		}
	}

	private void SetupCurrentStep()
	{
		if (this.m_CurrentStep >= this.m_Steps.Count)
		{
			return;
		}
		GhostStep ghostStep = this.m_Steps[this.m_CurrentStep];
		foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
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

	private void UpdateState()
	{
		ConstructionGhost.GhostState state = this.m_State;
		if (state != ConstructionGhost.GhostState.Dragging)
		{
			if (state == ConstructionGhost.GhostState.Building)
			{
				if (this.IsReady())
				{
					this.SetState(ConstructionGhost.GhostState.Ready);
				}
			}
		}
		else
		{
			this.UpdateRotation();
			this.UpdateTransform();
			this.UpdateColor();
		}
	}

	private void FixedUpdate()
	{
		if (this.m_State == ConstructionGhost.GhostState.Dragging)
		{
			this.UpdateProhibitionType();
		}
	}

	private bool CheckCornerInAir(Vector3 corner)
	{
		corner.y += this.m_RayCheckUpOffset;
		RaycastHit[] array = Physics.RaycastAll(corner, Vector3.down, this.m_RayCheckLength);
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i].collider == this.m_BoxCollider))
			{
				if (array[i].collider.GetType() == typeof(TerrainCollider))
				{
					DebugRender.DrawLine(corner, corner + Vector3.down * this.m_RayCheckLength, Color.green, 0f);
					return false;
				}
			}
		}
		DebugRender.DrawLine(corner, corner + Vector3.down * this.m_RayCheckLength, Color.red, 0f);
		return true;
	}

	private void UpdateProhibitionType()
	{
		this.m_ProhibitionType = ConstructionGhost.ProhibitionType.None;
		if (this.m_AllignToTerrain && !this.m_Smoker && this.m_FirecampRacks.Count == 0)
		{
			float num = Vector3.Angle(base.transform.up, Vector3.up);
			if (num > this.m_MaxAllignAngle)
			{
				this.ClearCollidingPlants();
				this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
				return;
			}
		}
		Vector3 center = this.m_BoxCollider.bounds.center;
		Vector3 halfExtents = this.m_BoxCollider.size * 0.5f;
		center.y += this.m_ColliderShrinkBottom * 0.5f;
		halfExtents.y -= this.m_ColliderShrinkBottom;
		halfExtents.y = Mathf.Max(halfExtents.y, this.m_ColliderShrinkBottom);
		Collider[] collection = Physics.OverlapBox(center, halfExtents, this.m_BoxCollider.transform.rotation);
		List<Collider> list = new List<Collider>(collection);
		list.Remove(this.m_BoxCollider);
		if (this.m_PlacingCondition == ConstructionGhost.GhostPlacingCondition.NeedFirecamp && this.m_Firecamp)
		{
			list.Remove(this.m_Firecamp.m_Collider);
		}
		if ((ItemInfo.IsFirecamp(this.m_ResultItemID) || ItemInfo.IsStoneRing(this.m_ResultItemID)) && this.m_FirecampRacks.Count > 0)
		{
			foreach (FirecampRack firecampRack in this.m_FirecampRacks)
			{
				list.Remove(firecampRack.m_Collider);
			}
		}
		if ((ItemInfo.IsFirecamp(this.m_ResultItemID) || ItemInfo.IsStoneRing(this.m_ResultItemID)) && this.m_Smoker)
		{
			list.Remove(this.m_Smoker.m_Collider);
		}
		foreach (Collider collider in list)
		{
			if (!(collider.tag == "Sectr_trigger"))
			{
				if (this.m_ConstructionToAttachTo)
				{
					if (collider.gameObject == this.m_ConstructionToAttachTo.gameObject)
					{
						continue;
					}
					if (collider.gameObject == Terrain.activeTerrain.gameObject)
					{
						continue;
					}
				}
				if (!collider.gameObject.IsWater() && !this.m_LayerMasksToIgnore.Contains(collider.gameObject.layer) && !collider.isTrigger)
				{
					this.ClearCollidingPlants();
					this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
					return;
				}
				ConstructionGhost component = collider.gameObject.GetComponent<ConstructionGhost>();
				if (component != null)
				{
					foreach (GhostStep ghostStep in component.m_Steps)
					{
						foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
						{
							if (this.m_Collider.bounds.Intersects(ghostSlot.m_Collider.bounds))
							{
								this.ClearCollidingPlants();
								this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
								return;
							}
						}
					}
				}
				Construction component2 = collider.gameObject.GetComponent<Construction>();
				if (component2)
				{
					this.ClearCollidingPlants();
					this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
					return;
				}
			}
		}
		Collider collider2 = null;
		foreach (Collider collider3 in list)
		{
			if (collider3.gameObject.IsWater())
			{
				collider2 = collider3;
				break;
			}
		}
		if (this.m_PlacingCondition == ConstructionGhost.GhostPlacingCondition.MustBeInWater && !collider2)
		{
			this.ClearCollidingPlants();
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
			return;
		}
		if ((this.m_PlacingCondition == ConstructionGhost.GhostPlacingCondition.CantBeInWater || this.m_PlacingCondition == ConstructionGhost.GhostPlacingCondition.NeedFirecamp) && collider2)
		{
			this.ClearCollidingPlants();
			this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
			return;
		}
		if (collider2 && this.m_MaxDepthOfWater > 0f)
		{
			float num2 = collider2.bounds.max.y - this.m_BoxCollider.bounds.min.y;
			if (num2 > this.m_MaxDepthOfWater)
			{
				this.ClearCollidingPlants();
				this.m_ProhibitionType = ConstructionGhost.ProhibitionType.Hard;
				return;
			}
		}
		if (this.m_PlacingCondition == ConstructionGhost.GhostPlacingCondition.Whatever || this.m_PlacingCondition == ConstructionGhost.GhostPlacingCondition.CantBeInWater)
		{
			List<GameObject> list2 = new List<GameObject>();
			foreach (Collider collider4 in list)
			{
				if (this.m_LayerMasksToIgnore.Contains(collider4.gameObject.layer) && collider4.gameObject.layer != this.m_ItemLayer)
				{
					if (!this.m_CollidingPlants.ContainsKey(collider4.gameObject))
					{
						this.AddCollidingPlant(collider4.gameObject);
					}
					list2.Add(collider4.gameObject);
				}
			}
			int i = 0;
			while (i < this.m_CollidingPlants.Keys.Count)
			{
				if (!list2.Contains(this.m_CollidingPlants.ElementAt(i).Key))
				{
					this.RemoveCollidingPlant(this.m_CollidingPlants.ElementAt(i).Key);
				}
				else
				{
					i++;
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
			Player.Get().BlockRotation();
		}
		else if (this.m_Rotate && !InputsManager.Get().IsActionActive(InputsManager.InputAction.ConstructionRotate))
		{
			this.m_Rotate = false;
			Player.Get().UnblockRotation();
		}
		if (this.m_Rotate)
		{
			float num = Mathf.Clamp(CrossPlatformInputManager.GetAxis("Mouse X"), -this.m_MaxRot, this.m_MaxRot);
			float num2 = num * this.m_RotationSpeed * Time.deltaTime;
			this.m_Rotation -= num2;
		}
	}

	private void UpdateColor()
	{
		Color color = this.m_AvailableCol;
		Color color_fresnel = this.m_AvailableColorFresnel;
		if (this.m_ProhibitionType == ConstructionGhost.ProhibitionType.Hard)
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

	private void UpdateTransform()
	{
		if (this.UpdateAttaching())
		{
			return;
		}
		this.m_ConstructionToAttachTo = null;
		base.gameObject.transform.rotation = Player.Get().transform.rotation;
		base.gameObject.transform.Rotate(Vector3.up, this.m_Rotation);
		bool enabled = this.m_BoxCollider.enabled;
		this.m_BoxCollider.enabled = false;
		Vector3 vector = base.gameObject.transform.position;
		vector.y += 5f;
		float terrainY = MainLevel.GetTerrainY(vector);
		if (terrainY >= vector.y)
		{
			vector.y = terrainY;
		}
		else
		{
			float num = float.MinValue;
			foreach (RaycastHit raycastHit in Physics.RaycastAll(vector, -Vector3.up))
			{
				if (!(raycastHit.collider.gameObject.GetComponent<Water>() != null))
				{
					if (raycastHit.collider.GetType() == typeof(TerrainCollider))
					{
						if (raycastHit.point.y > num)
						{
							vector = raycastHit.point;
							num = raycastHit.point.y;
						}
					}
				}
			}
		}
		float num2 = vector.y;
		this.m_BoxCollider.enabled = enabled;
		num2 -= this.m_BoxCollider.bounds.min.y - base.gameObject.transform.position.y;
		Vector3 position = Player.Get().transform.position;
		Vector3 forward = Player.Get().transform.forward;
		Vector3 normalized2D = forward.GetNormalized2D();
		Vector3 position2 = position + normalized2D * CJTools.Math.GetProportionalClamp(this.m_PositionOffsetMin, this.m_PositionOffsetMax, Camera.main.transform.forward.y, -0.5f, 0f);
		position2.y = num2;
		base.gameObject.transform.position = position2;
		if (this.UpdateSnapToFirecamp())
		{
			return;
		}
		if (this.UpdateSnapToRack())
		{
			return;
		}
		if (this.UpdateSnapToSmoker())
		{
			return;
		}
		this.AllignToTerrain();
	}

	private void AllignToTerrain()
	{
		if (!this.m_AllignToTerrain)
		{
			return;
		}
		bool enabled = this.m_BoxCollider.enabled;
		this.m_BoxCollider.enabled = false;
		RaycastHit raycastHit = default(RaycastHit);
		Vector3 normalized2D = base.transform.forward.GetNormalized2D();
		Vector3 vector = this.m_BoxCollider.bounds.center + normalized2D * this.m_BoxCollider.size.z + Vector3.up * this.m_BoxCollider.size.y;
		this.GetRaycastHit(vector, ref raycastHit);
		vector = raycastHit.point;
		Vector3 vector2 = this.m_BoxCollider.bounds.center - normalized2D * this.m_BoxCollider.size.z + Vector3.up * this.m_BoxCollider.size.y;
		this.GetRaycastHit(vector2, ref raycastHit);
		vector2 = raycastHit.point;
		Vector3 vector3 = vector - vector2;
		Vector3 normalized2D2 = base.transform.right.GetNormalized2D();
		Vector3 vector4 = Vector3.zero;
		Vector3 vector5 = this.m_BoxCollider.bounds.center + normalized2D2 * this.m_BoxCollider.size.x + Vector3.up * this.m_BoxCollider.size.y;
		this.GetRaycastHit(vector5, ref raycastHit);
		vector5 = raycastHit.point;
		Vector3 vector6 = this.m_BoxCollider.bounds.center - normalized2D2 * this.m_BoxCollider.size.x + Vector3.up * this.m_BoxCollider.size.y;
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

	private void GetRaycastHit(Vector3 pos, ref RaycastHit hit)
	{
		pos.y = Mathf.Max(pos.y, MainLevel.GetTerrainY(pos) + 0.1f);
		RaycastHit[] collection = Physics.RaycastAll(pos, -Vector3.up);
		this.m_RaycastOrig = pos;
		List<RaycastHit> list = new List<RaycastHit>(collection);
		list.Sort(new Comparison<RaycastHit>(this.CompareListByDist));
		foreach (RaycastHit raycastHit in list)
		{
			if (!(raycastHit.collider.gameObject == base.gameObject))
			{
				if (!raycastHit.collider.isTrigger)
				{
					if (!this.m_LayerMasksToIgnore.Contains(raycastHit.collider.gameObject.layer))
					{
						hit = raycastHit;
						break;
					}
				}
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
			float num2 = Vector3.Distance(base.transform.position, firecamp2.transform.position);
			if (num2 < num)
			{
				firecamp = firecamp2;
				num = num2;
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
		if (!ItemInfo.IsFirecamp(this.m_ResultItemID) && !ItemInfo.IsStoneRing(this.m_ResultItemID))
		{
			return false;
		}
		this.m_FirecampRacks.Clear();
		foreach (FirecampRack firecampRack in FirecampRack.s_FirecampRacks)
		{
			float num = Vector3.Distance(base.transform.position, firecampRack.transform.position);
			if (num < this.m_FirecampSnapDist)
			{
				this.m_FirecampRacks.Add(firecampRack);
			}
		}
		if (this.m_FirecampRacks.Count > 0)
		{
			Vector3 position = this.m_FirecampRacks[0].transform.position;
			position.y -= this.m_BoxCollider.bounds.min.y - base.gameObject.transform.position.y;
			base.transform.position = position;
			this.AllignToTerrain();
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
		for (int i = 0; i < Construction.s_Constructions.Count; i++)
		{
			if (ItemInfo.IsSmoker(Construction.s_Constructions[i].GetInfoID()))
			{
				float num2 = Vector3.Distance(base.transform.position, Construction.s_Constructions[i].transform.position);
				if (num2 < num)
				{
					construction = Construction.s_Constructions[i];
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
		while (i < Construction.s_Constructions.Count)
		{
			Construction construction2 = Construction.s_Constructions[i];
			if (!(construction2 == null))
			{
				goto IL_A2;
			}
			if (Construction.s_Constructions[i])
			{
				construction2 = Construction.s_Constructions[i];
			}
			if (!(construction2 == null))
			{
				goto IL_A2;
			}
			IL_1EA:
			i++;
			continue;
			IL_A2:
			Vector3 vector = construction2.gameObject.transform.position;
			if ((vector - Player.Get().transform.position).magnitude > this.m_PositionOffsetMax)
			{
				goto IL_1EA;
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
				goto IL_1EA;
			}
			goto IL_1EA;
		}
		if (construction != null)
		{
			base.gameObject.transform.rotation = rotation;
			base.gameObject.transform.position = position;
			this.m_ConstructionToAttachTo = construction;
			this.AllignToTerrain();
			return true;
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
		foreach (Renderer renderer in componentsInChildren)
		{
			foreach (Material material in renderer.materials)
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
		GhostStep ghostStep = this.m_Steps[this.m_CurrentStep];
		foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
		{
			if (!ghostSlot.m_Fulfilled)
			{
				flag = false;
				break;
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
			}
			else
			{
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
	}

	private bool IsReady()
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
			foreach (IFirecampAttach firecampAttach in components)
			{
				firecampAttach.SetFirecamp(this.m_Firecamp);
			}
		}
		if (this.m_FirecampRacks.Count > 0 && ItemInfo.IsFirecamp(item.GetInfoID()))
		{
			foreach (FirecampRack firecampRack in this.m_FirecampRacks)
			{
				IFirecampAttach[] components2 = firecampRack.gameObject.GetComponents<IFirecampAttach>();
				foreach (IFirecampAttach firecampAttach2 in components2)
				{
					firecampAttach2.SetFirecamp((Firecamp)item);
				}
			}
		}
		if (this.m_Smoker && ItemInfo.IsFirecamp(item.GetInfoID()))
		{
			IFirecampAttach[] components3 = this.m_Smoker.gameObject.GetComponents<IFirecampAttach>();
			foreach (IFirecampAttach firecampAttach3 in components3)
			{
				firecampAttach3.SetFirecamp((Firecamp)item);
			}
		}
		if (this.m_ConstructionObjectName.Length > 0)
		{
			item.gameObject.name = this.m_ConstructionObjectName;
		}
		ScenarioAction.OnItemCreated(item.gameObject);
		ScenarioCndTF.OnItemCreated(item.gameObject);
		EventsManager.OnEvent(Enums.Event.Build, 1, (int)this.m_ResultItemID);
		HUDMessages hudmessages = (HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages));
		hudmessages.AddMessage(GreenHellGame.Instance.GetLocalization().Get(this.m_ResultItemID.ToString()) + " " + GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_Created"), null, HUDMessageIcon.None, string.Empty);
		PlayerAudioModule.Get().PlayBuildCompletedSound();
		ItemsManager.Get().OnCreateItem(this.m_ResultItemID);
		item.SetLayer(item.transform, LayerMask.NameToLayer("Item"));
		foreach (IGhostObserver ghostObserver in this.m_Observers)
		{
			ghostObserver.OnCreateConstruction(this, item);
		}
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
		return !GreenHellGame.TWITCH_DEMO;
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
		return this.m_ResultItemID.ToString();
	}

	private void AddCollidingPlant(GameObject obj)
	{
		List<Renderer> componentsDeepChild = General.GetComponentsDeepChild<Renderer>(obj);
		for (int i = 0; i < componentsDeepChild.Count; i++)
		{
			Renderer renderer = componentsDeepChild[i];
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				Material material = renderer.materials[j];
				Texture texture = (!material.HasProperty("_MainTex")) ? null : material.GetTexture("_MainTex");
				Texture texture2 = (!material.HasProperty("_BumpMap")) ? null : material.GetTexture("_BumpMap");
				Texture texture3 = (!material.HasProperty("_RoughTex")) ? null : material.GetTexture("_RoughTex");
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
		List<Renderer> componentsDeepChild = General.GetComponentsDeepChild<Renderer>(obj);
		int num = 0;
		for (int i = 0; i < componentsDeepChild.Count; i++)
		{
			Renderer renderer = componentsDeepChild[i];
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				Material material = renderer.materials[j];
				material.shader = this.m_CollidingPlants[obj][num];
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
				SaveGame.SaveVal("GhostSlot" + index.ToString() + i.ToString() + j.ToString(), this.m_Steps[i].m_Slots[j].m_Fulfilled);
			}
		}
	}

	public void Load(int index)
	{
		base.transform.position = SaveGame.LoadV3Val("GhostPos" + index);
		base.transform.rotation = SaveGame.LoadQVal("GhostRot" + index);
		for (int i = 0; i < this.m_Steps.Count; i++)
		{
			for (int j = 0; j < this.m_Steps[i].m_Slots.Count; j++)
			{
				if (SaveGame.LoadBVal("GhostSlot" + index.ToString() + i.ToString() + j.ToString()))
				{
					this.m_Steps[i].m_Slots[j].Fulfill(true);
				}
			}
		}
	}

	public ConstructionGhost.ProhibitionType GetProhibitionType()
	{
		return this.m_ProhibitionType;
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
	public Material m_PrevMaterial;

	public float m_PositionOffsetMin = 2f;

	public float m_PositionOffsetMax = 6f;

	private Construction m_ConstructionToAttachTo;

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

	private Vector3 m_RaycastOrig = Vector3.zero;

	private List<IGhostObserver> m_Observers = new List<IGhostObserver>();

	private int m_ShaderPropertyFresnelColor = -1;

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
		NeedFirecamp
	}

	public enum ProhibitionType
	{
		None,
		Hard,
		Soft
	}
}
