using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Cinemachine.Timeline
{
	public sealed class CinemachineMixer : PlayableBehaviour
	{
		public override void OnGraphStop(Playable playable)
		{
			if (this.mBrain != null)
			{
				this.mBrain.ReleaseCameraOverride(this.mBrainOverrideId);
			}
			this.mBrainOverrideId = -1;
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			base.ProcessFrame(playable, info, playerData);
			GameObject gameObject = playerData as GameObject;
			if (gameObject == null)
			{
				this.mBrain = (CinemachineBrain)playerData;
			}
			else
			{
				this.mBrain = gameObject.GetComponent<CinemachineBrain>();
			}
			if (this.mBrain == null)
			{
				return;
			}
			int num = 0;
			ICinemachineCamera cinemachineCamera = null;
			ICinemachineCamera camA = null;
			float weightB = 1f;
			for (int i = 0; i < playable.GetInputCount<Playable>(); i++)
			{
				CinemachineShotPlayable behaviour = ((ScriptPlayable<T>)playable.GetInput(i)).GetBehaviour();
				float inputWeight = playable.GetInputWeight(i);
				if (behaviour != null && behaviour.VirtualCamera != null && playable.GetPlayState<Playable>() == PlayState.Playing && inputWeight > 0.0001f)
				{
					if (num == 1)
					{
						camA = cinemachineCamera;
					}
					weightB = inputWeight;
					cinemachineCamera = behaviour.VirtualCamera;
					num++;
					if (num == 2)
					{
						break;
					}
				}
			}
			float deltaTime = info.deltaTime;
			if (!this.mPlaying)
			{
				if (this.mBrainOverrideId < 0)
				{
					this.mLastOverrideFrame = -1f;
				}
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				deltaTime = Time.unscaledDeltaTime;
				if (!Application.isPlaying && (this.mLastOverrideFrame < 0f || realtimeSinceStartup - this.mLastOverrideFrame > Time.maximumDeltaTime))
				{
					deltaTime = -1f;
				}
				this.mLastOverrideFrame = realtimeSinceStartup;
			}
			this.mBrainOverrideId = this.mBrain.SetCameraOverride(this.mBrainOverrideId, camA, cinemachineCamera, weightB, deltaTime);
		}

		public override void PrepareFrame(Playable playable, FrameData info)
		{
			this.mPlaying = (info.evaluationType == FrameData.EvaluationType.Playback);
		}

		private CinemachineBrain mBrain;

		private int mBrainOverrideId = -1;

		private bool mPlaying;

		private float mLastOverrideFrame;
	}
}
