using System;

public interface IRemoteStorage
{
	bool FileWrite(string file_name, byte[] data);

	int FileRead(string file_name, byte[] data, int length);

	int GetFileSize(string file_name);

	int GetFileCount();

	string GetFileNameAndSize(int index, out int file_size);

	bool FileDelete(string file_name);

	bool FileForget(string file_name);

	bool FileExistsInRemoteStorage(string save_name);
}
