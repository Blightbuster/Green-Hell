using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UberLogger;
using UnityEngine;

public class UberLoggerAppWindow : MonoBehaviour, UberLogger.ILogger
{
	public void Log(LogInfo logInfo)
	{
		this.LogInfo.Add(logInfo);
		if (logInfo.Severity == LogSeverity.Error)
		{
			this.NoErrors++;
		}
		else if (logInfo.Severity == LogSeverity.Warning)
		{
			this.NoWarnings++;
		}
		else
		{
			this.NoMessages++;
		}
		if (logInfo.Severity == LogSeverity.Error && this.PauseOnError)
		{
			Debug.Break();
		}
	}

	private void Clear()
	{
		this.LogInfo.Clear();
		this.NoWarnings = 0;
		this.NoErrors = 0;
		this.NoMessages = 0;
	}

	private void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		UberLogger.Logger.AddLogger(this, true);
		this.ClearSelectedMessage();
		this.WindowRect = new Rect(0f, 0f, (float)(Screen.width / 2), (float)Screen.height);
		this.CurrentTopPaneHeight = (float)Screen.height * this.SizerStartHeightRatio;
	}

	public bool ShowWindow { get; set; }

	public void OnGUI()
	{
		GUI.skin = this.Skin;
		if (this.ShowWindow)
		{
			Color color = GUI.color;
			GUI.color = this.GUIColour;
			this.WindowRect = new Rect(0f, 0f, (float)(Screen.width / 2), (float)Screen.height);
			this.LogLineStyle1 = this.Skin.customStyles[0];
			this.LogLineStyle2 = this.Skin.customStyles[1];
			this.SelectedLogLineStyle = this.Skin.customStyles[2];
			this.LogLineStyle1.fontSize = this.FontSize;
			this.LogLineStyle2.fontSize = this.FontSize;
			this.SelectedLogLineStyle.fontSize = this.FontSize;
			this.WindowRect = GUILayout.Window(1, this.WindowRect, new GUI.WindowFunction(this.DrawWindow), "Uber Console", GUI.skin.window, Array.Empty<GUILayoutOption>());
			GUI.color = color;
			return;
		}
		this.DrawActivationButton();
	}

	public void DrawActivationButton()
	{
		Texture2D image = this.ButtonTexture;
		if (this.NoErrors > 0)
		{
			image = this.ErrorButtonTexture;
		}
		Vector2 buttonPosition = this.ButtonPosition;
		buttonPosition.x *= (float)Screen.width;
		buttonPosition.y *= (float)Screen.height;
		if (buttonPosition.x + this.ButtonSize.x > (float)Screen.width)
		{
			buttonPosition.x = (float)Screen.width - this.ButtonSize.x;
		}
		if (buttonPosition.y + this.ButtonSize.y > (float)Screen.height)
		{
			buttonPosition.y = (float)Screen.height - this.ButtonSize.y;
		}
		Rect position = new Rect(buttonPosition.x, buttonPosition.y, this.ButtonSize.x, this.ButtonSize.y);
		GUIStyle style = new GUIStyle();
		if (GUI.Button(position, image, style))
		{
			this.ShowWindow = !this.ShowWindow;
		}
	}

	private void DrawWindow(int windowID)
	{
		Color color = GUI.color;
		GUI.color = this.GUIColour;
		GUILayout.BeginVertical(new GUILayoutOption[]
		{
			GUILayout.Height(this.CurrentTopPaneHeight - (float)GUI.skin.window.padding.top),
			GUILayout.MinHeight(100f)
		});
		this.DrawToolbar();
		this.DrawFilter();
		this.DrawChannels();
		this.DrawLogList();
		GUILayout.EndVertical();
		this.ResizeTopPane();
		GUILayout.Space(10f);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		this.DrawLogDetails();
		GUILayout.EndVertical();
		GUI.color = color;
		this.DrawActivationButton();
	}

	private bool ButtonClamped(string text, GUIStyle style)
	{
		return GUILayout.Button(text, style, new GUILayoutOption[]
		{
			GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x)
		});
	}

	private bool ToggleClamped(bool state, string text, GUIStyle style)
	{
		return GUILayout.Toggle(state, text, style, new GUILayoutOption[]
		{
			GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x)
		});
	}

	private bool ToggleClamped(bool state, GUIContent content, GUIStyle style, params GUILayoutOption[] par)
	{
		return GUILayout.Toggle(state, content, style, new GUILayoutOption[]
		{
			GUILayout.MaxWidth(style.CalcSize(content).x)
		});
	}

	private void LabelClamped(string text, GUIStyle style)
	{
		GUILayout.Label(text, style, new GUILayoutOption[]
		{
			GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x)
		});
	}

	private void DrawToolbar()
	{
		GUIStyle guistyle = GUI.skin.customStyles[3];
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (this.ButtonClamped("Clear", guistyle))
		{
			this.Clear();
		}
		this.ShowTimes = this.ToggleClamped(this.ShowTimes, "Show Times", guistyle);
		float y = guistyle.CalcSize(new GUIContent("T")).y;
		GUILayout.FlexibleSpace();
		bool flag = this.ToggleClamped(this.ShowErrors, new GUIContent(this.NoErrors.ToString(), this.SmallErrorIcon), guistyle, new GUILayoutOption[]
		{
			GUILayout.Height(y)
		});
		bool flag2 = this.ToggleClamped(this.ShowWarnings, new GUIContent(this.NoWarnings.ToString(), this.SmallWarningIcon), guistyle, new GUILayoutOption[]
		{
			GUILayout.Height(y)
		});
		bool flag3 = this.ToggleClamped(this.ShowMessages, new GUIContent(this.NoMessages.ToString(), this.SmallMessageIcon), guistyle, new GUILayoutOption[]
		{
			GUILayout.Height(y)
		});
		if (flag != this.ShowErrors || flag2 != this.ShowWarnings || flag3 != this.ShowMessages)
		{
			this.ClearSelectedMessage();
		}
		this.ShowWarnings = flag2;
		this.ShowMessages = flag3;
		this.ShowErrors = flag;
		GUILayout.EndHorizontal();
	}

	private void DrawChannels()
	{
		List<string> channels = this.GetChannels();
		int num = 0;
		for (int i = 0; i < channels.Count; i++)
		{
			if (channels[i] == this.CurrentChannel)
			{
				num = i;
				break;
			}
		}
		num = GUILayout.SelectionGrid(num, channels.ToArray(), channels.Count, Array.Empty<GUILayoutOption>());
		if (this.CurrentChannel != channels[num])
		{
			this.CurrentChannel = channels[num];
			this.ClearSelectedMessage();
		}
	}

	private bool ShouldShowLog(Regex regex, LogInfo log)
	{
		return (log.Channel == this.CurrentChannel || this.CurrentChannel == "All" || (this.CurrentChannel == "No Channel" && string.IsNullOrEmpty(log.Channel))) && ((log.Severity == LogSeverity.Message && this.ShowMessages) || (log.Severity == LogSeverity.Warning && this.ShowWarnings) || (log.Severity == LogSeverity.Error && this.ShowErrors)) && (regex == null || regex.IsMatch(log.Message));
	}

	public void DrawLogList()
	{
		Color backgroundColor = GUI.backgroundColor;
		this.LogListScrollPosition = GUILayout.BeginScrollView(this.LogListScrollPosition, Array.Empty<GUILayoutOption>());
		float height = this.WindowRect.height;
		float num = 0f;
		float y = this.LogLineStyle1.CalcSize(new GUIContent("Test")).y;
		Regex regex = null;
		if (!string.IsNullOrEmpty(this.FilterRegex))
		{
			regex = new Regex(this.FilterRegex);
		}
		int num2 = 0;
		GUIStyle style = this.LogLineStyle1;
		for (int i = 0; i < this.LogInfo.Count; i++)
		{
			LogInfo logInfo = this.LogInfo[i];
			if (this.ShouldShowLog(regex, logInfo))
			{
				num2++;
				if (num + y > this.LogListScrollPosition.y && num < this.LogListScrollPosition.y + height)
				{
					if (i == this.SelectedMessage)
					{
						style = this.SelectedLogLineStyle;
					}
					else
					{
						style = ((num2 % 2 == 0) ? this.LogLineStyle1 : this.LogLineStyle2);
					}
					string text = logInfo.Message;
					text = text.Replace(UberLogger.Logger.UnityInternalNewLine, " ");
					if (this.ShowTimes)
					{
						text = logInfo.GetRelativeTimeStampAsString() + ": " + text;
					}
					if (GUILayout.Button(new GUIContent(text, this.GetIconForLog(logInfo)), style, new GUILayoutOption[]
					{
						GUILayout.Height(y)
					}))
					{
						if (i == this.SelectedMessage)
						{
							if ((double)Time.realtimeSinceStartup - this.LastMessageClickTime < 0.30000001192092896)
							{
								this.LastMessageClickTime = 0.0;
							}
							else
							{
								this.LastMessageClickTime = (double)Time.realtimeSinceStartup;
							}
						}
						else
						{
							this.SelectedMessage = i;
							this.SelectedCallstackFrame = -1;
						}
					}
				}
				else
				{
					GUILayout.Space(y);
				}
				num += y;
			}
		}
		GUILayout.EndScrollView();
		GUI.backgroundColor = backgroundColor;
	}

	public void DrawLogDetails()
	{
		Color backgroundColor = GUI.backgroundColor;
		this.SelectedMessage = Mathf.Clamp(this.SelectedMessage, 0, this.LogInfo.Count);
		if (this.LogInfo.Count > 0 && this.SelectedMessage >= 0)
		{
			this.LogDetailsScrollPosition = GUILayout.BeginScrollView(this.LogDetailsScrollPosition, Array.Empty<GUILayoutOption>());
			LogInfo logInfo = this.LogInfo[this.SelectedMessage];
			GUIStyle style = this.LogLineStyle1;
			for (int i = 0; i < logInfo.Callstack.Count; i++)
			{
				string formattedMethodNameWithFileName = logInfo.Callstack[i].GetFormattedMethodNameWithFileName();
				if (!string.IsNullOrEmpty(formattedMethodNameWithFileName))
				{
					if (i == this.SelectedCallstackFrame)
					{
						style = this.SelectedLogLineStyle;
					}
					else
					{
						style = ((i % 2 == 0) ? this.LogLineStyle1 : this.LogLineStyle2);
					}
					if (GUILayout.Button(formattedMethodNameWithFileName, style, Array.Empty<GUILayoutOption>()))
					{
						this.SelectedCallstackFrame = i;
					}
				}
			}
			GUILayout.EndScrollView();
		}
		GUI.backgroundColor = backgroundColor;
	}

	private Texture2D GetIconForLog(LogInfo log)
	{
		if (log.Severity == LogSeverity.Error)
		{
			return this.SmallErrorIcon;
		}
		if (log.Severity == LogSeverity.Warning)
		{
			return this.SmallWarningIcon;
		}
		return this.SmallMessageIcon;
	}

	private void DrawFilter()
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		this.LabelClamped("Filter Regex", GUI.skin.label);
		string text = GUILayout.TextArea(this.FilterRegex, Array.Empty<GUILayoutOption>());
		if (this.ButtonClamped("Clear", GUI.skin.button))
		{
			text = "";
			GUIUtility.keyboardControl = 0;
			GUIUtility.hotControl = 0;
		}
		if (text != this.FilterRegex)
		{
			this.ClearSelectedMessage();
			this.FilterRegex = text;
		}
		GUILayout.EndHorizontal();
	}

	private List<string> GetChannels()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (LogInfo logInfo in this.LogInfo)
		{
			if (!string.IsNullOrEmpty(logInfo.Channel) && !hashSet.Contains(logInfo.Channel))
			{
				hashSet.Add(logInfo.Channel);
			}
		}
		List<string> list = new List<string>();
		list.Add("All");
		list.Add("No Channel");
		list.AddRange(hashSet);
		return list;
	}

	private void ResizeTopPane()
	{
		float num = 0f;
		Rect position = new Rect(0f, this.CurrentTopPaneHeight + num, this.WindowRect.width, 5f);
		Color color = GUI.color;
		GUI.color = this.SizerLineColour;
		GUI.DrawTexture(position, Texture2D.whiteTexture);
		GUI.color = color;
		if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition))
		{
			this.Resizing = true;
		}
		if (this.Resizing)
		{
			this.CurrentTopPaneHeight = Event.current.mousePosition.y;
		}
		if (Event.current.type == EventType.MouseUp)
		{
			this.Resizing = false;
		}
		this.CurrentTopPaneHeight = Mathf.Clamp(this.CurrentTopPaneHeight, 100f, this.WindowRect.height - 100f);
	}

	private void ClearSelectedMessage()
	{
		this.SelectedMessage = -1;
		this.SelectedCallstackFrame = -1;
	}

	public GUISkin Skin;

	public Texture2D SmallErrorIcon;

	public Texture2D SmallWarningIcon;

	public Texture2D SmallMessageIcon;

	public Color GUIColour = new Color(1f, 1f, 1f, 0.5f);

	public int FontSize;

	public Color SizerLineColour = new Color(0.164705887f, 0.164705887f, 0.164705887f);

	public float SizerStartHeightRatio = 0.75f;

	public Texture2D ButtonTexture;

	public Texture2D ErrorButtonTexture;

	public Vector2 ButtonPosition;

	public Vector2 ButtonSize = new Vector2(32f, 32f);

	private bool Resizing;

	private Vector2 LogListScrollPosition;

	private Vector2 LogDetailsScrollPosition;

	private bool ShowTimes = true;

	private float CurrentTopPaneHeight;

	private int SelectedMessage = -1;

	private double LastMessageClickTime;

	private List<LogInfo> LogInfo = new List<LogInfo>();

	private bool PauseOnError;

	private int NoErrors;

	private int NoWarnings;

	private int NoMessages;

	private Rect WindowRect = new Rect(0f, 0f, 100f, 100f);

	private GUIStyle LogLineStyle1;

	private GUIStyle LogLineStyle2;

	private GUIStyle SelectedLogLineStyle;

	private string CurrentChannel;

	private string FilterRegex = "";

	private bool ShowErrors = true;

	private bool ShowWarnings = true;

	private bool ShowMessages = true;

	private int SelectedCallstackFrame;
}
