using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Cinemachine
{
	public sealed class CinemachineCore
	{
		public static CinemachineCore Instance
		{
			get
			{
				if (CinemachineCore.sInstance == null)
				{
					CinemachineCore.sInstance = new CinemachineCore();
				}
				return CinemachineCore.sInstance;
			}
		}

		public int BrainCount
		{
			get
			{
				return this.mActiveBrains.Count;
			}
		}

		public CinemachineBrain GetActiveBrain(int index)
		{
			return this.mActiveBrains[index];
		}

		internal void AddActiveBrain(CinemachineBrain brain)
		{
			this.RemoveActiveBrain(brain);
			this.mActiveBrains.Insert(0, brain);
		}

		internal void RemoveActiveBrain(CinemachineBrain brain)
		{
			this.mActiveBrains.Remove(brain);
		}

		public int VirtualCameraCount
		{
			get
			{
				return this.mActiveCameras.Count;
			}
		}

		public ICinemachineCamera GetVirtualCamera(int index)
		{
			return this.mActiveCameras[index];
		}

		internal void AddActiveCamera(ICinemachineCamera vcam)
		{
			this.RemoveActiveCamera(vcam);
			int i;
			for (i = 0; i < this.mActiveCameras.Count; i++)
			{
				if (vcam.Priority >= this.mActiveCameras[i].Priority)
				{
					break;
				}
			}
			this.mActiveCameras.Insert(i, vcam);
		}

		internal void RemoveActiveCamera(ICinemachineCamera vcam)
		{
			this.mActiveCameras.Remove(vcam);
		}

		internal void AddChildCamera(ICinemachineCamera vcam)
		{
			this.RemoveChildCamera(vcam);
			int num = 0;
			for (ICinemachineCamera cinemachineCamera = vcam; cinemachineCamera != null; cinemachineCamera = cinemachineCamera.ParentCamera)
			{
				num++;
			}
			while (this.mChildCameras.Count < num)
			{
				this.mChildCameras.Add(new List<ICinemachineCamera>());
			}
			this.mChildCameras[num - 1].Add(vcam);
		}

		internal void RemoveChildCamera(ICinemachineCamera vcam)
		{
			for (int i = 0; i < this.mChildCameras.Count; i++)
			{
				this.mChildCameras[i].Remove(vcam);
			}
		}

		internal void UpdateAllActiveVirtualCameras(Vector3 worldUp, float deltaTime)
		{
			int num;
			for (int i = this.mChildCameras.Count - 1; i >= 0; i--)
			{
				num = this.mChildCameras[i].Count;
				for (int j = 0; j < num; j++)
				{
					this.UpdateVirtualCamera(this.mChildCameras[i][j], worldUp, deltaTime);
				}
			}
			num = this.VirtualCameraCount;
			for (int k = 0; k < num; k++)
			{
				this.UpdateVirtualCamera(this.GetVirtualCamera(k), worldUp, deltaTime);
			}
		}

		internal bool UpdateVirtualCamera(ICinemachineCamera vcam, Vector3 worldUp, float deltaTime)
		{
			int frameCount = Time.frameCount;
			CinemachineCore.UpdateFilter updateFilter = this.CurrentUpdateFilter;
			bool flag = updateFilter != CinemachineCore.UpdateFilter.ForcedFixed && updateFilter != CinemachineCore.UpdateFilter.ForcedLate;
			bool flag2 = updateFilter == CinemachineCore.UpdateFilter.Late;
			if (!flag)
			{
				if (updateFilter == CinemachineCore.UpdateFilter.ForcedFixed)
				{
					updateFilter = CinemachineCore.UpdateFilter.Fixed;
				}
				if (updateFilter == CinemachineCore.UpdateFilter.ForcedLate)
				{
					updateFilter = CinemachineCore.UpdateFilter.Late;
				}
			}
			if (this.mUpdateStatus == null)
			{
				this.mUpdateStatus = new Dictionary<ICinemachineCamera, CinemachineCore.UpdateStatus>();
			}
			if (vcam.VirtualCameraGameObject == null)
			{
				if (this.mUpdateStatus.ContainsKey(vcam))
				{
					this.mUpdateStatus.Remove(vcam);
				}
				return false;
			}
			CinemachineCore.UpdateStatus value;
			if (!this.mUpdateStatus.TryGetValue(vcam, out value))
			{
				value = new CinemachineCore.UpdateStatus(frameCount);
				this.mUpdateStatus.Add(vcam, value);
			}
			int num = (!flag2) ? CinemachineBrain.GetSubframeCount() : 1;
			if (value.lastUpdateFrame != frameCount)
			{
				value.lastUpdateSubframe = 0;
			}
			bool flag3 = !flag;
			if (flag)
			{
				Matrix4x4 pos;
				if (!CinemachineCore.GetTargetPosition(vcam, out pos))
				{
					flag3 = flag2;
				}
				else
				{
					flag3 = (value.ChoosePreferredUpdate(frameCount, pos, updateFilter) == updateFilter);
				}
			}
			if (flag3)
			{
				value.preferredUpdate = updateFilter;
				while (value.lastUpdateSubframe < num)
				{
					vcam.UpdateCameraState(worldUp, deltaTime);
					value.lastUpdateSubframe++;
				}
				value.lastUpdateFrame = frameCount;
			}
			this.mUpdateStatus[vcam] = value;
			return true;
		}

		internal CinemachineCore.UpdateFilter CurrentUpdateFilter { get; set; }

		private static bool GetTargetPosition(ICinemachineCamera vcam, out Matrix4x4 targetPos)
		{
			ICinemachineCamera liveChildOrSelf = vcam.LiveChildOrSelf;
			if (liveChildOrSelf == null || liveChildOrSelf.VirtualCameraGameObject == null)
			{
				targetPos = Matrix4x4.identity;
				return false;
			}
			targetPos = liveChildOrSelf.VirtualCameraGameObject.transform.localToWorldMatrix;
			if (liveChildOrSelf.LookAt != null)
			{
				targetPos = liveChildOrSelf.LookAt.localToWorldMatrix;
				return true;
			}
			if (liveChildOrSelf.Follow != null)
			{
				targetPos = liveChildOrSelf.Follow.localToWorldMatrix;
				return true;
			}
			targetPos = vcam.VirtualCameraGameObject.transform.localToWorldMatrix;
			return true;
		}

		public CinemachineCore.UpdateFilter GetVcamUpdateStatus(ICinemachineCamera vcam)
		{
			CinemachineCore.UpdateStatus updateStatus;
			if (this.mUpdateStatus == null || !this.mUpdateStatus.TryGetValue(vcam, out updateStatus))
			{
				return CinemachineCore.UpdateFilter.Late;
			}
			return updateStatus.preferredUpdate;
		}

		public bool IsLive(ICinemachineCamera vcam)
		{
			if (vcam != null)
			{
				for (int i = 0; i < this.BrainCount; i++)
				{
					CinemachineBrain activeBrain = this.GetActiveBrain(i);
					if (activeBrain != null && activeBrain.IsLive(vcam))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void GenerateCameraActivationEvent(ICinemachineCamera vcam)
		{
			if (vcam != null)
			{
				for (int i = 0; i < this.BrainCount; i++)
				{
					CinemachineBrain activeBrain = this.GetActiveBrain(i);
					if (activeBrain != null && activeBrain.IsLive(vcam))
					{
						activeBrain.m_CameraActivatedEvent.Invoke(vcam);
					}
				}
			}
		}

		public void GenerateCameraCutEvent(ICinemachineCamera vcam)
		{
			if (vcam != null)
			{
				for (int i = 0; i < this.BrainCount; i++)
				{
					CinemachineBrain activeBrain = this.GetActiveBrain(i);
					if (activeBrain != null && activeBrain.IsLive(vcam))
					{
						activeBrain.m_CameraCutEvent.Invoke(activeBrain);
					}
				}
			}
		}

		public CinemachineBrain FindPotentialTargetBrain(ICinemachineCamera vcam)
		{
			int brainCount = this.BrainCount;
			if (vcam != null && brainCount > 1)
			{
				for (int i = 0; i < brainCount; i++)
				{
					CinemachineBrain activeBrain = this.GetActiveBrain(i);
					if (activeBrain != null && activeBrain.OutputCamera != null && activeBrain.IsLive(vcam))
					{
						return activeBrain;
					}
				}
			}
			for (int j = 0; j < brainCount; j++)
			{
				CinemachineBrain activeBrain2 = this.GetActiveBrain(j);
				if (activeBrain2 != null && activeBrain2.OutputCamera != null)
				{
					return activeBrain2;
				}
			}
			return null;
		}

		static CinemachineCore()
		{
			// Note: this type is marked as 'beforefieldinit'.
			if (CinemachineCore.<>f__mg$cache0 == null)
			{
				CinemachineCore.<>f__mg$cache0 = new CinemachineCore.AxisInputDelegate(Input.GetAxis);
			}
			CinemachineCore.GetInputAxis = CinemachineCore.<>f__mg$cache0;
		}

		public static readonly int kStreamingVersion = 20170927;

		public static readonly string kVersionString = "2.1";

		private static CinemachineCore sInstance = null;

		public static bool sShowHiddenObjects = false;

		public static CinemachineCore.AxisInputDelegate GetInputAxis;

		private List<CinemachineBrain> mActiveBrains = new List<CinemachineBrain>();

		private List<ICinemachineCamera> mActiveCameras = new List<ICinemachineCamera>();

		private List<List<ICinemachineCamera>> mChildCameras = new List<List<ICinemachineCamera>>();

		private Dictionary<ICinemachineCamera, CinemachineCore.UpdateStatus> mUpdateStatus;

		[CompilerGenerated]
		private static CinemachineCore.AxisInputDelegate <>f__mg$cache0;

		public enum Stage
		{
			Body,
			Aim,
			Noise
		}

		public delegate float AxisInputDelegate(string axisName);

		private struct UpdateStatus
		{
			public UpdateStatus(int currentFrame)
			{
				this.lastUpdateFrame = -1;
				this.lastUpdateSubframe = 0;
				this.windowStart = currentFrame;
				this.numWindowLateUpdateMoves = 0;
				this.numWindowFixedUpdateMoves = 0;
				this.numWindows = 0;
				this.preferredUpdate = CinemachineCore.UpdateFilter.Late;
				this.targetPos = Matrix4x4.zero;
			}

			public CinemachineCore.UpdateFilter ChoosePreferredUpdate(int currentFrame, Matrix4x4 pos, CinemachineCore.UpdateFilter updateFilter)
			{
				if (this.targetPos != pos)
				{
					if (updateFilter == CinemachineCore.UpdateFilter.Late)
					{
						this.numWindowLateUpdateMoves++;
					}
					else if (this.lastUpdateSubframe == 0)
					{
						this.numWindowFixedUpdateMoves++;
					}
					this.targetPos = pos;
				}
				CinemachineCore.UpdateFilter updateFilter2 = this.preferredUpdate;
				if ((this.numWindowLateUpdateMoves > 0 && this.numWindowFixedUpdateMoves > 0) || this.numWindowLateUpdateMoves >= this.numWindowFixedUpdateMoves)
				{
					updateFilter2 = CinemachineCore.UpdateFilter.Late;
				}
				else
				{
					updateFilter2 = CinemachineCore.UpdateFilter.Fixed;
				}
				if (this.numWindows == 0)
				{
					this.preferredUpdate = updateFilter2;
				}
				if (this.windowStart + 30 <= currentFrame)
				{
					this.preferredUpdate = updateFilter2;
					this.numWindows++;
					this.windowStart = currentFrame;
					this.numWindowLateUpdateMoves = (this.numWindowFixedUpdateMoves = 0);
				}
				return this.preferredUpdate;
			}

			private const int kWindowSize = 30;

			public int lastUpdateFrame;

			public int lastUpdateSubframe;

			public int windowStart;

			public int numWindowLateUpdateMoves;

			public int numWindowFixedUpdateMoves;

			public int numWindows;

			public CinemachineCore.UpdateFilter preferredUpdate;

			public Matrix4x4 targetPos;
		}

		public enum UpdateFilter
		{
			Fixed,
			ForcedFixed,
			Late,
			ForcedLate
		}
	}
}
