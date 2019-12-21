using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Torch : Weapon
{
	protected override void Awake()
	{
		base.Awake();
		this.SetupAudio();
	}

	protected override void Start()
	{
		base.Start();
		Light[] componentsInChildren = base.GetComponentsInChildren<Light>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			TorchLightData torchLightData = new TorchLightData();
			torchLightData.m_Light = componentsInChildren[i];
			torchLightData.m_Light.gameObject.SetActive(false);
			torchLightData.m_DefaultRange = torchLightData.m_Light.range;
			torchLightData.m_DefaultIntensity = torchLightData.m_Light.intensity;
			this.m_Lights.Add(torchLightData);
		}
		ParticleSystem[] componentsInChildren2 = base.GetComponentsInChildren<ParticleSystem>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			this.m_Particles.Add(componentsInChildren2[j]);
			this.m_ParticlesRenderers.Add(componentsInChildren2[j].GetComponent<ParticleSystemRenderer>());
			if (!this.m_ParticlesParent)
			{
				this.m_ParticlesParent = componentsInChildren2[j].transform.parent;
				this.m_ParticlesParentLocPos = this.m_ParticlesParent.localPosition;
				this.m_ParticlesParent.parent = null;
			}
		}
		this.m_WasInBackPackLastFrame = InventoryBackpack.Get().Contains(this);
		this.SetupEffects();
		if (this.m_DebugIgniteOnStart)
		{
			this.m_Fuel = 1f;
			this.StartBurning();
		}
	}

	private void SetupAudio()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		if (this.m_AudioSource2 == null)
		{
			this.m_AudioSource2 = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource2.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		if (Torch.s_AudioClipsLoaded)
		{
			return;
		}
		Torch.s_IgniteAudioClips[296] = new List<AudioClip>();
		AudioClip item = (AudioClip)Resources.Load("Sounds/Weapon/torch_ignite_01");
		Torch.s_IgniteAudioClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_ignite_02");
		Torch.s_IgniteAudioClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_ignite_03");
		Torch.s_IgniteAudioClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_ignite_04");
		Torch.s_IgniteAudioClips[296].Add(item);
		Torch.s_IgniteAudioClips[297] = Torch.s_IgniteAudioClips[296];
		Torch.s_IgniteAudioClips[295] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_ignite_01");
		Torch.s_IgniteAudioClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_ignite_02");
		Torch.s_IgniteAudioClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_ignite_03");
		Torch.s_IgniteAudioClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_ignite_04");
		Torch.s_IgniteAudioClips[295].Add(item);
		Torch.s_BurningClips[296] = (AudioClip)Resources.Load("Sounds/Weapon/torch_loop");
		Torch.s_BurningClips[297] = Torch.s_BurningClips[296];
		Torch.s_BurningClips[295] = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_loop");
		Torch.s_TakeOutClips[296] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_put_out_01");
		Torch.s_TakeOutClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_put_out_02");
		Torch.s_TakeOutClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_put_out_03");
		Torch.s_TakeOutClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_put_out_04");
		Torch.s_TakeOutClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_put_out_05");
		Torch.s_TakeOutClips[296].Add(item);
		Torch.s_TakeOutClips[297] = Torch.s_TakeOutClips[296];
		Torch.s_TakeOutClips[295] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_put_out_01");
		Torch.s_TakeOutClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_put_out_02");
		Torch.s_TakeOutClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_put_out_03");
		Torch.s_TakeOutClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_put_out_04");
		Torch.s_TakeOutClips[295].Add(item);
		Torch.s_SwingClips[296] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_whoosh_01");
		Torch.s_SwingClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_whoosh_02");
		Torch.s_SwingClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_whoosh_03");
		Torch.s_SwingClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_whoosh_04");
		Torch.s_SwingClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_whoosh_05");
		Torch.s_SwingClips[296].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_whoosh_06");
		Torch.s_SwingClips[296].Add(item);
		Torch.s_SwingClips[297] = Torch.s_SwingClips[296];
		Torch.s_SwingClips[295] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_whoosh_01");
		Torch.s_SwingClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_whoosh_02");
		Torch.s_SwingClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_whoosh_03");
		Torch.s_SwingClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_whoosh_04");
		Torch.s_SwingClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_whoosh_05");
		Torch.s_SwingClips[295].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/torch_weak_whoosh_06");
		Torch.s_SwingClips[295].Add(item);
		Torch.s_AudioClipsLoaded = true;
	}

	public void StartBurning()
	{
		if (this.m_Burning)
		{
			return;
		}
		this.m_Burning = true;
		this.m_StartBurningTime = Time.time - this.m_BurningDuration;
		this.SetupEffects();
		this.PlayIgniteSound();
	}

	public void Extinguish()
	{
		this.m_Burning = false;
		this.SetupEffects();
		this.m_AudioSource.Stop();
	}

	private void SetupEffects()
	{
		for (int i = 0; i < this.m_Lights.Count; i++)
		{
			this.m_Lights[i].m_Light.gameObject.SetActive(this.m_Burning);
		}
		for (int j = 0; j < this.m_Particles.Count; j++)
		{
			this.m_Particles[j].gameObject.SetActive(this.m_Burning);
		}
		for (int k = 0; k < this.m_ParticlesRenderers.Count; k++)
		{
			this.m_ParticlesRenderers[k].enabled = this.m_Burning;
		}
		if (!this.m_Renderer)
		{
			this.m_Renderer = base.gameObject.GetComponent<Renderer>();
		}
		this.m_Renderer.materials = (this.m_Burning ? this.m_EmissionMaterials.ToArray() : this.m_NoEmissionMaterials.ToArray());
	}

	public override void UpdateHealth()
	{
		if (!this.ReplIsOwner())
		{
			return;
		}
		this.m_Info.m_Health = this.m_Fuel * this.m_Info.m_MaxHealth;
		if (this.m_Fuel == 0f)
		{
			base.UpdateHealth();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.IsInWater())
		{
			this.Extinguish();
		}
		this.CheckRain();
		this.UpdateBurning();
		this.UpdateLightNoise();
		this.m_WasInBackPackLastFrame = InventoryBackpack.Get().Contains(this);
	}

	private void CheckRain()
	{
		if (!this.m_Burning)
		{
			return;
		}
		if (RainManager.Get().IsRain() && Time.time - this.m_StartBurningTime > 1f && !RainManager.Get().IsInRainCutter(base.transform.position))
		{
			this.Extinguish();
		}
	}

	private void UpdateBurning()
	{
		if (!this.m_Burning || this.m_DebugInfiniteBurn)
		{
			return;
		}
		this.m_BurningDuration += Time.deltaTime;
		float num = ((TorchInfo)this.m_Info).m_BurningDurationInMinutes * 60f;
		this.m_Fuel = (num - (Time.time - this.m_StartBurningTime)) / num;
		if (this.m_Fuel <= 0f)
		{
			this.m_Fuel = 0f;
			this.Extinguish();
		}
		this.m_Info.m_Health -= ((TorchInfo)this.m_Info).m_DamageWhenBurning;
		if (this.m_IgniteSoundPlaying && !this.m_AudioSource.isPlaying)
		{
			this.PlayBurningSound();
			this.m_IgniteSoundPlaying = false;
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (this.m_Burning)
		{
			this.m_ParticlesParent.rotation = base.transform.rotation;
			this.m_ParticlesParent.position = base.transform.position + base.transform.TransformDirection(this.m_ParticlesParentLocPos);
		}
	}

	private void UpdateLightNoise()
	{
		if (!this.m_Burning)
		{
			return;
		}
		float num = (Mathf.Sin(Time.time * this.m_LightNoiseRangeSpeed) + Mathf.Sin(Time.time * this.m_LightNoiseRangeSpeed * 1.6f)) * this.m_LightNoiseRange;
		float num2 = (Mathf.Sin(Time.time * this.m_LightNoiseIntensitySpeed) + Mathf.Sin(Time.time * this.m_LightNoiseIntensitySpeed * 1.6f)) * this.m_LightNoiseIntensity;
		for (int i = 0; i < this.m_Lights.Count; i++)
		{
			this.m_Lights[i].m_Light.range = this.m_Lights[i].m_DefaultRange + num;
			this.m_Lights[i].m_Light.intensity = this.m_Lights[i].m_DefaultIntensity + num2;
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("TorchBurn" + index, this.m_Burning);
		SaveGame.SaveVal("TorchSTime" + index, this.m_StartBurningTime);
		SaveGame.SaveVal("TorchFuel" + index, this.m_Fuel);
		SaveGame.SaveVal("TorchBurningDuration" + index, this.m_BurningDuration);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_Burning = SaveGame.LoadBVal("TorchBurn" + index);
		this.m_StartBurningTime = SaveGame.LoadFVal("TorchSTime" + index);
		this.m_Fuel = SaveGame.LoadFVal("TorchFuel" + index);
		if (GreenHellGame.s_GameVersion >= GreenHellGame.s_GameVersionMasterShelters1_3)
		{
			this.m_BurningDuration = SaveGame.LoadFVal("TorchBurningDuration" + index);
		}
		else
		{
			this.m_BurningDuration = 0f;
		}
		this.SetupEffects();
	}

	public override void CheckIfInBackPack()
	{
		if (!this.m_Burning)
		{
			return;
		}
		if (!InventoryBackpack.Get().Contains(this))
		{
			if (InventoryBackpack.Get().Contains(this) != this.m_WasInBackPackLastFrame)
			{
				for (int i = 0; i < this.m_Lights.Count; i++)
				{
					this.m_Lights[i].m_Light.gameObject.SetActive(this.m_Burning);
				}
			}
			for (int j = 0; j < this.m_ParticlesRenderers.Count; j++)
			{
				this.m_ParticlesRenderers[j].enabled = this.m_Burning;
			}
			return;
		}
		if (Inventory3DManager.Get().m_ActivePocket != BackpackPocket.Left)
		{
			for (int k = 0; k < this.m_Lights.Count; k++)
			{
				this.m_Lights[k].m_Light.gameObject.SetActive(false);
			}
			for (int l = 0; l < this.m_ParticlesRenderers.Count; l++)
			{
				this.m_ParticlesRenderers[l].enabled = false;
			}
			return;
		}
		for (int m = 0; m < this.m_Lights.Count; m++)
		{
			this.m_Lights[m].m_Light.gameObject.SetActive(this.m_Burning);
		}
		for (int n = 0; n < this.m_ParticlesRenderers.Count; n++)
		{
			this.m_ParticlesRenderers[n].enabled = this.m_Burning;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.m_ParticlesParent)
		{
			this.m_ParticlesParent.gameObject.SetActive(true);
		}
		this.SetupEffects();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_ParticlesParent)
		{
			this.m_ParticlesParent.gameObject.SetActive(false);
		}
	}

	private void PlayIgniteSound()
	{
		List<AudioClip> list = null;
		if (!Torch.s_IgniteAudioClips.TryGetValue((int)this.m_Info.m_ID, out list))
		{
			return;
		}
		if (list.Count == 0)
		{
			return;
		}
		AudioClip clip = list[UnityEngine.Random.Range(0, list.Count)];
		this.m_AudioSource.loop = false;
		this.m_AudioSource.PlayOneShot(clip);
		this.m_IgniteSoundPlaying = true;
	}

	private void PlayBurningSound()
	{
		AudioClip clip = null;
		if (!Torch.s_BurningClips.TryGetValue((int)this.m_Info.m_ID, out clip))
		{
			return;
		}
		this.m_AudioSource.loop = true;
		this.m_AudioSource.clip = clip;
		this.m_AudioSource.Play();
	}

	public void PlayTakeOutSound()
	{
		List<AudioClip> list = null;
		if (!Torch.s_TakeOutClips.TryGetValue((int)this.m_Info.m_ID, out list))
		{
			return;
		}
		if (list.Count == 0)
		{
			return;
		}
		AudioClip clip = list[UnityEngine.Random.Range(0, list.Count)];
		this.m_AudioSource2.loop = false;
		this.m_AudioSource2.clip = clip;
		this.m_AudioSource2.PlayOneShot(clip);
	}

	public void PlaySwingSound()
	{
		List<AudioClip> list = null;
		if (!Torch.s_SwingClips.TryGetValue((int)this.m_Info.m_ID, out list))
		{
			return;
		}
		if (list.Count == 0)
		{
			return;
		}
		AudioClip clip = list[UnityEngine.Random.Range(0, list.Count)];
		this.m_AudioSource2.loop = false;
		this.m_AudioSource2.clip = clip;
		this.m_AudioSource2.PlayOneShot(clip);
	}

	public override void OnStartSwing()
	{
		base.OnStartSwing();
		if (this.m_Burning)
		{
			this.PlaySwingSound();
		}
	}

	[HideInInspector]
	public bool m_Burning;

	private bool m_WasInBackPackLastFrame;

	private float m_StartBurningTime;

	private float m_BurningDuration;

	[HideInInspector]
	public float m_Fuel = 1f;

	private List<TorchLightData> m_Lights = new List<TorchLightData>();

	private List<ParticleSystem> m_Particles = new List<ParticleSystem>();

	private List<ParticleSystemRenderer> m_ParticlesRenderers = new List<ParticleSystemRenderer>();

	public float m_LightNoiseRangeSpeed = 20f;

	public float m_LightNoiseRange = 0.1f;

	public float m_LightNoiseIntensitySpeed = 20f;

	public float m_LightNoiseIntensity = 0.1f;

	private Transform m_ParticlesParent;

	private Vector3 m_ParticlesParentLocPos = Vector3.zero;

	public bool m_DebugIgnite;

	public bool m_DebugIgniteOnStart;

	public bool m_DebugInfiniteBurn;

	public List<Material> m_EmissionMaterials;

	public List<Material> m_NoEmissionMaterials;

	private Renderer m_Renderer;

	private AudioSource m_AudioSource;

	private AudioSource m_AudioSource2;

	private static bool s_AudioClipsLoaded = false;

	private static Dictionary<int, List<AudioClip>> s_IgniteAudioClips = new Dictionary<int, List<AudioClip>>();

	private static Dictionary<int, AudioClip> s_BurningClips = new Dictionary<int, AudioClip>();

	private static Dictionary<int, List<AudioClip>> s_TakeOutClips = new Dictionary<int, List<AudioClip>>();

	private static Dictionary<int, List<AudioClip>> s_SwingClips = new Dictionary<int, List<AudioClip>>();

	private bool m_IgniteSoundPlaying;
}
