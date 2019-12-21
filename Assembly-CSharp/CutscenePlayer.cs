using System;
using System.Collections.Generic;
using System.Linq;
using CJTools;
using UnityEngine;

public class CutscenePlayer : Being
{
	protected override void Awake()
	{
		base.Awake();
		this.m_PlayerControllers = base.GetComponents<CutscenePlayerController>();
		this.m_Cam = base.gameObject.transform.FindDeepChild("cam_orientation");
		this.m_Animator = base.GetComponent<Animator>();
	}

	protected override void Update()
	{
		base.Update();
		bool key = Input.GetKey(KeyCode.LeftAlt);
		for (int i = 48; i <= 57; i++)
		{
			if (Input.GetKeyDown((KeyCode)i))
			{
				if (i == 48)
				{
					this.m_Animator.SetBool("Cutscene" + (key ? "1" : "0") + (i - 48).ToString(), true);
				}
				else
				{
					this.m_Animator.SetTrigger("Cutscene" + (key ? "1" : "0") + (i - 48).ToString());
				}
			}
			if (i == 48 && Input.GetKeyUp((KeyCode)i))
			{
				this.m_Animator.SetBool("Cutscene0" + (i - 48).ToString(), false);
				this.m_Animator.SetBool("Cutscene1" + (i - 48).ToString(), false);
			}
		}
		if (!key)
		{
			for (int j = 0; j <= 9; j++)
			{
				this.m_Animator.ResetTrigger("Cutscene1" + j);
			}
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		this.UpdateBonesRotation();
	}

	public void UpdateBonesRotation()
	{
		List<Transform> list = new List<Transform>();
		CutscenePlayerController cutscenePlayerController = null;
		Vector3 right = base.gameObject.transform.right;
		Vector3 up = base.gameObject.transform.up;
		for (int i = 0; i < this.m_PlayerControllers.Length; i++)
		{
			CutscenePlayerController cutscenePlayerController2 = this.m_PlayerControllers[i];
			if (cutscenePlayerController2.IsActive())
			{
				Dictionary<Transform, float> bodyRotationBonesParams = cutscenePlayerController2.GetBodyRotationBonesParams();
				if (bodyRotationBonesParams != null && bodyRotationBonesParams.Count > 0)
				{
					DebugUtils.Assert(!cutscenePlayerController, true);
					cutscenePlayerController = cutscenePlayerController2;
					if (cutscenePlayerController != this.m_LastActiveBodyRotationController)
					{
						this.m_LastTimeBodyRotationControllerChange = Time.time;
					}
					for (int j = 0; j < bodyRotationBonesParams.Count; j++)
					{
						Transform transform = bodyRotationBonesParams.Keys.ElementAt(j);
						if (!list.Contains(transform))
						{
							list.Add(transform);
						}
						float num = bodyRotationBonesParams[transform];
						float num2 = 0f;
						this.m_RotatedBodyBones.TryGetValue(transform, out num2);
						float num3 = num2 + (num - num2) * Mathf.Clamp01(Time.time - this.m_LastTimeBodyRotationControllerChange);
						transform.Rotate(right, num3, Space.World);
						this.m_RotatedBodyBones[transform] = num3;
					}
					this.m_LastActiveBodyRotationController = cutscenePlayerController;
				}
			}
		}
		int k = 0;
		while (k < this.m_RotatedBodyBones.Count)
		{
			Transform transform2 = this.m_RotatedBodyBones.Keys.ElementAt(k);
			if (!list.Contains(transform2))
			{
				float num4 = this.m_RotatedBodyBones[transform2];
				num4 *= Mathf.Clamp01(1f - (Time.time - this.m_LastTimeBodyRotationControllerChange) / 0.3f);
				this.m_RotatedBodyBones[transform2] = num4;
				if (Mathf.Abs(num4) > 0.1f)
				{
					transform2.Rotate(right, num4, Space.World);
					k++;
				}
				else
				{
					this.m_RotatedBodyBones.Remove(transform2);
				}
			}
			else
			{
				k++;
			}
		}
	}

	private CutscenePlayerController[] m_PlayerControllers;

	private CutscenePlayerController m_LastActiveBodyRotationController;

	private float m_LastTimeBodyRotationControllerChange;

	private Dictionary<Transform, float> m_RotatedBodyBones = new Dictionary<Transform, float>();

	private Animator m_Animator;
}
