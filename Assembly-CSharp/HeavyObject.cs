using System;
using System.Collections.Generic;
using System.Linq;
using CJTools;
using Enums;
using UnityEngine;

public class HeavyObject : Item
{
	protected override void Awake()
	{
		base.Awake();
		int num = 0;
		for (int i = 0; i < 10; i++)
		{
			string name = "Attach" + i.ToString();
			Transform transform = base.gameObject.transform.FindDeepChild(name);
			if (transform)
			{
				this.m_DummiesToAttach.Add(transform);
				num++;
			}
		}
		this.m_NumObjectsToAttach = num;
	}

	protected override void Start()
	{
		base.Start();
		this.SetupAudio();
	}

	private void SetupAudio()
	{
		if (this.m_Info.m_ID == ItemID.Log)
		{
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/heavy_log_drop_01"));
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/heavy_log_drop_02"));
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/heavy_log_drop_04"));
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/heavy_log_drop_05"));
		}
		else if (this.m_Info.m_ID == ItemID.Big_Stone)
		{
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/heavy_stone_drop_01"));
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/heavy_stone_drop_03"));
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/heavy_stone_drop_04"));
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/heavy_stone_drop_05"));
		}
		else if (this.m_Info.m_ID == ItemID.Long_Stick)
		{
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/long_stick_drop_01"));
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/long_stick_drop_02"));
			this.m_CollisionClips.Add((AudioClip)Resources.Load("Sounds/Items/long_stick_drop_03"));
		}
	}

	public void AttachHeavyObject(HeavyObject ho)
	{
		if (!ho)
		{
			return;
		}
		Transform transform = this.FindFreeSlot();
		if (transform)
		{
			HeavyObject x = null;
			if (!this.m_Attached.TryGetValue(transform, out x) || x == null)
			{
				Physics.IgnoreCollision(Player.Get().GetComponent<Collider>(), ho.GetComponent<Collider>(), true);
				ho.transform.position = transform.position;
				ho.transform.rotation = transform.rotation;
				ho.gameObject.transform.parent = transform;
				this.m_Attached[transform] = ho;
				ho.OnItemAttachedToHand();
			}
		}
	}

	public void DetachHeavyObjects()
	{
		while (this.m_Attached.Count > 0)
		{
			this.DetachHeavyObject(0, false);
		}
	}

	public void DetachHeavyObject(int index, bool destroy = false)
	{
		if (index < 0 || index >= this.m_Attached.Count)
		{
			return;
		}
		Transform transform = this.m_Attached.Keys.ElementAt(index);
		if (transform == null)
		{
			this.m_Attached.Remove(transform);
			return;
		}
		HeavyObject heavyObject = this.m_Attached[transform];
		Physics.IgnoreCollision(Player.Get().GetComponent<Collider>(), heavyObject.GetComponent<Collider>(), false);
		heavyObject.gameObject.transform.parent = null;
		heavyObject.OnItemDetachedFromHand();
		this.m_Attached.Remove(transform);
		if (destroy)
		{
			UnityEngine.Object.Destroy(heavyObject.gameObject);
		}
		else
		{
			heavyObject.transform.rotation = Player.Get().transform.rotation;
			Vector3 position = new Vector3(0.5f, 0.5f, 1.1f);
			position.x = System.Math.Max(0.5f, position.x);
			Vector3 a = Camera.main.ViewportToWorldPoint(position);
			heavyObject.transform.position = a + Vector3.up * (float)(index + 1) * 0.2f;
		}
	}

	public Transform FindFreeSlot()
	{
		Transform result = null;
		for (int i = 0; i < this.m_DummiesToAttach.Count; i++)
		{
			Transform transform = this.m_DummiesToAttach[i];
			if (!this.m_Attached.ContainsKey(transform))
			{
				result = transform;
				break;
			}
		}
		return result;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (!this.m_Info.m_CanBeAddedToInventory)
		{
			actions.Add(TriggerAction.TYPE.PickUp);
			actions.Add(TriggerAction.TYPE.Expand);
		}
		else if (this.m_Info.m_Harvestable || this.m_Info.m_Eatable || this.m_Info.CanDrink() || this.m_Info.m_Craftable)
		{
			actions.Add(TriggerAction.TYPE.Expand);
		}
	}

	public override bool CanTrigger()
	{
		return !this.m_InPlayersHands && !SwimController.Get().IsActive() && base.CanTrigger();
	}

	public override bool CanExecuteActions()
	{
		if (!base.CanExecuteActions())
		{
			return false;
		}
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (!currentItem)
		{
			return true;
		}
		HeavyObject heavyObject = null;
		if (currentItem.m_Info.IsHeavyObject())
		{
			heavyObject = (HeavyObject)currentItem;
		}
		return (!heavyObject || currentItem.m_Info.m_ID == this.m_Info.m_ID) && (!heavyObject || heavyObject.FindFreeSlot());
	}

	public override void UpdatePhx()
	{
		bool flag = this.m_PhxStaticRequests > 0;
		if (!flag)
		{
			flag = (CraftingManager.Get().ContainsItem(this) || (base.gameObject.transform.parent != null && (base.gameObject.transform.parent.GetComponent<PlayerHolder>() != null || base.gameObject.transform.parent.name.Contains("Attach"))));
		}
		if (this.m_BoxCollider)
		{
			this.m_BoxCollider.isTrigger = flag;
			this.m_BoxCollider.enabled = (this.m_InInventory || this.m_OnCraftingTable || !flag);
		}
		if (this.m_Rigidbody)
		{
			this.m_Rigidbody.isKinematic = flag;
		}
	}

	public override void OnItemAttachedToHand()
	{
		base.OnItemAttachedToHand();
		this.m_InPlayersHands = true;
	}

	public override void OnItemDetachedFromHand()
	{
		base.OnItemDetachedFromHand();
		this.m_InPlayersHands = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (this.m_FallenObject && Time.time - this.m_FallenObjectCreationTime < 20f)
		{
			return;
		}
		if (collision.relativeVelocity.magnitude > 1f && this.m_CollisionClips.Count > 0)
		{
			if (this.m_AudioSource == null)
			{
				this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
				this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
				this.m_AudioSource.spatialBlend = 1f;
				this.m_AudioSource.minDistance = 1f;
				this.m_AudioSource.maxDistance = 10f;
				this.m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
			}
			this.m_AudioSource.PlayOneShot(this.m_CollisionClips[UnityEngine.Random.Range(0, this.m_CollisionClips.Count)]);
		}
	}

	protected override void Update()
	{
		base.Update();
		foreach (KeyValuePair<Transform, HeavyObject> keyValuePair in this.m_Attached)
		{
			if (keyValuePair.Value != null)
			{
				keyValuePair.Value.transform.position = keyValuePair.Key.transform.position;
				keyValuePair.Value.transform.rotation = keyValuePair.Key.transform.rotation;
			}
		}
	}

	public Dictionary<Transform, HeavyObject> m_Attached = new Dictionary<Transform, HeavyObject>();

	private List<Transform> m_DummiesToAttach = new List<Transform>();

	public int m_NumObjectsToAttach;

	[HideInInspector]
	public bool m_InPlayersHands;

	private List<AudioClip> m_CollisionClips = new List<AudioClip>();

	private AudioSource m_AudioSource;
}
