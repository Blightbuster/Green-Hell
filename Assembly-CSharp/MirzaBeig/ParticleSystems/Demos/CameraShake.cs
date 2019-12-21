using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirzaBeig.ParticleSystems.Demos
{
	public class CameraShake : MonoBehaviour
	{
		private void Start()
		{
		}

		public void Add(float amplitude, float frequency, float duration, CameraShakeTarget target, AnimationCurve amplitudeOverLifetimeCurve)
		{
			this.shakes.Add(new CameraShake.Shake(amplitude, frequency, duration, target, amplitudeOverLifetimeCurve));
		}

		public void Add(float amplitude, float frequency, float duration, CameraShakeTarget target, CameraShakeAmplitudeCurve amplitudeOverLifetimeCurve)
		{
			this.shakes.Add(new CameraShake.Shake(amplitude, frequency, duration, target, amplitudeOverLifetimeCurve));
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F))
			{
				this.Add(0.25f, 1f, 2f, CameraShakeTarget.Position, CameraShakeAmplitudeCurve.FadeInOut25);
			}
			if (Input.GetKeyDown(KeyCode.G))
			{
				this.Add(15f, 1f, 2f, CameraShakeTarget.Rotation, CameraShakeAmplitudeCurve.FadeInOut25);
			}
			Input.GetKey(KeyCode.H);
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			for (int i = 0; i < this.shakes.Count; i++)
			{
				this.shakes[i].Update();
				if (this.shakes[i].target == CameraShakeTarget.Position)
				{
					vector += this.shakes[i].noise;
				}
				else
				{
					vector2 += this.shakes[i].noise;
				}
			}
			this.shakes.RemoveAll((CameraShake.Shake x) => !x.IsAlive());
			base.transform.localPosition = Vector3.SmoothDamp(base.transform.localPosition, vector, ref this.smoothDampPositionVelocity, this.smoothDampTime);
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			localEulerAngles.x = Mathf.SmoothDampAngle(localEulerAngles.x, vector2.x, ref this.smoothDampRotationVelocityX, this.smoothDampTime);
			localEulerAngles.y = Mathf.SmoothDampAngle(localEulerAngles.y, vector2.y, ref this.smoothDampRotationVelocityY, this.smoothDampTime);
			localEulerAngles.z = Mathf.SmoothDampAngle(localEulerAngles.z, vector2.z, ref this.smoothDampRotationVelocityZ, this.smoothDampTime);
			base.transform.localEulerAngles = localEulerAngles;
		}

		public float smoothDampTime = 0.025f;

		private Vector3 smoothDampPositionVelocity;

		private float smoothDampRotationVelocityX;

		private float smoothDampRotationVelocityY;

		private float smoothDampRotationVelocityZ;

		private List<CameraShake.Shake> shakes = new List<CameraShake.Shake>();

		[Serializable]
		public class Shake
		{
			public void Init()
			{
				this.timeRemaining = this.duration;
				this.ApplyRandomSeed();
			}

			private void Init(float amplitude, float frequency, float duration, CameraShakeTarget target)
			{
				this.amplitude = amplitude;
				this.frequency = frequency;
				this.duration = duration;
				this.timeRemaining = duration;
				this.target = target;
				this.ApplyRandomSeed();
			}

			public void ApplyRandomSeed()
			{
				float num = 32f;
				this.perlinNoiseX.x = UnityEngine.Random.Range(-num, num);
				this.perlinNoiseX.y = UnityEngine.Random.Range(-num, num);
				this.perlinNoiseY.x = UnityEngine.Random.Range(-num, num);
				this.perlinNoiseY.y = UnityEngine.Random.Range(-num, num);
				this.perlinNoiseZ.x = UnityEngine.Random.Range(-num, num);
				this.perlinNoiseZ.y = UnityEngine.Random.Range(-num, num);
			}

			public Shake(float amplitude, float frequency, float duration, CameraShakeTarget target, AnimationCurve amplitudeOverLifetimeCurve)
			{
				this.Init(amplitude, frequency, duration, target);
				this.amplitudeOverLifetimeCurve = amplitudeOverLifetimeCurve;
			}

			public Shake(float amplitude, float frequency, float duration, CameraShakeTarget target, CameraShakeAmplitudeCurve amplitudeOverLifetimeCurve)
			{
				this.Init(amplitude, frequency, duration, target);
				switch (amplitudeOverLifetimeCurve)
				{
				case CameraShakeAmplitudeCurve.Constant:
					this.amplitudeOverLifetimeCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
					return;
				case CameraShakeAmplitudeCurve.FadeInOut25:
					this.amplitudeOverLifetimeCurve = new AnimationCurve(new Keyframe[]
					{
						new Keyframe(0f, 0f),
						new Keyframe(0.25f, 1f),
						new Keyframe(1f, 0f)
					});
					return;
				case CameraShakeAmplitudeCurve.FadeInOut50:
					this.amplitudeOverLifetimeCurve = new AnimationCurve(new Keyframe[]
					{
						new Keyframe(0f, 0f),
						new Keyframe(0.5f, 1f),
						new Keyframe(1f, 0f)
					});
					return;
				case CameraShakeAmplitudeCurve.FadeInOut75:
					this.amplitudeOverLifetimeCurve = new AnimationCurve(new Keyframe[]
					{
						new Keyframe(0f, 0f),
						new Keyframe(0.75f, 1f),
						new Keyframe(1f, 0f)
					});
					return;
				default:
					throw new Exception("Unknown enum.");
				}
			}

			public bool IsAlive()
			{
				return this.timeRemaining > 0f;
			}

			public void Update()
			{
				if (this.timeRemaining < 0f)
				{
					return;
				}
				Vector2 b = Time.deltaTime * new Vector2(this.frequency, this.frequency);
				this.perlinNoiseX += b;
				this.perlinNoiseY += b;
				this.perlinNoiseZ += b;
				this.noise.x = Mathf.PerlinNoise(this.perlinNoiseX.x, this.perlinNoiseX.y) - 0.5f;
				this.noise.y = Mathf.PerlinNoise(this.perlinNoiseY.x, this.perlinNoiseY.y) - 0.5f;
				this.noise.z = Mathf.PerlinNoise(this.perlinNoiseZ.x, this.perlinNoiseZ.y) - 0.5f;
				float num = this.amplitudeOverLifetimeCurve.Evaluate(1f - this.timeRemaining / this.duration);
				this.noise *= this.amplitude * num;
				this.timeRemaining -= Time.deltaTime;
			}

			public float amplitude = 1f;

			public float frequency = 1f;

			public float duration;

			[HideInInspector]
			public CameraShakeTarget target;

			private float timeRemaining;

			private Vector2 perlinNoiseX;

			private Vector2 perlinNoiseY;

			private Vector2 perlinNoiseZ;

			[HideInInspector]
			public Vector3 noise;

			public AnimationCurve amplitudeOverLifetimeCurve = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0f, 1f),
				new Keyframe(1f, 0f)
			});
		}
	}
}
