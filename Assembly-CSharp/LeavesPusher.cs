using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class LeavesPusher : MonoBehaviour
{
	public static LeavesPusher Get()
	{
		return LeavesPusher.s_Instance;
	}

	private void Awake()
	{
		LeavesPusher.s_Instance = this;
	}

	private void Start()
	{
		LeavesPusher.s_Shader = Shader.Find("Custom/WindLeavesC2Standard");
		LeavesPusher.s_ShaderPropertyShakeAdd = Shader.PropertyToID("_BranchAmplitudeAdd");
		LeavesPusher.s_ShaderPropertyShakeTimeAdd = Shader.PropertyToID("_BranchFreqAdd");
		this.m_SOM = StaticObjectsManager.Get();
		this.m_CharacterController = Player.Get().GetComponent<CharacterControllerProxy>();
	}

	public void Push(GameObject pusher_obj, float radius, Vector3? position_offset = null)
	{
		if (!this.m_SOM)
		{
			return;
		}
		float radius2 = this.GetRadius(pusher_obj.ReplIsOwner());
		foreach (KeyValuePair<StaticObjectClass, GameObject> keyValuePair in this.m_SOM.m_ReplacedMap)
		{
			GameObject value = keyValuePair.Value;
			if (value == null)
			{
				return;
			}
			float num = value.transform.position.Distance2D(pusher_obj.transform.position + (position_offset ?? Vector3.zero));
			float def_shake_mul = 0f;
			float def_shake_time_mul = 0f;
			PushLeaves pushLeaves = null;
			value.GetComponents<PushLeaves>(this.m_PushLeavesCache);
			if (this.m_PushLeavesCache.Count > 0)
			{
				pushLeaves = this.m_PushLeavesCache[0];
			}
			if (num < radius2 && this.GetDataByObject(value) == null && pushLeaves != null)
			{
				List<Material> materialsToModify = pushLeaves.GetMaterialsToModify();
				int num2 = 0;
				if (num2 < materialsToModify.Count)
				{
					Material material = materialsToModify[num2];
					if (material.HasProperty(LeavesPusher.s_ShaderPropertyShakeAdd))
					{
						def_shake_mul = material.GetFloat(LeavesPusher.s_ShaderPropertyShakeAdd);
					}
					if (material.HasProperty(LeavesPusher.s_ShaderPropertyShakeTimeAdd))
					{
						def_shake_time_mul = material.GetFloat(LeavesPusher.s_ShaderPropertyShakeTimeAdd);
					}
				}
				this.m_Data.Add(new LeavesPusherData(value, value.transform.rotation, pusher_obj.transform.right, radius2, value.GetComponent<PushLeaves>(), def_shake_mul, def_shake_time_mul));
				if (pusher_obj.gameObject.ReplIsOwner())
				{
					PlayerInjuryModule.Get().SetLeechNextTime(PlayerInjuryModule.Get().GetLeechNextTime() - this.m_LeechCooldownDecrease2);
					PlayerInjuryModule.Get().CheckLeeches();
				}
			}
		}
	}

	public void PushHit(GameObject go, Vector3 hit_dir)
	{
		LeavesPusherData leavesPusherData = this.GetDataByObject(go);
		if (leavesPusherData != null)
		{
			leavesPusherData.m_HitAxis = Vector3.Cross(hit_dir, Vector3.up);
			leavesPusherData.m_HitTime = Time.time;
			return;
		}
		float def_shake_mul = 0f;
		float def_shake_time_mul = 0f;
		List<Material> materialsToModify = go.GetComponent<PushLeaves>().GetMaterialsToModify();
		int num = 0;
		if (num < materialsToModify.Count)
		{
			Material material = materialsToModify[num];
			if (material.HasProperty(LeavesPusher.s_ShaderPropertyShakeAdd))
			{
				def_shake_mul = material.GetFloat(LeavesPusher.s_ShaderPropertyShakeAdd);
			}
			if (material.HasProperty(LeavesPusher.s_ShaderPropertyShakeTimeAdd))
			{
				def_shake_time_mul = material.GetFloat(LeavesPusher.s_ShaderPropertyShakeTimeAdd);
			}
		}
		leavesPusherData = new LeavesPusherData(go, go.transform.rotation, Player.Get().transform.right, 0f, go.GetComponent<PushLeaves>(), def_shake_mul, def_shake_time_mul);
		leavesPusherData.m_HitAxis = Vector3.Cross(hit_dir, Vector3.up);
		leavesPusherData.m_HitTime = Time.time;
		this.m_Data.Add(leavesPusherData);
	}

	private LeavesPusherData GetDataByObject(GameObject go)
	{
		for (int i = 0; i < this.m_Data.Count; i++)
		{
			LeavesPusherData leavesPusherData = this.m_Data[i];
			if (leavesPusherData.m_Object.Equals(go))
			{
				return leavesPusherData;
			}
		}
		return null;
	}

	private void Update()
	{
		int i = 0;
		while (i < this.m_Data.Count)
		{
			this.m_LeavesPusher_data = this.m_Data[i];
			if (this.m_LeavesPusher_data.m_Object == null)
			{
				this.m_Data.RemoveAt(i);
			}
			else
			{
				i++;
				float num = float.MaxValue;
				Vector3 right = Vector3.right;
				bool flag = false;
				foreach (ReplicatedLogicalPlayer replicatedLogicalPlayer in ReplicatedLogicalPlayer.s_AllLogicalPlayers)
				{
					float num2 = this.m_LeavesPusher_data.m_Object.transform.position.Distance2D(replicatedLogicalPlayer.transform.position);
					if (num2 < num)
					{
						flag = replicatedLogicalPlayer.ReplIsOwner();
						num = num2;
						right = replicatedLogicalPlayer.transform.right;
					}
				}
				if (num < this.m_LeavesPusher_data.m_EnterRadiusSize)
				{
					if (Mathf.Abs(this.m_LeavesPusher_data.m_Angle) < 0.1f)
					{
						this.m_LeavesPusher_data.m_RotationAxis = right;
						if (flag && !this.m_LeavesPusher_data.m_PushLeaves.m_SmallPlant)
						{
							Player.Get().GetComponent<PlayerAudioModule>().PlayPlantsPushingSound(1f, false);
						}
					}
					this.m_LeavesPusher_data.m_Angle += (10f - this.m_LeavesPusher_data.m_Angle) * Time.deltaTime * 4f;
					this.m_LeavesPusher_data.m_ShaderPropertyShake = this.m_LeavesPusher_data.m_DefShake + Mathf.Sin(this.m_LeavesPusher_data.m_Angle / 10f * 3.14159274f) * this.m_LeavesPusher_data.m_PushLeaves.m_ShakeAdd;
					this.m_LeavesPusher_data.m_ShaderPropertyShakeTime = this.m_LeavesPusher_data.m_DefShakeTime + Mathf.Sin(this.m_LeavesPusher_data.m_Angle / 10f * 3.14159274f) * this.m_LeavesPusher_data.m_PushLeaves.m_ShakeTimeAdd;
				}
				else
				{
					this.m_LeavesPusher_data.m_Angle -= this.m_LeavesPusher_data.m_Angle * Time.deltaTime * 4f;
					float num3 = this.m_LeavesPusher_data.m_Angle / 10f;
					num3 = Mathf.Clamp(num3, 0f, 0.999f);
					this.m_LeavesPusher_data.m_ShaderPropertyShake = this.m_LeavesPusher_data.m_DefShake + (this.m_LeavesPusher_data.m_ShaderPropertyShake - this.m_LeavesPusher_data.m_DefShake) * num3;
					this.m_LeavesPusher_data.m_ShaderPropertyShakeTime = this.m_LeavesPusher_data.m_DefShakeTime + (this.m_LeavesPusher_data.m_ShaderPropertyShakeTime - this.m_LeavesPusher_data.m_DefShakeTime) * num3;
				}
				Quaternion originalQuat = this.m_LeavesPusher_data.m_OriginalQuat;
				Quaternion lhs = Quaternion.AngleAxis(this.m_LeavesPusher_data.m_Angle, this.m_LeavesPusher_data.m_RotationAxis);
				this.m_LeavesPusher_data.m_Object.transform.rotation = lhs * originalQuat;
				if (this.m_LeavesPusher_data.m_HitTime > 0f)
				{
					float num4 = Mathf.Clamp01((Time.time - this.m_LeavesPusher_data.m_HitTime) / 1f);
					lhs = Quaternion.AngleAxis(-(Mathf.Sin(num4 * 3.14159274f * 1.5f) * (7f * (1f - num4))), this.m_LeavesPusher_data.m_HitAxis);
					this.m_LeavesPusher_data.m_Object.transform.rotation = lhs * this.m_LeavesPusher_data.m_Object.transform.rotation;
					float a = this.m_LeavesPusher_data.m_DefShake + this.m_LeavesPusher_data.m_PushLeaves.m_HitShakeAdd * (1f - num4);
					float a2 = this.m_LeavesPusher_data.m_DefShakeTime + this.m_LeavesPusher_data.m_PushLeaves.m_HitShakeTimeAdd * (1f - num4);
					this.m_LeavesPusher_data.m_ShaderPropertyShake = Mathf.Max(a, this.m_LeavesPusher_data.m_ShaderPropertyShake);
					this.m_LeavesPusher_data.m_ShaderPropertyShakeTime = Mathf.Max(a2, this.m_LeavesPusher_data.m_ShaderPropertyShakeTime);
				}
				List<Material> materialsToModify = this.m_LeavesPusher_data.m_PushLeaves.GetMaterialsToModify();
				for (int j = 0; j < materialsToModify.Count; j++)
				{
					Material material = materialsToModify[j];
					material.SetFloat(LeavesPusher.s_ShaderPropertyShakeAdd, this.m_LeavesPusher_data.m_ShaderPropertyShake);
					material.SetFloat(LeavesPusher.s_ShaderPropertyShakeTimeAdd, this.m_LeavesPusher_data.m_ShaderPropertyShakeTime);
				}
			}
		}
	}

	private float GetRadius(bool is_local_player)
	{
		if (is_local_player)
		{
			return 1f;
		}
		return 1f * CJTools.Math.GetProportionalClamp(1f, 2f, this.m_CharacterController.velocity.magnitude, 0f, 4f);
	}

	public static Shader s_Shader;

	public static int s_ShaderPropertyShakeAdd;

	public static int s_ShaderPropertyShakeTimeAdd;

	private StaticObjectsManager m_SOM;

	private List<LeavesPusherData> m_Data = new List<LeavesPusherData>(400);

	private const float m_MaxRadius = 1f;

	private const float m_MaxAngle = 10f;

	private const float m_RotationSpeedMul = 4f;

	private CharacterControllerProxy m_CharacterController;

	private const float m_HitDuration = 1f;

	private static LeavesPusher s_Instance;

	private float m_LeechCooldownDecrease2 = 5f;

	private LeavesPusherData m_LeavesPusher_data;

	private List<PushLeaves> m_PushLeavesCache = new List<PushLeaves>(10);

	private List<Renderer> m_RendererCache = new List<Renderer>(10);
}
