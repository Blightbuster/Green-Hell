using System;
using CJTools;
using UnityEngine;

public class LookController : PlayerController
{
	public static LookController Get()
	{
		return LookController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		LookController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.Look;
		this.m_LookDevSmooth.Init(Vector2.zero, 1f, true);
	}

	public void SetLookAtObjectFactor(float factor)
	{
		this.m_LookAtObjectFactor = factor;
	}

	public void ResetLookAtObjectFactor()
	{
		this.m_LookAtObjectFactor = 2f;
	}

	public void SetLookAtObject(GameObject obj)
	{
		this.m_LookAtObject = obj;
	}

	public void ResetLookAtObject(GameObject obj)
	{
		this.m_LookAtObject = null;
	}

	public void UpdateLookDev(float x, float y)
	{
		if (this.UpdateLookAtObject())
		{
			return;
		}
		if (this.UpdateWantedLookDir())
		{
			return;
		}
		this.m_WantedLookDev.x = this.m_WantedLookDev.x + x;
		this.m_WantedLookDev.y = this.m_WantedLookDev.y + y;
		if (this.m_WantedLookDev.y > 80f)
		{
			this.m_WantedLookDev.y = 80f;
		}
		if (this.m_WantedLookDev.y < -64f)
		{
			this.m_WantedLookDev.y = -64f;
		}
		this.m_LookDev.x = this.m_LookDev.x % 360f;
		this.m_WantedLookDev.x = this.m_WantedLookDev.x % 360f;
		if (this.m_LookDev.x < 0f)
		{
			this.m_LookDev.x = 360f + this.m_LookDev.x;
		}
		if (this.m_WantedLookDev.x < 0f)
		{
			this.m_WantedLookDev.x = 360f + this.m_WantedLookDev.x;
		}
		this.m_LookDevSmooth.current = this.m_LookDev;
		this.m_LookDevSmooth.target = this.m_WantedLookDev;
		this.m_LookDevSmooth.Update(GreenHellGame.Instance.m_Settings.m_LookRotationSpeed * Time.unscaledDeltaTime);
		this.m_LookDev = this.m_LookDevSmooth.current;
	}

	public void CalculateLookDev(Vector3 dir)
	{
		Vector3 vector = dir;
		vector.y = 0f;
		if (vector.magnitude == 0f)
		{
			this.m_LookDev.x = 0f;
		}
		else
		{
			vector.Normalize();
			this.m_LookDev.x = Mathf.Acos(vector.z) * 57.29578f;
			this.m_LookDev.x = this.m_LookDev.x + ((vector.x < 0f) ? (180f - this.m_LookDev.x) : 0f);
		}
		vector = dir;
		vector.x = 0f;
		if (vector.magnitude == 0f)
		{
			this.m_LookDev.y = 0f;
			return;
		}
		vector.Normalize();
		this.m_LookDev.y = Mathf.Asin(vector.y) * 57.29578f * Mathf.Sign(vector.y);
	}

	public bool UpdateLookAtObject()
	{
		if (!this.m_LookAtObject)
		{
			return false;
		}
		Vector3 vector = this.m_LookAtObject.transform.position - Camera.main.transform.position;
		vector.Normalize();
		Vector2 zero = Vector2.zero;
		zero.x = 57.29578f * Mathf.Atan2(vector.x, vector.z);
		zero.x %= 360f;
		if (zero.x < 0f)
		{
			zero.x = 360f + zero.x;
		}
		float num = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
		if (num == 0f)
		{
			zero.y = ((vector.y > 0f) ? 90f : -90f);
		}
		else
		{
			zero.y = 57.29578f * Mathf.Atan(vector.y / num);
		}
		if (Mathf.Abs(zero.x - this.m_LookDev.x) > 180f)
		{
			if (zero.x > this.m_LookDev.x)
			{
				this.m_LookDev.x = this.m_LookDev.x + 360f;
			}
			else
			{
				zero.x += 360f;
			}
		}
		this.m_LookDev += (zero - this.m_LookDev) * Time.deltaTime * this.m_LookAtObjectFactor;
		this.m_LookDev.x = this.m_LookDev.x % 360f;
		if (this.m_LookDev.x < 0f)
		{
			this.m_LookDev.x = 360f + this.m_LookDev.x;
		}
		this.m_WantedLookDev = this.m_LookDev;
		return true;
	}

	public void SetWantedLookDir(Vector3 dir)
	{
		this.m_WantedLookDir = dir;
	}

	public bool UpdateWantedLookDir()
	{
		if (this.m_WantedLookDir == Vector3.zero)
		{
			return false;
		}
		Vector3 wantedLookDir = this.m_WantedLookDir;
		wantedLookDir.Normalize();
		Vector2 zero = Vector2.zero;
		zero.x = 57.29578f * Mathf.Atan2(wantedLookDir.x, wantedLookDir.z);
		zero.x %= 360f;
		if (zero.x < 0f)
		{
			zero.x = 360f + zero.x;
		}
		float num = Mathf.Sqrt(wantedLookDir.x * wantedLookDir.x + wantedLookDir.z * wantedLookDir.z);
		if (num == 0f)
		{
			zero.y = ((wantedLookDir.y > 0f) ? 90f : -90f);
		}
		else
		{
			zero.y = 57.29578f * Mathf.Atan(wantedLookDir.y / num);
		}
		if (Mathf.Abs(zero.x - this.m_LookDev.x) > 180f)
		{
			if (zero.x > this.m_LookDev.x)
			{
				this.m_LookDev.x = this.m_LookDev.x + 360f;
			}
			else
			{
				zero.x += 360f;
			}
		}
		this.m_LookDev += (zero - this.m_LookDev) * Time.deltaTime * this.m_LookAtObjectFactor;
		this.m_LookDev.x = this.m_LookDev.x % 360f;
		if (this.m_LookDev.x < 0f)
		{
			this.m_LookDev.x = 360f + this.m_LookDev.x;
		}
		this.m_WantedLookDev = this.m_LookDev;
		return true;
	}

	public Vector2 m_LookDev;

	private SpringVec2 m_LookDevSmooth;

	public Vector2 m_WantedLookDev;

	public float m_LookDevSpeed = 0.5f;

	private Vector3 m_WantedLookDir = Vector3.zero;

	public const float CAMERA_MAX_ANGLE_UP = 80f;

	public const float CAMERA_MAX_ANGLE_DOWN = 64f;

	private GameObject m_LookAtObject;

	public float m_LookAtObjectFactor = 2f;

	private static LookController s_Instance;
}
