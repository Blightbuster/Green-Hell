using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScriptParser
{
	public ScriptParser()
	{
		this.m_Position = 0;
	}

	public string GetText()
	{
		return this.m_Text;
	}

	public int Position
	{
		get
		{
			return this.m_Position;
		}
		set
		{
			this.m_Position = value;
		}
	}

	public bool Parse(string file_name, bool from_asset = true)
	{
		string text = (!from_asset) ? (Application.dataPath + "/" + file_name) : ("Scripts/" + ((file_name.LastIndexOf(".") < 0) ? file_name : file_name.Substring(0, file_name.LastIndexOf("."))));
		if (from_asset)
		{
			this.m_TextAsset = (Resources.Load(text) as TextAsset);
			if (!this.m_TextAsset)
			{
				Debug.Log("[ScriptParser::Parse] Cannot load resource " + text);
				return false;
			}
			this.m_Text = this.m_TextAsset.text;
		}
		else if (File.Exists(text))
		{
			StreamReader streamReader = File.OpenText(text);
			if (streamReader != null)
			{
				this.m_Text = streamReader.ReadToEnd();
				streamReader.Close();
			}
		}
		if (this.m_Text.Length <= 0)
		{
			Debug.Log("[ScriptParser::Parse] Loaded file does not contain any data - " + file_name);
			return false;
		}
		this.ParseTokens();
		this.ProcessTokens();
		return true;
	}

	public bool Write(string file_name, bool display_warning_message = true)
	{
		string path = Application.dataPath + "/" + file_name;
		if (!display_warning_message || File.Exists(path))
		{
		}
		StreamWriter streamWriter = new StreamWriter(path);
		if (streamWriter == null)
		{
			return false;
		}
		for (int i = 0; i < this.m_Keys.Count; i++)
		{
			Key key = this.m_Keys[i];
			key.Write(streamWriter, 0);
			streamWriter.Write("\n");
		}
		streamWriter.Close();
		return true;
	}

	protected void ParseTokens()
	{
		while (this.m_Position < this.m_Text.Length)
		{
			this.SkipWhiteSpaces();
			if (this.m_Position >= this.m_Text.Length)
			{
				break;
			}
			if (!this.ParseToken())
			{
				this.m_Position++;
			}
		}
	}

	private void SkipWhiteSpaces()
	{
		string text = this.m_Text[this.m_Position].ToString();
		while (text.IndexOfAny(ScriptParser.WHITE_SPACES) != -1 || text.IndexOfAny(ScriptParser.LINE_END) != -1 || text.IndexOfAny(ScriptParser.TABS) != -1)
		{
			this.m_Position++;
			if (this.m_Position >= this.m_Text.Length)
			{
				break;
			}
			text = this.m_Text[this.m_Position].ToString();
		}
	}

	private bool ParseToken()
	{
		bool result = true;
		TokenComment tokenComment = new TokenComment(this);
		TokenKeyword tokenKeyword = new TokenKeyword(this);
		TokenLeftBracket tokenLeftBracket = new TokenLeftBracket(this);
		TokenRightBracket tokenRightBracket = new TokenRightBracket(this);
		TokenString tokenString = new TokenString(this);
		TokenComma tokenComma = new TokenComma(this);
		TokenFloat tokenFloat = new TokenFloat(this);
		TokenInt tokenInt = new TokenInt(this);
		TokenBool tokenBool = new TokenBool(this);
		TokenLeftCurlyBracket tokenLeftCurlyBracket = new TokenLeftCurlyBracket(this);
		TokenRightCurlyBracket tokenRightCurlyBracket = new TokenRightCurlyBracket(this);
		if (tokenComment.TryToGet())
		{
			this.m_Tokens.Add(tokenComment);
		}
		else if (tokenBool.TryToGet())
		{
			this.m_Tokens.Add(tokenBool);
		}
		else if (tokenKeyword.TryToGet())
		{
			this.m_Tokens.Add(tokenKeyword);
		}
		else if (tokenLeftBracket.TryToGet())
		{
			this.m_Tokens.Add(tokenLeftBracket);
		}
		else if (tokenRightBracket.TryToGet())
		{
			this.m_Tokens.Add(tokenRightBracket);
		}
		else if (tokenString.TryToGet())
		{
			this.m_Tokens.Add(tokenString);
		}
		else if (tokenComma.TryToGet())
		{
			this.m_Tokens.Add(tokenComma);
		}
		else if (tokenFloat.TryToGet())
		{
			this.m_Tokens.Add(tokenFloat);
		}
		else if (tokenInt.TryToGet())
		{
			this.m_Tokens.Add(tokenInt);
		}
		else if (tokenLeftCurlyBracket.TryToGet())
		{
			this.m_Tokens.Add(tokenLeftCurlyBracket);
		}
		else if (tokenRightCurlyBracket.TryToGet())
		{
			this.m_Tokens.Add(tokenRightCurlyBracket);
		}
		else
		{
			result = false;
		}
		return result;
	}

	protected void ProcessTokens()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		Key key = null;
		for (int i = 0; i < this.m_Tokens.Count; i++)
		{
			Token token = this.m_Tokens[i];
			if (token.GetType() == typeof(TokenKeyword))
			{
				if (flag)
				{
					Debug.Log("Parsing error around " + token.GetValue());
					return;
				}
				Key key2 = new Key(token.GetValue());
				if (key != null)
				{
					if (flag5)
					{
						key2.m_Parent = key;
					}
					else if (key.m_Parent != null)
					{
						key2.m_Parent = key.m_Parent;
					}
				}
				key = key2;
				if (key.m_Parent != null)
				{
					key.m_Parent.AddKey(key);
				}
				else
				{
					this.m_Keys.Add(key);
				}
				flag = true;
				flag5 = false;
			}
			else if (token.GetType() == typeof(TokenLeftBracket))
			{
				if (!flag || flag2)
				{
					Debug.Log(("Parsing error around " + key == null) ? "unknown" : key.GetName());
					return;
				}
				flag2 = true;
			}
			else if (token.GetType() == typeof(TokenRightBracket))
			{
				if (!flag || !flag2)
				{
					Debug.Log(("Parsing error around " + key == null) ? "unknown" : key.GetName());
					return;
				}
				flag2 = false;
				flag = false;
			}
			else if (token.GetType() == typeof(TokenString))
			{
				if (!flag || !flag2)
				{
					Debug.Log(("Parsing error around " + key == null) ? "unknown" : token.GetValue());
					return;
				}
				key.AddVariable(new CJVariable
				{
					SValue = token.GetValue()
				});
			}
			else if (token.GetType() == typeof(TokenFloat))
			{
				if (!flag || !flag2)
				{
					Debug.Log(("Parsing error around " + key == null) ? "unknown" : token.GetValue());
					return;
				}
				key.AddVariable(new CJVariable
				{
					FValue = float.Parse(token.GetValue())
				});
			}
			else if (token.GetType() == typeof(TokenInt))
			{
				if (!flag || !flag2)
				{
					Debug.Log(("Parsing error around " + key == null) ? "unknown" : token.GetValue());
					return;
				}
				key.AddVariable(new CJVariable
				{
					IValue = int.Parse(token.GetValue())
				});
			}
			else if (token.GetType() == typeof(TokenBool))
			{
				if (!flag || !flag2)
				{
					Debug.Log(("Parsing error around " + key == null) ? "unknown" : token.GetValue());
					return;
				}
				key.AddVariable(new CJVariable
				{
					BValue = bool.Parse(token.GetValue())
				});
			}
			else if (token.GetType() == typeof(TokenComma))
			{
				if (!flag || !flag2)
				{
					Debug.Log(("Parsing error around " + key == null) ? "unknown" : key.GetName());
					return;
				}
			}
			else if (token.GetType() == typeof(TokenLeftCurlyBracket))
			{
				if (flag || flag2 || key == null)
				{
					Debug.Log(("Parsing error around " + key == null) ? "unknown" : key.GetName());
					return;
				}
				flag3 = true;
				flag4 = false;
				flag5 = true;
			}
			else if (token.GetType() == typeof(TokenRightCurlyBracket))
			{
				if (flag || flag2 || key == null || (!flag3 && !flag4))
				{
					Debug.Log(("Parsing error around " + key == null) ? "unknown" : key.GetName());
					return;
				}
				flag3 = false;
				flag4 = true;
				key = key.m_Parent;
				if (key != null && key.m_Parent != null)
				{
					flag3 = true;
				}
			}
		}
	}

	public int GetKeysCount()
	{
		return this.m_Keys.Count;
	}

	public Key GetKey(int index)
	{
		if (index < 0 || index >= this.m_Keys.Count)
		{
			return null;
		}
		return this.m_Keys[index];
	}

	public Key AddKey(string key_name)
	{
		Key key = new Key(key_name);
		this.m_Keys.Add(key);
		return key;
	}

	private TextAsset m_TextAsset;

	protected string m_Text = string.Empty;

	private int m_Position;

	public static char[] WHITE_SPACES = new char[]
	{
		' '
	};

	public static char[] TABS = new char[]
	{
		'\t'
	};

	public static char[] LINE_END = new char[]
	{
		'\n'
	};

	public static string LETTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

	public static string DIGITS = "0123456789";

	public static char COMA = ',';

	public static char PERIOD = '.';

	public static char MINUS = '-';

	public static char UNDERLINE = '_';

	private List<Token> m_Tokens = new List<Token>();

	public List<Key> m_Keys = new List<Key>();
}
