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
			int num = (int)MainLevel.Instance.m_TODSky.Cycle.Hour;
			int num2 = num % 10;
			int num3 = (num - num2) / 10;
			int num4 = (int)((MainLevel.Instance.m_TODSky.Cycle.Hour - (float)num) * 60f);
			int num5 = num4 % 10;
			int num6 = (num4 - num5) / 10;
			watchTimeData.m_TimeHourDec.text = num3.ToString();
			watchTimeData.m_TimeHourUnit.text = num2.ToString();
			watchTimeData.m_TimeMinuteDec.text = num6.ToString();
			watchTimeData.m_TimeMinuteUnit.text = num5.ToString();
			Localization localization = GreenHellGame.Instance.GetLocalization();
			string key = "Watch_" + MainLevel.Instance.m_TODSky.Cycle.DateTime.DayOfWeek.ToString();
			watchTimeData.m_DayName.text = localization.Get(key);
			switch (MainLevel.Instance.m_TODSky.Cycle.DateTime.Month)
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
			watchTimeData.m_MonthName.text = localization.Get(key);
			int day = MainLevel.Instance.m_TODSky.Cycle.DateTime.Day;
			int num7 = day % 10;
			int num8 = (day - num7) / 10;
			watchTimeData.m_DayDec.text = num8.ToString();
			watchTimeData.m_DayUnit.text = num7.ToString();
			break;
		}
		case Watch.State.Sanity:
		{
			WatchSanityData watchSanityData = (WatchSanityData)this.m_Datas[1];
			watchSanityData.m_Sanity.BeatsPerMinute = (int)CJTools.Math.GetProportionalClamp(Watch.MIN_BEATS_PER_SEC, Watch.MAX_BEATS_PER_SEC, (float)PlayerSanityModule.Get().m_Sanity, 1f, 0f);
			watchSanityData.m_SanityText.text = PlayerSanityModule.Get().m_Sanity.ToString();
			break;
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
			break;
		}
		case Watch.State.Compass:
		{
			WatchCompassData watchCompassData = (WatchCompassData)this.m_Datas[3];
			Vector3 forward = Player.Get().gameObject.transform.forward;
			float num9 = Vector3.Angle(Vector3.forward, forward);
			if (forward.x < 0f)
			{
				num9 = 360f - num9;
			}
			Quaternion rotation = Quaternion.Euler(new Vector3(0f, 0f, num9));
			watchCompassData.m_Compass.transform.rotation = rotation;
			int num10 = 0;
			int num11 = 0;
			Player.Get().GetGPSCoordinates(out num10, out num11);
			watchCompassData.m_GPSCoordinates.text = num10.ToString() + "'W\n" + num11.ToString() + "'S";
			break;
		}
		}
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

	public Watch.State m_State = Watch.State.None;

	private Dictionary<int, WatchData> m_Datas = new Dictionary<int, WatchData>();

	private static float MAX_BEATS_PER_SEC = 240f;

	private static float MIN_BEATS_PER_SEC = 60f;

	public GameObject m_Canvas;

	private static Watch s_Instance;

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
