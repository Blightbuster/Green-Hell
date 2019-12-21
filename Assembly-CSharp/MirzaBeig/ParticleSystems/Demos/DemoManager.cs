using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirzaBeig.ParticleSystems.Demos
{
	public class DemoManager : MonoBehaviour
	{
		private void Awake()
		{
			this.loopingParticleSystems = UnityEngine.Object.FindObjectOfType<LoopingParticleSystemsManager>();
			this.oneshotParticleSystems = UnityEngine.Object.FindObjectOfType<OneshotParticleSystemsManager>();
			this.loopingParticleSystems.Init();
			this.oneshotParticleSystems.Init();
		}

		private void Start()
		{
			this.cameraPositionStart = this.cameraTranslationTransform.localPosition;
			this.cameraRotationStart = this.cameraRotationTransform.localEulerAngles;
			this.ResetCameraTransformTargets();
			DemoManager.ParticleMode particleMode = this.particleMode;
			if (particleMode != DemoManager.ParticleMode.looping)
			{
				if (particleMode != DemoManager.ParticleMode.oneshot)
				{
					MonoBehaviour.print("Unknown case.");
				}
				else
				{
					this.SetToOneshotParticleMode(true);
					this.loopingParticleModeToggle.isOn = false;
					this.oneshotParticleModeToggle.isOn = true;
				}
			}
			else
			{
				this.SetToLoopingParticleMode(true);
				this.loopingParticleModeToggle.isOn = true;
				this.oneshotParticleModeToggle.isOn = false;
			}
			this.SetAdvancedRendering(this.advancedRendering);
			this.advancedRenderingToggle.isOn = this.advancedRendering;
			this.levelToggles = this.levelTogglesContainer.GetComponentsInChildren<Toggle>(true);
			for (int i = 0; i < this.levels.Length; i++)
			{
				if (i == (int)this.currentLevel)
				{
					this.levels[i].SetActive(true);
					this.levelToggles[i].isOn = true;
				}
				else
				{
					this.levels[i].SetActive(false);
					this.levelToggles[i].isOn = false;
				}
			}
			this.UpdateCurrentParticleSystemNameText();
			this.timeScaleSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnTimeScaleSliderValueChanged));
			this.OnTimeScaleSliderValueChanged(this.timeScaleSlider.value);
		}

		public void OnTimeScaleSliderValueChanged(float value)
		{
			Time.timeScale = value;
			this.timeScaleSliderValueText.text = value.ToString("0.00");
		}

		public void SetToLoopingParticleMode(bool set)
		{
			if (set)
			{
				this.oneshotParticleSystems.Clear();
				this.loopingParticleSystems.gameObject.SetActive(true);
				this.oneshotParticleSystems.gameObject.SetActive(false);
				this.particleSpawnInstructionText.gameObject.SetActive(false);
				this.particleMode = DemoManager.ParticleMode.looping;
				this.UpdateCurrentParticleSystemNameText();
			}
		}

		public void SetToOneshotParticleMode(bool set)
		{
			if (set)
			{
				this.loopingParticleSystems.gameObject.SetActive(false);
				this.oneshotParticleSystems.gameObject.SetActive(true);
				this.particleSpawnInstructionText.gameObject.SetActive(true);
				this.particleMode = DemoManager.ParticleMode.oneshot;
				this.UpdateCurrentParticleSystemNameText();
			}
		}

		public void SetLevel(DemoManager.Level level)
		{
			for (int i = 0; i < this.levels.Length; i++)
			{
				if (i == (int)level)
				{
					this.levels[i].SetActive(true);
				}
				else
				{
					this.levels[i].SetActive(false);
				}
			}
			this.currentLevel = level;
		}

		public void SetLevelFromToggle(Toggle toggle)
		{
			if (toggle.isOn)
			{
				this.SetLevel((DemoManager.Level)Array.IndexOf<Toggle>(this.levelToggles, toggle));
			}
		}

		public void SetAdvancedRendering(bool value)
		{
			this.advancedRendering = value;
			this.mainCamera.allowHDR = value;
			if (value)
			{
				QualitySettings.SetQualityLevel(32, true);
				this.mainCamera.renderingPath = RenderingPath.UsePlayerSettings;
				this.mouse.gameObject.SetActive(true);
			}
			else
			{
				QualitySettings.SetQualityLevel(0, true);
				this.mainCamera.renderingPath = RenderingPath.VertexLit;
				this.mouse.gameObject.SetActive(false);
			}
			for (int i = 0; i < this.mainCameraPostEffects.Length; i++)
			{
				if (this.mainCameraPostEffects[i])
				{
					this.mainCameraPostEffects[i].enabled = value;
				}
			}
		}

		public static Vector3 DampVector3(Vector3 from, Vector3 to, float speed, float dt)
		{
			return Vector3.Lerp(from, to, 1f - Mathf.Exp(-speed * dt));
		}

		private void Update()
		{
			this.input.x = Input.GetAxis("Horizontal");
			this.input.y = Input.GetAxis("Vertical");
			if (Input.GetKey(KeyCode.LeftShift))
			{
				this.targetCameraPosition.z = this.targetCameraPosition.z + this.input.y * this.cameraMoveAmount;
				this.targetCameraPosition.z = Mathf.Clamp(this.targetCameraPosition.z, -6.3f, -1f);
			}
			else
			{
				this.targetCameraRotation.y = this.targetCameraRotation.y + this.input.x * this.cameraRotateAmount;
				this.targetCameraRotation.x = this.targetCameraRotation.x + this.input.y * this.cameraRotateAmount;
				this.targetCameraRotation.x = Mathf.Clamp(this.targetCameraRotation.x, this.cameraAngleLimits.x, this.cameraAngleLimits.y);
			}
			this.cameraTranslationTransform.localPosition = Vector3.SmoothDamp(this.cameraTranslationTransform.localPosition, this.targetCameraPosition, ref this.cameraPositionSmoothDampVelocity, 1f / this.cameraMoveSpeed, float.PositiveInfinity, Time.unscaledDeltaTime);
			this.cameraRotation = Vector3.SmoothDamp(this.cameraRotation, this.targetCameraRotation, ref this.cameraRotationSmoothDampVelocity, 1f / this.cameraRotationSpeed, float.PositiveInfinity, Time.unscaledDeltaTime);
			this.cameraRotationTransform.localEulerAngles = this.cameraRotation;
			this.cameraTranslationTransform.LookAt(this.cameraLookAtPosition);
			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				this.Next();
			}
			else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				this.Previous();
			}
			if (Input.GetKeyDown(KeyCode.U))
			{
				this.ui.SetActive(!this.ui.activeSelf);
			}
			if (Input.GetKeyDown(KeyCode.O))
			{
				if (this.particleMode == DemoManager.ParticleMode.looping)
				{
					this.SetToOneshotParticleMode(true);
				}
				else
				{
					this.SetToLoopingParticleMode(true);
				}
			}
			if (Input.GetKeyDown(KeyCode.L))
			{
				this.SetLevel((this.currentLevel + 1) % (DemoManager.Level)Enum.GetNames(typeof(DemoManager.Level)).Length);
			}
			else
			{
				Input.GetKey(KeyCode.R);
			}
			if (this.particleMode == DemoManager.ParticleMode.oneshot)
			{
				Vector3 mousePosition = Input.mousePosition;
				if (Input.GetMouseButtonDown(0))
				{
					CameraShake cameraShake = UnityEngine.Object.FindObjectOfType<CameraShake>();
					cameraShake.Add(0.2f, 5f, 0.2f, CameraShakeTarget.Position, CameraShakeAmplitudeCurve.FadeInOut25);
					cameraShake.Add(4f, 5f, 0.5f, CameraShakeTarget.Rotation, CameraShakeAmplitudeCurve.FadeInOut25);
					this.oneshotParticleSystems.InstantiateParticlePrefab(mousePosition, this.mouse.distanceFromCamera);
				}
				if (Input.GetMouseButton(1))
				{
					this.oneshotParticleSystems.InstantiateParticlePrefab(mousePosition, this.mouse.distanceFromCamera);
				}
			}
			if (Input.GetKeyDown(KeyCode.R))
			{
				this.ResetCameraTransformTargets();
			}
		}

		private void LateUpdate()
		{
			this.particleCountText.text = "PARTICLE COUNT: ";
			if (this.particleMode == DemoManager.ParticleMode.looping)
			{
				Text text = this.particleCountText;
				text.text += this.loopingParticleSystems.GetParticleCount().ToString();
				return;
			}
			if (this.particleMode == DemoManager.ParticleMode.oneshot)
			{
				Text text2 = this.particleCountText;
				text2.text += this.oneshotParticleSystems.GetParticleCount().ToString();
			}
		}

		private void ResetCameraTransformTargets()
		{
			this.targetCameraPosition = this.cameraPositionStart;
			this.targetCameraRotation = this.cameraRotationStart;
		}

		private void UpdateCurrentParticleSystemNameText()
		{
			if (this.particleMode == DemoManager.ParticleMode.looping)
			{
				this.currentParticleSystemText.text = this.loopingParticleSystems.GetCurrentPrefabName(true);
				return;
			}
			if (this.particleMode == DemoManager.ParticleMode.oneshot)
			{
				this.currentParticleSystemText.text = this.oneshotParticleSystems.GetCurrentPrefabName(true);
			}
		}

		public void Next()
		{
			if (this.particleMode == DemoManager.ParticleMode.looping)
			{
				this.loopingParticleSystems.Next();
			}
			else if (this.particleMode == DemoManager.ParticleMode.oneshot)
			{
				this.oneshotParticleSystems.Next();
			}
			this.UpdateCurrentParticleSystemNameText();
		}

		public void Previous()
		{
			if (this.particleMode == DemoManager.ParticleMode.looping)
			{
				this.loopingParticleSystems.Previous();
			}
			else if (this.particleMode == DemoManager.ParticleMode.oneshot)
			{
				this.oneshotParticleSystems.Previous();
			}
			this.UpdateCurrentParticleSystemNameText();
		}

		public Transform cameraRotationTransform;

		public Transform cameraTranslationTransform;

		public Vector3 cameraLookAtPosition = new Vector3(0f, 3f, 0f);

		public MouseFollow mouse;

		private Vector3 targetCameraPosition;

		private Vector3 targetCameraRotation;

		private Vector3 cameraPositionStart;

		private Vector3 cameraRotationStart;

		private Vector2 input;

		private Vector3 cameraRotation;

		public float cameraMoveAmount = 2f;

		public float cameraRotateAmount = 2f;

		public float cameraMoveSpeed = 12f;

		public float cameraRotationSpeed = 12f;

		public Vector2 cameraAngleLimits = new Vector2(-8f, 60f);

		public GameObject[] levels;

		public DemoManager.Level currentLevel = DemoManager.Level.basic;

		public DemoManager.ParticleMode particleMode;

		public bool advancedRendering = true;

		public Toggle loopingParticleModeToggle;

		public Toggle oneshotParticleModeToggle;

		public Toggle advancedRenderingToggle;

		private Toggle[] levelToggles;

		public ToggleGroup levelTogglesContainer;

		private LoopingParticleSystemsManager loopingParticleSystems;

		private OneshotParticleSystemsManager oneshotParticleSystems;

		public GameObject ui;

		public Text particleCountText;

		public Text currentParticleSystemText;

		public Text particleSpawnInstructionText;

		public Slider timeScaleSlider;

		public Text timeScaleSliderValueText;

		public Camera mainCamera;

		public MonoBehaviour[] mainCameraPostEffects;

		private Vector3 cameraPositionSmoothDampVelocity;

		private Vector3 cameraRotationSmoothDampVelocity;

		public enum ParticleMode
		{
			looping,
			oneshot
		}

		public enum Level
		{
			none,
			basic
		}
	}
}
