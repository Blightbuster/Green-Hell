using System;
using Steamworks;

public class RemoteStorageSteam : IRemoteStorage
{
	public bool FileWrite(string file_name, byte[] data)
	{
		return SteamRemoteStorage.FileWrite(file_name, data, data.Length);
	}

	public int FileRead(string file_name, byte[] data, int length)
	{
		return SteamRemoteStorage.FileRead(file_name, data, length);
	}

	public int GetFileSize(string file_name)
	{
		return SteamRemoteStorage.GetFileSize(file_name);
	}

	public int GetFileCount()
	{
		return SteamRemoteStorage.GetFileCount();
	}

	public string GetFileNameAndSize(int index, out int file_size)
	{
		return SteamRemoteStorage.GetFileNameAndSize(index, out file_size);
	}

	public bool FileDelete(string file_name)
	{
		return SteamRemoteStorage.FileDelete(file_name);
	}

	public bool FileForget(string file_name)
	{
		return SteamRemoteStorage.FileForget(file_name);
	}

	public bool FileExistsInRemoteStorage(string save_name)
	{
		int fileCount = this.GetFileCount();
		for (int i = 0; i < fileCount; i++)
		{
			string text = string.Empty;
			int num = 0;
			text = this.GetFileNameAndSize(i, out num);
			if (num > 0 && text.ToLower() == save_name.ToLower())
			{
				return true;
			}
		}
		return false;
	}
}
