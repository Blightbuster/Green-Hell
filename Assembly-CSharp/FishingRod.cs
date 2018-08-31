using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using UnityEngine;

public class FishingRod : MonoBehaviour
{
	private void Awake()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_HookPrefab);
		this.m_Hook = gameObject.GetComponent<FishingHook>();
		this.m_Hook.GetComponent<Rigidbody>().isKinematic = true;
		this.m_Hook.transform.rotation = Quaternion.AngleAxis(-90f, Vector3.forward);
		this.m_Hook.SetFishingRod(this);
		this.m_Hook.gameObject.SetActive(false);
		this.m_FishingRodItem = base.gameObject.GetComponent<Item>();
	}

	private void Start()
	{
		int num = 1;
		for (;;)
		{
			Transform transform = base.gameObject.transform.FindDeepChild("Bone" + ((num >= 10) ? (string.Empty + num) : ("0" + num)));
			if (!transform)
			{
				break;
			}
			this.m_Bones.Add(transform.gameObject);
			num++;
		}
		base.enabled = false;
	}

	private void OnEnable()
	{
		this.m_Hook.gameObject.SetActive(true);
	}

	private void OnDisable()
	{
		if (this.m_Hook)
		{
			this.m_Hook.gameObject.SetActive(false);
		}
		this.SetState(FishingRod.State.None);
	}

	public void StartFishing(Vector3 pos)
	{
		this.m_FloatPos = pos;
		this.SetupHookInWaterPos();
		this.m_FishTank = AIManager.Get().GetFishTankWithPointInside(this.m_Hook.transform.position);
		if (this.m_FishTank)
		{
			this.m_FishTank.StartFishing(this);
		}
		this.m_Fish = null;
		this.m_Biting = false;
		this.SetState(FishingRod.State.Fishing);
	}

	public void StopFishing()
	{
		if (this.m_Fish)
		{
			UnityEngine.Object.Destroy(this.m_Fish);
			this.m_Fish = null;
		}
		if (this.m_FishTank)
		{
			this.m_FishTank.StopFishing();
			this.m_FishTank = null;
		}
		this.m_Rotation = 0f;
		this.RotateBones();
		this.SetState(FishingRod.State.None);
	}

	public void SetBiting(bool set)
	{
		this.m_Biting = set;
	}

	public void Strike()
	{
		this.m_Fish = null;
		Fish fish = (!this.m_Hook) ? null : this.m_Hook.GetFish();
		if (fish)
		{
			this.m_Fish = fish;
			this.m_Fish.Catch();
			this.m_Hook.DeleteBait();
			Skill.Get<FishingSkill>().OnSkillAction();
		}
		this.SetState(FishingRod.State.Strike);
	}

	public void Reel()
	{
		this.SetState(FishingRod.State.Reel);
	}

	public void DestroyFish()
	{
		if (this.m_Fish)
		{
			UnityEngine.Object.Destroy(this.m_Fish.gameObject);
			this.m_Fish = null;
		}
	}

	private void SetState(FishingRod.State state)
	{
		this.m_State = state;
	}

	private void SetupHookInWaterPos()
	{
		Ray ray = new Ray(this.m_FloatPos + Vector3.up * 0.1f, Vector3.down);
		RaycastHit[] collection = Physics.RaycastAll(ray);
		List<RaycastHit> list = new List<RaycastHit>(collection);
		FishTank fishTank = null;
		Vector3 vector = Vector3.zero;
		foreach (RaycastHit raycastHit in list)
		{
			fishTank = raycastHit.collider.gameObject.GetComponent<FishTank>();
			if (fishTank)
			{
				vector = raycastHit.point;
				break;
			}
		}
		float terrainY = MainLevel.GetTerrainY(this.m_FloatPos);
		if (terrainY >= this.m_FloatPos.y)
		{
			DebugUtils.Assert("[FishingRod:OnEnterState] Float is under terrain!", true, DebugUtils.AssertType.Info);
		}
		Vector3 floatPos = this.m_FloatPos;
		if (fishTank)
		{
			floatPos.y = vector.y - fishTank.m_BoxCollider.size.y * 0.5f;
		}
		if (floatPos.y < terrainY + 0.2f)
		{
			floatPos.y = terrainY + 0.2f;
		}
		floatPos.y = Mathf.Min(floatPos.y, this.m_FloatPos.y);
		this.m_Hook.transform.position = floatPos;
	}

	private void Update()
	{
		this.UpdateVein();
		FishingRod.State state = this.m_State;
		if (state != FishingRod.State.Fishing)
		{
			if (state == FishingRod.State.Reel)
			{
				this.UpdateReel();
			}
		}
		else
		{
			this.UpdateFishing();
		}
	}

	private void UpdateVein()
	{
		FishingController fishingController = FishingController.Get();
		switch (fishingController.m_State)
		{
		case FishingController.State.None:
		case FishingController.State.Waiting:
		case FishingController.State.Strike:
		case FishingController.State.Reel:
		case FishingController.State.Fish:
			this.m_Vein.transform.position = this.m_Hook.transform.position;
			break;
		case FishingController.State.Aim:
		case FishingController.State.Cast:
			this.m_Vein.transform.position = this.m_Top.transform.position;
			break;
		}
	}

	private void LateUpdate()
	{
		this.UpdateHook();
	}

	private void UpdateHook()
	{
		if (FishingController.Get().m_State == FishingController.State.Cast || FishingController.Get().m_State == FishingController.State.Aim)
		{
			if (this.m_Hook.gameObject.activeSelf)
			{
				this.m_Hook.gameObject.SetActive(false);
			}
		}
		else if (!this.m_Hook.gameObject.activeSelf)
		{
			this.m_Hook.gameObject.SetActive(true);
		}
		if (this.m_State == FishingRod.State.None && this.m_Hook.gameObject.activeSelf)
		{
			Vector3 vector = this.m_Top.transform.position + Vector3.down * 1f;
			vector.y = Mathf.Max(vector.y, MainLevel.GetTerrainY(vector) + 0.2f);
			this.m_Hook.transform.position = vector;
		}
	}

	private void UpdateFishing()
	{
		if (Input.GetKeyDown(KeyCode.N))
		{
			this.m_Biting = !this.m_Biting;
		}
		this.UpdateBones();
	}

	private void UpdateReel()
	{
		this.UpdateBones();
	}

	private void UpdateBones()
	{
		if (this.m_Biting)
		{
			this.m_Rotation += this.m_BitingRotationSpeed * Time.deltaTime;
			this.m_Rotation = Mathf.Clamp(this.m_Rotation, this.m_MaxBitingRotation, 0f);
		}
		else if (this.m_Rotation != 0f)
		{
			float num = Mathf.Max(0f, 1f - this.m_SpringDamping * Time.fixedDeltaTime);
			float num2 = -this.m_Rotation * this.m_SpringStiffness * Time.fixedDeltaTime;
			this.m_SpringCurrVel = this.m_SpringCurrVel * num + num2;
			this.m_Rotation += this.m_SpringCurrVel * Time.fixedDeltaTime;
			if (Mathf.Abs(this.m_Rotation) < this.m_SpringThreshold && Mathf.Abs(this.m_SpringCurrVel) < this.m_SpringThreshold)
			{
				this.m_Rotation = 0f;
				this.m_SpringCurrVel = this.m_SpringDefaultvel;
			}
		}
		this.RotateBones();
	}

	private void FixedUpdate()
	{
		this.UpdateBones();
	}

	private void RotateBones()
	{
		Quaternion localRotation = Quaternion.identity;
		for (int i = 1; i < this.m_Bones.Count; i++)
		{
			localRotation = this.m_Bones[i].transform.localRotation;
			localRotation.z = this.m_Rotation * ((float)i / (float)this.m_Bones.Count);
			this.m_Bones[i].transform.localRotation = localRotation;
		}
	}

	private FishingRod.State m_State;

	private List<GameObject> m_Bones = new List<GameObject>();

	public GameObject m_Top;

	private float m_Rotation;

	private float m_BitingRotationSpeed = -0.2f;

	private float m_MaxBitingRotation = -0.25f;

	private Vector3 m_FloatPos = Vector3.zero;

	public GameObject m_HookPrefab;

	[HideInInspector]
	public FishingHook m_Hook;

	private FishTank m_FishTank;

	[HideInInspector]
	public Fish m_Fish;

	private bool m_Biting;

	public GameObject m_Vein;

	private float m_SpringDefaultvel = 1f;

	private float m_SpringCurrVel = 1f;

	private float m_SpringStiffness = 500f;

	private float m_SpringDamping = 5f;

	private float m_SpringThreshold = 0.01f;

	[HideInInspector]
	public Item m_FishingRodItem;

	private enum State
	{
		None,
		Fishing,
		Strike,
		Reel
	}
}
