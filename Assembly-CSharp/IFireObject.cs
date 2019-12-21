using System;

public interface IFireObject
{
	void Ignite();

	bool IsBurning();

	Construction GetConstruction();
}
