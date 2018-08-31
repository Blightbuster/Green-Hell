using System;
using UnityEngine;

public class TOD_Time : MonoBehaviour
{
	public event Action OnMinute;

	public event Action OnHour;

	public event Action OnDay;

	public event Action OnMonth;

	public event Action OnYear;

	public event Action OnSunrise;

	public event Action OnSunset;

	public void RefreshTimeCurve()
	{
		this.TimeCurve.preWrapMode = WrapMode.Once;
		this.TimeCurve.postWrapMode = WrapMode.Once;
		this.ApproximateCurve(this.TimeCurve, out this.timeCurve, out this.timeCurveInverse);
		this.timeCurve.preWrapMode = WrapMode.Loop;
		this.timeCurve.postWrapMode = WrapMode.Loop;
		this.timeCurveInverse.preWrapMode = WrapMode.Loop;
		this.timeCurveInverse.postWrapMode = WrapMode.Loop;
	}

	public float ApplyTimeCurve(float deltaTime)
	{
		float num = this.timeCurveInverse.Evaluate(this.sky.Cycle.Hour) + deltaTime;
		deltaTime = this.timeCurve.Evaluate(num) - this.sky.Cycle.Hour;
		if (num >= 24f)
		{
			deltaTime += (float)((int)num / 24 * 24);
		}
		else if (num < 0f)
		{
			deltaTime += (float)(((int)num / 24 - 1) * 24);
		}
		return deltaTime;
	}

	public void AddHours(float hours, bool adjust = true)
	{
		if (this.UseTimeCurve && adjust)
		{
			hours = this.ApplyTimeCurve(hours);
		}
		DateTime dateTime = this.sky.Cycle.DateTime;
		DateTime dateTime2 = dateTime.AddHours((double)hours);
		this.sky.Cycle.GameTimeDelta = hours;
		this.sky.Cycle.GameTime += hours;
		this.sky.Cycle.DateTime = dateTime2;
		if (dateTime2.Year > dateTime.Year)
		{
			if (this.OnYear != null)
			{
				this.OnYear();
			}
			if (this.OnMonth != null)
			{
				this.OnMonth();
			}
			if (this.OnDay != null)
			{
				this.OnDay();
			}
			if (this.OnHour != null)
			{
				this.OnHour();
			}
			if (this.OnMinute != null)
			{
				this.OnMinute();
			}
		}
		else if (dateTime2.Month > dateTime.Month)
		{
			if (this.OnMonth != null)
			{
				this.OnMonth();
			}
			if (this.OnDay != null)
			{
				this.OnDay();
			}
			if (this.OnHour != null)
			{
				this.OnHour();
			}
			if (this.OnMinute != null)
			{
				this.OnMinute();
			}
		}
		else if (dateTime2.Day > dateTime.Day)
		{
			if (this.OnDay != null)
			{
				this.OnDay();
			}
			if (this.OnHour != null)
			{
				this.OnHour();
			}
			if (this.OnMinute != null)
			{
				this.OnMinute();
			}
		}
		else if (dateTime2.Hour > dateTime.Hour)
		{
			if (this.OnHour != null)
			{
				this.OnHour();
			}
			if (this.OnMinute != null)
			{
				this.OnMinute();
			}
		}
		else if (dateTime2.Minute > dateTime.Minute && this.OnMinute != null)
		{
			this.OnMinute();
		}
		double totalHours = dateTime.TimeOfDay.TotalHours;
		double totalHours2 = dateTime2.TimeOfDay.TotalHours;
		if (totalHours < (double)this.sky.SunriseTime && totalHours2 >= (double)this.sky.SunriseTime && this.OnSunrise != null)
		{
			this.OnSunrise();
		}
		if (totalHours < (double)this.sky.SunsetTime && totalHours2 >= (double)this.sky.SunsetTime && this.OnSunset != null)
		{
			this.OnSunset();
		}
	}

