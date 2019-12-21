using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Cinemachine
{
	[DocumentationSorting(0f, DocumentationSortingAttribute.Level.UserRef)]
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[AddComponentMenu("Cinemachine/CinemachineBrain")]
	[SaveDuringPlay]
	public class CinemachineBrain : MonoBehaviour
	{
		public Camera OutputCamera
		{
			get
			{
				if (this.m_OutputCamera == null)
				{
					this.m_OutputCamera = base.GetComponent<Camera>();
				}
				return this.m_OutputCamera;
			}
		}

		internal Component PostProcessingComponent { get; set; }

		public static ICinemachineCamera SoloCamera { get; set; }

		public static Color GetSoloGUIColor()
		{
			return Color.Lerp(Color.red, Color.yellow, 0.8f);
		}

		public Vector3 DefaultWorldUp
		{
			get
			{
				if (!(this.m_WorldUpOverride != null))
				{
					return Vector3.up;
				}
				return this.m_WorldUpOverride.transform.up;
			}
		}

		private CinemachineBrain.OverrideStackFrame GetOverrideFrame(int id)
		{
			int count = this.mOverrideStack.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.mOverrideStack[i].id == id)
				{
					return this.mOverrideStack[i];
				}
			}
			CinemachineBrain.OverrideStackFrame overrideStackFrame = new CinemachineBrain.OverrideStackFrame();
			overrideStackFrame.id = id;
			this.mOverrideStack.Insert(0, overrideStackFrame);
			return overrideStackFrame;
		}

		private CinemachineBrain.OverrideStackFrame GetNextActiveFrame(int overrideId)
		{
			bool flag = false;
			int count = this.mOverrideStack.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.mOverrideStack[i].id == overrideId)
				{
					flag = true;
				}
				else if (this.mOverrideStack[i].Active && flag)
				{
					return this.mOverrideStack[i];
				}
			}
			this.mOverrideBlendFromNothing.camera = this.TopCameraFromPriorityQueue();
			this.mOverrideBlendFromNothing.blend = this.mActiveBlend;
			return this.mOverrideBlendFromNothing;
		}

		private CinemachineBrain.OverrideStackFrame GetActiveOverride()
		{
			int count = this.mOverrideStack.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.mOverrideStack[i].Active)
				{
					return this.mOverrideStack[i];
				}
			}
			return null;
		}

		internal int SetCameraOverride(int overrideId, ICinemachineCamera camA, ICinemachineCamera camB, float weightB, float deltaTime)
		{
			if (overrideId < 0)
			{
				int num = this.mNextOverrideId;
				this.mNextOverrideId = num + 1;
				overrideId = num;
			}
			CinemachineBrain.OverrideStackFrame overrideFrame = this.GetOverrideFrame(overrideId);
			overrideFrame.camera = null;
			overrideFrame.deltaTime = deltaTime;
			overrideFrame.timeOfOverride = Time.realtimeSinceStartup;
			if (camA != null || camB != null)
			{
				if (weightB <= 0.0001f)
				{
					overrideFrame.blend = null;
					if (camA != null)
					{
						overrideFrame.camera = camA;
					}
				}
				else if (weightB >= 0.9999f)
				{
					overrideFrame.blend = null;
					if (camB != null)
					{
						overrideFrame.camera = camB;
					}
				}
				else
				{
					if (camB == null)
					{
						ICinemachineCamera cinemachineCamera = camB;
						camB = camA;
						camA = cinemachineCamera;
						weightB = 1f - weightB;
					}
					if (camA == null)
					{
						CinemachineBrain.OverrideStackFrame nextActiveFrame = this.GetNextActiveFrame(overrideId);
						if (nextActiveFrame.blend != null)
						{
							camA = new BlendSourceVirtualCamera(nextActiveFrame.blend, deltaTime);
						}
						else
						{
							camA = ((nextActiveFrame.camera != null) ? nextActiveFrame.camera : camB);
						}
					}
					if (overrideFrame.blend == null)
					{
						overrideFrame.blend = new CinemachineBlend(camA, camB, AnimationCurve.Linear(0f, 0f, 1f, 1f), 1f, weightB);
					}
					overrideFrame.blend.CamA = camA;
					overrideFrame.blend.CamB = camB;
					overrideFrame.blend.TimeInBlend = weightB;
					overrideFrame.camera = camB;
				}
			}
			return overrideId;
		}

		internal void ReleaseCameraOverride(int overrideId)
		{
			int count = this.mOverrideStack.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.mOverrideStack[i].id == overrideId)
				{
					this.mOverrideStack.RemoveAt(i);
					return;
				}
			}
		}

		private void OnEnable()
		{
			this.mActiveBlend = null;
			this.mActiveCameraPreviousFrame = null;
			this.mOutgoingCameraPreviousFrame = null;
			this.mPreviousFrameWasOverride = false;
			CinemachineCore.Instance.AddActiveBrain(this);
		}

		private void OnDisable()
		{
			CinemachineCore.Instance.RemoveActiveBrain(this);
			this.mActiveBlend = null;
			this.mActiveCameraPreviousFrame = null;
			this.mOutgoingCameraPreviousFrame = null;
			this.mPreviousFrameWasOverride = false;
			this.mOverrideStack.Clear();
		}

		private void Start()
		{
			this.UpdateVirtualCameras(CinemachineCore.UpdateFilter.Late, -1f);
			base.StartCoroutine(this.AfterPhysics());
		}

		private IEnumerator AfterPhysics()
		{
			/*
An exception occurred when decompiling this method (0600412E)

ICSharpCode.Decompiler.DecompilerException: Error decompiling System.Collections.IEnumerator Cinemachine.CinemachineBrain::AfterPhysics()
 ---> System.ArgumentOutOfRangeException: Der Index lag außerhalb des Bereichs. Er darf nicht negativ und kleiner als die Sammlung sein.
Parametername: index
   bei System.ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource)
   bei ICSharpCode.Decompiler.ILAst.StateRangeAnalysis.CreateLabelRangeMapping(List`1 body, Int32 pos, Int32 bodyLength, LabelRangeMapping result, Boolean onlyInitialLabels) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\ILAst\StateRange.cs:Zeile 326.
   bei ICSharpCode.Decompiler.ILAst.MicrosoftYieldReturnDecompiler.AnalyzeMoveNext() in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\ILAst\MicrosoftYieldReturnDecompiler.cs:Zeile 347.
   bei ICSharpCode.Decompiler.ILAst.YieldReturnDecompiler.Run() in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\ILAst\YieldReturnDecompiler.cs:Zeile 93.
   bei ICSharpCode.Decompiler.ILAst.YieldReturnDecompiler.Run(DecompilerContext context, ILBlock method, AutoPropertyProvider autoPropertyProvider, List`1 list_ILNode, Func`2 getILInlining, List`1 listExpr, List`1 listBlock, Dictionary`2 labelRefCount) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\ILAst\YieldReturnDecompiler.cs:Zeile 69.
   bei ICSharpCode.Decompiler.ILAst.ILAstOptimizer.Optimize(DecompilerContext context, ILBlock method, AutoPropertyProvider autoPropertyProvider, ILAstOptimizationStep abortBeforeStep) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\ILAst\ILAstOptimizer.cs:Zeile 233.
   bei ICSharpCode.Decompiler.Ast.AstMethodBodyBuilder.CreateMethodBody(IEnumerable`1 parameters, MethodDebugInfoBuilder& builder) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\Ast\AstMethodBodyBuilder.cs:Zeile 118.
   bei ICSharpCode.Decompiler.Ast.AstMethodBodyBuilder.CreateMethodBody(MethodDef methodDef, DecompilerContext context, AutoPropertyProvider autoPropertyProvider, IEnumerable`1 parameters, Boolean valueParameterIsKeyword, StringBuilder sb, MethodDebugInfoBuilder& stmtsBuilder) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\Ast\AstMethodBodyBuilder.cs:Zeile 88.
   --- Ende der internen Ausnahmestapelüberwachung ---
   bei ICSharpCode.Decompiler.Ast.AstMethodBodyBuilder.CreateMethodBody(MethodDef methodDef, DecompilerContext context, AutoPropertyProvider autoPropertyProvider, IEnumerable`1 parameters, Boolean valueParameterIsKeyword, StringBuilder sb, MethodDebugInfoBuilder& stmtsBuilder) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\Ast\AstMethodBodyBuilder.cs:Zeile 92.
   bei ICSharpCode.Decompiler.Ast.AstBuilder.CreateMethodBody(MethodDef method, IEnumerable`1 parameters, Boolean valueParameterIsKeyword, MethodKind methodKind, MethodDebugInfoBuilder& builder) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\Ast\AstBuilder.cs:Zeile 1427.
*/;
		}

		private void LateUpdate()
		{
			if (CutscenesManager.Get() == null)
			{
				return;
			}
			float effectiveDeltaTime = this.GetEffectiveDeltaTime(false);
			if (this.m_UpdateMethod == CinemachineBrain.UpdateMethod.SmartUpdate)
			{
				this.UpdateVirtualCameras(CinemachineCore.UpdateFilter.Late, effectiveDeltaTime);
			}
			else if (this.m_UpdateMethod == CinemachineBrain.UpdateMethod.LateUpdate)
			{
				this.UpdateVirtualCameras(CinemachineCore.UpdateFilter.ForcedLate, effectiveDeltaTime);
			}
			this.ProcessActiveCamera(this.GetEffectiveDeltaTime(false));
		}

		private float GetEffectiveDeltaTime(bool fixedDelta)
		{
			if (CinemachineBrain.SoloCamera != null)
			{
				return Time.unscaledDeltaTime;
			}
			CinemachineBrain.OverrideStackFrame activeOverride = this.GetActiveOverride();
			if (activeOverride != null)
			{
				if (!activeOverride.Expired)
				{
					return activeOverride.deltaTime;
				}
				return -1f;
			}
			else
			{
				if (!Application.isPlaying)
				{
					return -1f;
				}
				if (this.m_IgnoreTimeScale)
				{
					if (!fixedDelta)
					{
						return Time.unscaledDeltaTime;
					}
					return Time.fixedDeltaTime;
				}
				else
				{
					if (!fixedDelta)
					{
						return Time.deltaTime;
					}
					return Time.fixedDeltaTime * Time.timeScale;
				}
			}
		}

		private void UpdateVirtualCameras(CinemachineCore.UpdateFilter updateFilter, float deltaTime)
		{
			CinemachineCore.Instance.CurrentUpdateFilter = updateFilter;
			CinemachineCore.Instance.UpdateAllActiveVirtualCameras(this.DefaultWorldUp, deltaTime);
			ICinemachineCamera activeVirtualCamera = this.ActiveVirtualCamera;
			if (activeVirtualCamera != null)
			{
				CinemachineCore.Instance.UpdateVirtualCamera(activeVirtualCamera, this.DefaultWorldUp, deltaTime);
			}
			CinemachineBlend activeBlend = this.ActiveBlend;
			if (activeBlend != null)
			{
				activeBlend.UpdateCameraState(this.DefaultWorldUp, deltaTime);
			}
			CinemachineCore.Instance.CurrentUpdateFilter = CinemachineCore.UpdateFilter.Late;
		}

		private void ProcessActiveCamera(float deltaTime)
		{
			if (!base.isActiveAndEnabled)
			{
				this.mActiveCameraPreviousFrame = null;
				this.mOutgoingCameraPreviousFrame = null;
				this.mPreviousFrameWasOverride = false;
				return;
			}
			CinemachineBrain.OverrideStackFrame activeOverride = this.GetActiveOverride();
			ICinemachineCamera activeVirtualCamera = this.ActiveVirtualCamera;
			if (activeVirtualCamera == null)
			{
				this.mOutgoingCameraPreviousFrame = null;
			}
			else
			{
				if (activeOverride != null)
				{
					this.mActiveBlend = null;
				}
				CinemachineBlend cinemachineBlend = this.ActiveBlend;
				if (this.mActiveCameraPreviousFrame != null && this.mActiveCameraPreviousFrame.VirtualCameraGameObject == null)
				{
					this.mActiveCameraPreviousFrame = null;
				}
				if (this.mActiveCameraPreviousFrame != activeVirtualCamera)
				{
					if (this.mActiveCameraPreviousFrame != null && !this.mPreviousFrameWasOverride && activeOverride == null && deltaTime >= 0f)
					{
						float duration = 0f;
						AnimationCurve blendCurve = this.LookupBlendCurve(this.mActiveCameraPreviousFrame, activeVirtualCamera, out duration);
						cinemachineBlend = this.CreateBlend(this.mActiveCameraPreviousFrame, activeVirtualCamera, blendCurve, duration, this.mActiveBlend);
					}
					if (activeVirtualCamera != this.mOutgoingCameraPreviousFrame)
					{
						activeVirtualCamera.OnTransitionFromCamera(this.mActiveCameraPreviousFrame, this.DefaultWorldUp, deltaTime);
						if (!activeVirtualCamera.VirtualCameraGameObject.activeInHierarchy && (cinemachineBlend == null || !cinemachineBlend.Uses(activeVirtualCamera)))
						{
							activeVirtualCamera.UpdateCameraState(this.DefaultWorldUp, -1f);
						}
						if (this.m_CameraActivatedEvent != null)
						{
							this.m_CameraActivatedEvent.Invoke(activeVirtualCamera);
						}
					}
					if ((cinemachineBlend == null || (cinemachineBlend.CamA != this.mActiveCameraPreviousFrame && cinemachineBlend.CamB != this.mActiveCameraPreviousFrame && cinemachineBlend.CamA != this.mOutgoingCameraPreviousFrame && cinemachineBlend.CamB != this.mOutgoingCameraPreviousFrame)) && this.m_CameraCutEvent != null)
					{
						this.m_CameraCutEvent.Invoke(this);
					}
				}
				if (cinemachineBlend != null)
				{
					if (activeOverride == null)
					{
						cinemachineBlend.TimeInBlend += ((deltaTime >= 0f) ? deltaTime : cinemachineBlend.Duration);
					}
					if (cinemachineBlend.IsComplete)
					{
						cinemachineBlend = null;
					}
				}
				if (activeOverride == null)
				{
					this.mActiveBlend = cinemachineBlend;
				}
				CameraState state = activeVirtualCamera.State;
				if (cinemachineBlend != null)
				{
					state = cinemachineBlend.State;
				}
				this.PushStateToUnityCamera(state, activeVirtualCamera);
				this.mOutgoingCameraPreviousFrame = null;
				if (cinemachineBlend != null)
				{
					this.mOutgoingCameraPreviousFrame = cinemachineBlend.CamB;
				}
			}
			this.mActiveCameraPreviousFrame = activeVirtualCamera;
			this.mPreviousFrameWasOverride = (activeOverride != null);
			if (this.mPreviousFrameWasOverride && activeOverride.blend != null)
			{
				if (activeOverride.blend.BlendWeight < 0.5f)
				{
					this.mActiveCameraPreviousFrame = activeOverride.blend.CamA;
					this.mOutgoingCameraPreviousFrame = activeOverride.blend.CamB;
					return;
				}
				this.mActiveCameraPreviousFrame = activeOverride.blend.CamB;
				this.mOutgoingCameraPreviousFrame = activeOverride.blend.CamA;
			}
		}

		public bool IsBlending
		{
			get
			{
				return this.ActiveBlend != null && this.ActiveBlend.IsValid;
			}
		}

		public CinemachineBlend ActiveBlend
		{
			get
			{
				if (CinemachineBrain.SoloCamera != null)
				{
					return null;
				}
				CinemachineBrain.OverrideStackFrame activeOverride = this.GetActiveOverride();
				if (activeOverride == null || activeOverride.blend == null)
				{
					return this.mActiveBlend;
				}
				return activeOverride.blend;
			}
		}

		public bool IsLive(ICinemachineCamera vcam)
		{
			if (this.IsLiveItself(vcam))
			{
				return true;
			}
			ICinemachineCamera parentCamera = vcam.ParentCamera;
			while (parentCamera != null && parentCamera.IsLiveChild(vcam))
			{
				if (this.IsLiveItself(parentCamera))
				{
					return true;
				}
				vcam = parentCamera;
				parentCamera = vcam.ParentCamera;
			}
			return false;
		}

		private bool IsLiveItself(ICinemachineCamera vcam)
		{
			return this.mActiveCameraPreviousFrame == vcam || this.ActiveVirtualCamera == vcam || (this.IsBlending && this.ActiveBlend.Uses(vcam));
		}

		public ICinemachineCamera ActiveVirtualCamera
		{
			get
			{
				if (CinemachineBrain.SoloCamera != null)
				{
					return CinemachineBrain.SoloCamera;
				}
				CinemachineBrain.OverrideStackFrame activeOverride = this.GetActiveOverride();
				if (activeOverride == null || activeOverride.camera == null)
				{
					return this.TopCameraFromPriorityQueue();
				}
				return activeOverride.camera;
			}
		}

		public CameraState CurrentCameraState { get; private set; }

		private ICinemachineCamera TopCameraFromPriorityQueue()
		{
			Camera outputCamera = this.OutputCamera;
			int num = (outputCamera == null) ? -1 : outputCamera.cullingMask;
			int virtualCameraCount = CinemachineCore.Instance.VirtualCameraCount;
			for (int i = 0; i < virtualCameraCount; i++)
			{
				ICinemachineCamera virtualCamera = CinemachineCore.Instance.GetVirtualCamera(i);
				GameObject gameObject = (virtualCamera != null) ? virtualCamera.VirtualCameraGameObject : null;
				if (gameObject != null && (num & 1 << gameObject.layer) != 0)
				{
					return virtualCamera;
				}
			}
			return null;
		}

		private AnimationCurve LookupBlendCurve(ICinemachineCamera fromKey, ICinemachineCamera toKey, out float duration)
		{
			AnimationCurve animationCurve = this.m_DefaultBlend.BlendCurve;
			if (this.m_CustomBlends != null)
			{
				string fromCameraName = (fromKey != null) ? fromKey.Name : string.Empty;
				string toCameraName = (toKey != null) ? toKey.Name : string.Empty;
				animationCurve = this.m_CustomBlends.GetBlendCurveForVirtualCameras(fromCameraName, toCameraName, animationCurve);
			}
			Keyframe[] keys = animationCurve.keys;
			duration = ((keys == null || keys.Length == 0) ? 0f : keys[keys.Length - 1].time);
			return animationCurve;
		}

		private CinemachineBlend CreateBlend(ICinemachineCamera camA, ICinemachineCamera camB, AnimationCurve blendCurve, float duration, CinemachineBlend activeBlend)
		{
			if (blendCurve == null || duration <= 0f || (camA == null && camB == null))
			{
				return null;
			}
			if (camA == null || activeBlend != null)
			{
				CameraState state = CameraState.Default;
				if (activeBlend != null)
				{
					state = activeBlend.State;
				}
				else
				{
					state.RawPosition = base.transform.position;
					state.RawOrientation = base.transform.rotation;
					state.Lens = LensSettings.FromCamera(this.OutputCamera);
				}
				camA = new StaticPointVirtualCamera(state, (activeBlend == null) ? "(none)" : "Mid-blend");
			}
			return new CinemachineBlend(camA, camB, blendCurve, duration, 0f);
		}

		private void PushStateToUnityCamera(CameraState state, ICinemachineCamera vcam)
		{
			this.CurrentCameraState = state;
			base.transform.position = state.FinalPosition;
			base.transform.rotation = state.FinalOrientation;
			Camera outputCamera = this.OutputCamera;
			if (outputCamera != null)
			{
				outputCamera.fieldOfView = state.Lens.FieldOfView;
				outputCamera.orthographicSize = state.Lens.OrthographicSize;
				outputCamera.nearClipPlane = state.Lens.NearClipPlane;
				outputCamera.farClipPlane = state.Lens.FarClipPlane;
			}
			if (CinemachineBrain.sPostProcessingHandler != null)
			{
				CinemachineBrain.sPostProcessingHandler.Invoke(this);
			}
		}

		private void AddSubframe()
		{
			int frameCount = Time.frameCount;
			if (frameCount == CinemachineBrain.msCurrentFrame)
			{
				if (CinemachineBrain.msFirstBrainObjectId == base.GetInstanceID())
				{
					CinemachineBrain.msSubframes++;
					return;
				}
			}
			else
			{
				CinemachineBrain.msCurrentFrame = frameCount;
				CinemachineBrain.msFirstBrainObjectId = base.GetInstanceID();
				CinemachineBrain.msSubframes = 1;
			}
		}

		internal static int GetSubframeCount()
		{
			return Math.Max(1, CinemachineBrain.msSubframes);
		}

		[Tooltip("When enabled, the current camera and blend will be indicated in the game window, for debugging")]
		public bool m_ShowDebugText;

		[Tooltip("When enabled, the camera's frustum will be shown at all times in the scene view")]
		public bool m_ShowCameraFrustum = true;

		[Tooltip("When enabled, the cameras will always respond in real-time to user input and damping, even if the game is running in slow motion")]
		public bool m_IgnoreTimeScale;

		[Tooltip("If set, this object's Y axis will define the worldspace Up vector for all the virtual cameras.  This is useful for instance in top-down game environments.  If not set, Up is worldspace Y.  Setting this appropriately is important, because Virtual Cameras don't like looking straight up or straight down.")]
		public Transform m_WorldUpOverride;

		[Tooltip("Use FixedUpdate if all your targets are animated during FixedUpdate (e.g. RigidBodies), LateUpdate if all your targets are animated during the normal Update loop, and SmartUpdate if you want Cinemachine to do the appropriate thing on a per-target basis.  SmartUpdate is the recommended setting")]
		public CinemachineBrain.UpdateMethod m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;

		[CinemachineBlendDefinitionProperty]
		[Tooltip("The blend that is used in cases where you haven't explicitly defined a blend between two Virtual Cameras")]
		public CinemachineBlendDefinition m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 2f);

		[Tooltip("This is the asset that contains custom settings for blends between specific virtual cameras in your scene")]
		public CinemachineBlenderSettings m_CustomBlends;

		private Camera m_OutputCamera;

		[Tooltip("This event will fire whenever a virtual camera goes live and there is no blend")]
		public CinemachineBrain.BrainEvent m_CameraCutEvent = new CinemachineBrain.BrainEvent();

		[Tooltip("This event will fire whenever a virtual camera goes live.  If a blend is involved, then the event will fire on the first frame of the blend.")]
		public CinemachineBrain.VcamEvent m_CameraActivatedEvent = new CinemachineBrain.VcamEvent();

		internal static CinemachineBrain.BrainEvent sPostProcessingHandler = new CinemachineBrain.BrainEvent();

		private ICinemachineCamera mActiveCameraPreviousFrame;

		private ICinemachineCamera mOutgoingCameraPreviousFrame;

		private CinemachineBlend mActiveBlend;

		private bool mPreviousFrameWasOverride;

		private List<CinemachineBrain.OverrideStackFrame> mOverrideStack = new List<CinemachineBrain.OverrideStackFrame>();

		private int mNextOverrideId = 1;

		private CinemachineBrain.OverrideStackFrame mOverrideBlendFromNothing = new CinemachineBrain.OverrideStackFrame();

		private WaitForFixedUpdate mWaitForFixedUpdate = new WaitForFixedUpdate();

		private static int msCurrentFrame;

		private static int msFirstBrainObjectId;

		private static int msSubframes;

		[DocumentationSorting(0.1f, DocumentationSortingAttribute.Level.UserRef)]
		public enum UpdateMethod
		{
			FixedUpdate,
			LateUpdate,
			SmartUpdate
		}

		[Serializable]
		public class BrainEvent : UnityEvent<CinemachineBrain>
		{
		}

		[Serializable]
		public class VcamEvent : UnityEvent<ICinemachineCamera>
		{
		}

		private class OverrideStackFrame
		{
			public bool Active
			{
				get
				{
					return this.camera != null;
				}
			}

			public bool Expired
			{
				get
				{
					return !Application.isPlaying && Time.realtimeSinceStartup - this.timeOfOverride > Time.maximumDeltaTime;
				}
			}

			public int id;

			public ICinemachineCamera camera;

			public CinemachineBlend blend;

			public float deltaTime;

			public float timeOfOverride;
		}
	}
}
