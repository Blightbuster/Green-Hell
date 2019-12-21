using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class Watch : MonoBehaviour
{
	public static Watch Get()
	{
		return Watch.s_Instance;
	}

	private void Awake()
	{
		Watch.s_Instance = this;
	}

	private void Start()
	{
		this.InitMacronutrientsData();
		this.InitSanityData();
		this.InitTimeData();
		this.InitCompassData();
		this.SetState(Watch.State.Macronutrients);
		base.gameObject.SetActive(false);
	}

	private void InitMacronutrientsData()
	{
		WatchMacronutrientsData watchMacronutrientsData = new WatchMacronutrientsData();
		watchMacronutrientsData.m_Parent = this.m_Canvas.transform.Find("Macronutrients").gameObject;
		watchMacronutrientsData.m_Fat = watchMacronutrientsData.m_Parent.transform.Find("Fat").GetComponent<Image>();
		watchMacronutrientsData.m_Carbo = watchMacronutrientsData.m_Parent.transform.Find("Carbo").GetComponent<Image>();
		watchMacronutrientsData.m_Hydration = watchMacronutrientsData.m_Parent.transform.Find("Hydration").GetComponent<Image>();
		watchMacronutrientsData.m_Proteins = watchMacronutrientsData.m_Parent.transform.Find("Proteins").GetComponent<Image>();
		watchMacronutrientsData.m_FatBG = watchMacronutrientsData.m_Parent.transform.Find("FatBG").GetComponent<Image>();
		watchMacronutrientsData.m_CarboBG = watchMacronutrientsData.m_Parent.transform.Find("CarboBG").GetComponent<Image>();
		watchMacronutrientsData.m_HydrationBG = watchMacronutrientsData.m_Parent.transform.Find("HydrationBG").GetComponent<Image>();
		watchMacronutrientsData.m_ProteinsBG = watchMacronutrientsData.m_Parent.transform.Find("ProteinsBG").GetComponent<Image>();
		Color color = IconColors.GetColor(IconColors.Icon.Fat);
		watchMacronutrientsData.m_Fat.color = color;
		color = IconColors.GetColor(IconColors.Icon.Carbo);
		watchMacronutrientsData.m_Carbo.color = color;
		color = IconColors.GetColor(IconColors.Icon.Proteins);
		watchMacronutrientsData.m_Proteins.color = color;
		color = IconColors.GetColor(IconColors.Icon.Hydration);
		watchMacronutrientsData.m_Hydration.color = color;
		watchMacronutrientsData.m_Parent.SetActive(false);
		this.m_Datas.Add(2, watchMacronutrientsData);
	}

	private void InitSanityData()
	{
		WatchSanityData watchSanityData = new WatchSanityData();
		watchSanityData.m_Parent = this.m_Canvas.transform.Find("Sanity").gameObject;
		watchSanityData.m_Sanity = watchSanityData.m_Parent.transform.Find("SanityHRM").GetComponent<SWP_HeartRateMonitor>();
		watchSanityData.m_SanityText = watchSanityData.m_Parent.GetComponent<Text>();
		watchSanityData.m_Parent.SetActive(false);
		this.m_Datas.Add(1, watchSanityData);
	}

	private void InitTimeData()
	{
		WatchTimeData watchTimeData = new WatchTimeData();
		watchTimeData.m_Parent = this.m_Canvas.transform.Find("Time").gameObject;
		watchTimeData.m_TimeHourDec = watchTimeData.m_Parent.transform.Find("HourDec").GetComponent<Text>();
		watchTimeData.m_TimeHourUnit = watchTimeData.m_Parent.transform.Find("HourUnit").GetComponent<Text>();
		watchTimeData.m_TimeMinuteDec = watchTimeData.m_Parent.transform.Find("MinuteDec").GetComponent<Text>();
		watchTimeData.m_TimeMinuteUnit = watchTimeData.m_Parent.transform.Find("MinuteUnit").GetComponent<Text>();
		watchTimeData.m_DayDec = watchTimeData.m_Parent.transform.Find("DayDec").GetComponent<Text>();
		watchTimeData.m_DayUnit = watchTimeData.m_Parent.transform.Find("DayUnit").GetComponent<Text>();
		watchTimeData.m_DayName = watchTimeData.m_Parent.transform.Find("DayName").GetComponent<Text>();
		watchTimeData.m_MonthName = watchTimeData.m_Parent.transform.Find("MonthName").GetComponent<Text>();
		watchTimeData.m_Parent.SetActive(false);
		this.m_Datas.Add(0, watchTimeData);
	}

	private void InitCompassData()
	{
		WatchCompassData watchCompassData = new WatchCompassData();
		watchCompassData.m_Parent = this.m_Canvas.transform.Find("Compass").gameObject;
		watchCompassData.m_Compass = watchCompassData.m_Parent.transform.Find("CompassIcon").gameObject;
		watchCompassData.m_GPSCoordinates = watchCompassData.m_Parent.transform.Find("Coordinates").GetComponent<Text>();
		watchCompassData.m_Parent.SetActive(false);
		this.m_Datas.Add(3, watchCompassData);
	}

	public void SetState(Watch.State state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.m_State = state;
		this.OnSetState();
	}

	private void OnSetState()
	{
		using (Dictionary<int, WatchData>.KeyCollection.Enumerator enumerator = this.m_Datas.Keys.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Watch.State state = (Watch.State)enumerator.Current;
				this.m_Datas[(int)state].GetParent().SetActive(this.m_State == state);
			}
		}
	}

	private void Update()
	{
		this.UpdateState();
	}

	private void UpdateState()
	{
		switch (this.m_State)
		{
		case Watch.State.Time:
		{
			WatchTimeData watchTimeData = (WatchTimeData)this.m_Datas[0];
			DateTime dateTime = MainLevel.Instance.m_TODSky.Cycle.DateTime.AddDays((double)this.m_FakeDayOffset);
			int hour = dateTime.Hour;
			int num = hour % 10;
			int num2 = (hour - num) / 10;
			int minute = dateTime.Minute;
			int num3 = minute % 10;
			int num4 = (minute - num3) / 10;
			watchTimeData.m_TimeHourDec.text = num2.ToString();
			watchTimeData.m_TimeHourUnit.text = num.ToString();
			watchTimeData.m_TimeMinuteDec.text = num4.ToString();
			watchTimeData.m_TimeMinuteUnit.text = num3.ToString();
			Localization localization = GreenHellGame.Instance.GetLocalization();
			string key = "Watch_" + EnumUtils<DayOfWeek>.GetName(dateTime.DayOfWeek);
			watchTimeData.m_DayName.text = localization.Get(key, true);
			switch (dateTime.Month)
			{
			case 1:
				key = "Watch_January";
				break;
			case 2:
				key = "Watch_February";
				break;
			case 3:
				key = "Watch_March";
				break;
			case 4:
				key = "Watch_April";
				break;
			case 5:
				key = "Watch_May";
				break;
			case 6:
				key = "Watch_June";
				break;
			case 7:
				key = "Watch_July";
				break;
			case 8:
				key = "Watch_August";
				break;
			case 9:
				key = "Watch_September";
				break;
			case 10:
				key = "Watch_October";
				break;
			case 11:
				key = "Watch_November";
				break;
			case 12:
				key = "Watch_December";
				break;
			}
			watchTimeData.m_MonthName.text = localization.Get(key, true);
			int day = dateTime.Day;
			int num5 = day % 10;
			int num6 = (day - num5) / 10;
			watchTimeData.m_DayDec.text = num6.ToString();
			watchTimeData.m_DayUnit.text = num5.ToString();
			return;
		}
		case Watch.State.Sanity:
		{
			WatchSanityData watchSanityData = (WatchSanityData)this.m_Datas[1];
			watchSanityData.m_Sanity.BeatsPerMinute = (int)CJTools.Math.GetProportionalClamp(Watch.MIN_BEATS_PER_SEC, Watch.MAX_BEATS_PER_SEC, (float)PlayerSanityModule.Get().m_Sanity, 1f, 0f);
			watchSanityData.m_SanityText.text = PlayerSanityModule.Get().m_Sanity.ToString();
			return;
		}
		case Watch.State.Macronutrients:
		{
			WatchMacronutrientsData watchMacronutrientsData = (WatchMacronutrientsData)this.m_Datas[2];
			float fillAmount = Player.Get().GetNutritionFat() / Player.Get().GetMaxNutritionFat();
			watchMacronutrientsData.m_Fat.fillAmount = fillAmount;
			float fillAmount2 = Player.Get().GetNutritionCarbo() / Player.Get().GetMaxNutritionCarbo();
			watchMacronutrientsData.m_Carbo.fillAmount = fillAmount2;
			float fillAmount3 = Player.Get().GetNutritionProtein() / Player.Get().GetMaxNutritionProtein();
			watchMacronutrientsData.m_Proteins.fillAmount = fillAmount3;
			float fillAmount4 = Player.Get().GetHydration() / Player.Get().GetMaxHydration();
			watchMacronutrientsData.m_Hydration.fillAmount = fillAmount4;
			return;
		}
		case Watch.State.Compass:
		{
			WatchCompassData watchCompassData = (WatchCompassData)this.m_Datas[3];
			Vector3 forward = Player.Get().gameObject.transform.forward;
			float num7 = Vector3.Angle(Vector3.forward, forward);
			if (forward.x < 0f)
			{
				num7 = 360f - num7;
			}
			Quaternion rotation = Quaternion.Euler(new Vector3(0f, 0f, num7));
			watchCompassData.m_Compass.transform.rotation = rotation;
			int num8 = 0;
			int num9 = 0;
			Player.Get().GetGPSCoordinates(out num8, out num9);
			watchCompassData.m_GPSCoordinates.text = num8.ToString() + "'W\n" + num9.ToString() + "'S";
			return;
		}
		default:
			return;
		}
	}

	public bool IsWatchTab(string state_name)
	{
		Watch.State state = (Watch.State)Enum.Parse(typeof(Watch.State), state_name);
		return this.m_State == state;
	}

	public void SetWatchTab(string state_name)
	{
		this.SetState((Watch.State)Enum.Parse(typeof(Watch.State), state_name));
	}

	public bool IsWatchTabActive(string state_name)
	{
		if (!WatchController.Get().enabled)
		{
			return false;
		}
		Watch.State state = (Watch.State)Enum.Parse(typeof(Watch.State), state_name);
		return this.m_State == state;
	}

	public void SetFakeDate(int day, int month)
	{
		DateTime dateTime = MainLevel.Instance.m_TODSky.Cycle.DateTime;
		dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
		int year = (month > dateTime.Month && day > dateTime.Day) ? (dateTime.Year - 1) : dateTime.Year;
		DateTime d = new DateTime(year, month, day);
		this.m_FakeDayOffset = (d - dateTime).Days;
	}

	public void ClearFakeDate()
	{
		this.m_FakeDayOffset = 0;
	}

	public Watch.State m_State = Watch.State.None;

	private Dictionary<int, WatchData> m_Datas = new Dictionary<int, WatchData>();

	private static float MAX_BEATS_PER_SEC = 240f;

	private static float MIN_BEATS_PER_SEC = 60f;

	public GameObject m_Canvas;

	private static Watch s_Instance = null;

	private int m_FakeDayOffset;

	public enum State
	{
		None = -1,
		Time,
		Sanity,
		Macronutrients,
		Compass,
		Count
	}
}
