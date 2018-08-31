using System;
using UnityEngine;

public class CutscenePlayerLookController : CutscenePlayerController
{
	public static CutscenePlayerLookController Get()
	{
		return CutscenePlayerLookController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		CutscenePlayerLookController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Look;
	}

	public void SetLookAtObject(GameObject obj)
	{
		this.m_LookAtObject = obj;
	}

	public void UpdateLookDev(float x, float y)
	{
		if (this.UpdateLookAtObject())
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
		this.m_LookDev = Vector2.Lerp(this.m_LookDev, this.m_WantedLookDev, this.m_LookDevSpeed);
		this.m_LookDev.x = this.m_LookDev.x % 360f;
		if (this.m_LookDev.x < 0f)
		{
			this.m_LookDev.x = 360f + this.m_LookDev.x;
		}
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
			this.m_LookDev.x = this.m_LookDev.x + ((vector.x >= 0f) ? 0f : (180f - this.m_LookDev.x));
		}
		vector = dir;
		vector.x = 0f;
		if (vector.magnitude == 0f)
		{
			this.m_LookDev.y = 0f;
		}
		else
		{
			vector.Normalize();
			this.m_LookDev.y = Mathf.Asin(vector.y) * 57.29578f * Mathf.Sign(vector.y);
		}
	}

	private bool UpdateLookAtObject()
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
			zero.y = ((vector.y <= 0f) ? -90f : 90f);
		}
		else
		{
			zero.y = 57.29578f * Mathf.Atan(vector.y / num);
		}
		float num2 = Mathf.Abs(zero.x - this.m_LookDev.x);
		if (num2 > 180f)
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

	public Vector2 m_LookDev = default(Vector2);

	public Vector2 m_WantedLookDev = default(Vector2);

	public float m_LookDevSpeed = 1f;

	public const float CAMERA_MAX_ANGLE_UP = 80f;

	public const float CAMERA_MAX_ANGLE_DOWN = 64f;

	private GameObject m_LookAtObject;

	private float m_LookAtObjectFactor = 2f;

	private static CutscenePlayerLookController s_Instance;
}