	public void AddSeconds(float seconds, bool adjust = true)
	{
		this.AddHours(seconds / 3600f, true);
	}

	private void CalculateLinearTangents(Keyframe[] keys)
	{
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[i];
			if (i > 0)
			{
				Keyframe keyframe2 = keys[i - 1];
				keyframe.inTangent = (keyframe.value - keyframe2.value) / (keyframe.time - keyframe2.time);
			}
			if (i < keys.Length - 1)
			{
				Keyframe keyframe3 = keys[i + 1];
				keyframe.outTangent = (keyframe3.value - keyframe.value) / (keyframe3.time - keyframe.time);
			}
			keys[i] = keyframe;
		}
	}

	private void ApproximateCurve(AnimationCurve source, out AnimationCurve approxCurve, out AnimationCurve approxInverse)
	{
		Keyframe[] array = new Keyframe[25];
		Keyframe[] array2 = new Keyframe[25];
		float num = -0.01f;
		for (int i = 0; i < 25; i++)
		{
			num = Mathf.Max(num + 0.01f, source.Evaluate((float)i));
			array[i] = new Keyframe((float)i, num);
			array2[i] = new Keyframe(num, (float)i);
		}
		this.CalculateLinearTangents(array);
		this.CalculateLinearTangents(array2);
		approxCurve = new AnimationCurve(array);
		approxInverse = new AnimationCurve(array2);
	}

	protected void Awake()
	{
		this.sky = base.GetComponent<TOD_Sky>();
		if (this.UseDeviceDate)
		{
			this.sky.Cycle.Year = DateTime.Now.Year;
			this.sky.Cycle.Month = DateTime.Now.Month;
			this.sky.Cycle.Day = DateTime.Now.Day;
		}
		if (this.UseDeviceTime)
		{
			this.sky.Cycle.Hour = (float)DateTime.Now.TimeOfDay.TotalHours;
		}
		this.RefreshTimeCurve();
	}

	protected void Update()
	{
		if (!Player.Get())
		{
			return;
		}
		if (Player.Get().m_DreamActive)
		{
			if (!this.m_WasDreamActive)
			{
				this.m_HourBeforeDream = this.sky.Cycle.Hour;
			}
			this.sky.Cycle.Hour = 1.3f;
			this.m_WasDreamActive = true;
		}
		else
		{
			if (this.m_WasDreamActive)
			{
				this.sky.Cycle.Hour = this.m_HourBeforeDream;
			}
			if (this.ProgressTime && this.m_DayLengthInMinutes > 0f && this.m_NightLengthInMinutes > 0f)
			{
				bool flag = this.sky.Cycle.Hour > 6f && this.sky.Cycle.Hour <= 18f;
				float num = (!flag) ? (this.m_NightLen / this.m_NightLengthInMinutes) : (this.m_DayLen / this.m_DayLengthInMinutes);
				this.AddSeconds(Time.deltaTime * num, true);
			}
			this.m_WasDreamActive = false;
		}
	}

	[Tooltip("Progress time at runtime.")]
	public bool ProgressTime = true;

	[Tooltip("Set the date to the current device date on start.")]
	public bool UseDeviceDate;

	[Tooltip("Set the time to the current device time on start.")]
	public bool UseDeviceTime;

	[Tooltip("Apply the time curve when progressing time.")]
	public bool UseTimeCurve;

	[Tooltip("Time progression curve.")]
	public AnimationCurve TimeCurve = AnimationCurve.Linear(0f, 0f, 24f, 24f);

	private TOD_Sky sky;

	private AnimationCurve timeCurve;

	private AnimationCurve timeCurveInverse;

	public float m_DayLengthInMinutes = 20f;

	public float m_NightLengthInMinutes = 10f;

	[HideInInspector]
	public float m_CurrentSpeedFactor = 1f;

	[HideInInspector]
	public float m_DayLen = 720f;

	public float m_NightLen = 720f;

	private bool m_WasDreamActive;

	private float m_HourBeforeDream;
}
