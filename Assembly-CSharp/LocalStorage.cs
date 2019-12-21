using System;
using System.IO;
using UnityEngine;

public class LocalStorage : IRemoteStorage
{
	public bool FileWrite(string file_name, byte[] data)
	{
		bool result;
		try
		{
			using (FileStream fileStream = new FileStream(this.GetPath() + file_name, FileMode.Create, FileAccess.Write))
			{
				fileStream.Write(data, 0, data.Length);
			}
			result = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			result = false;
		}
		return result;
	}

	public int FileRead(string file_name, byte[] data, int length)
	{
		FileInfo fileInfo = new FileInfo(this.GetPath() + file_name);
		int result = 0;
		using (FileStream fileStream = fileInfo.OpenRead())
		{
			fileStream.Read(data, 0, (int)fileStream.Length);
			result = (int)fileStream.Length;
		}
		return result;
	}

	public int GetFileSize(string file_name)
	{
		return (int)new FileInfo(this.GetPath() + file_name).Length;
	}

	public int GetFileCount()
	{
		this.m_FileNames = Directory.GetFiles(this.GetPath(), "*.sav");
		return this.m_FileNames.Length;
	}

	public string GetFileNameAndSize(int index, out int file_size)
	{
		if (index < 0)
		{
			file_size = 0;
			return string.Empty;
		}
		if (this.m_FileNames == null)
		{
			this.GetFileCount();
		}
		if (index < this.m_FileNames.Length)
		{
			FileInfo fileInfo = new FileInfo(this.m_FileNames[index]);
			file_size = (int)fileInfo.Length;
			return Path.GetFileName(fileInfo.Name);
		}
		file_size = 0;
		return string.Empty;
	}

	public bool FileDelete(string file_name)
	{
		if (this.FileExistsInRemoteStorage(file_name))
		{
			File.Delete(this.GetPath() + file_name);
			return true;
		}
		return false;
	}

	public bool FileForget(string file_name)
	{
		return true;
	}

	public bool FileExistsInRemoteStorage(string file_name)
	{
		return File.Exists(this.GetPath() + file_name);
	}

	protected virtual string GetPath()
	{
		return Application.persistentDataPath + "/";
	}

	private string[] m_FileNames;
}
