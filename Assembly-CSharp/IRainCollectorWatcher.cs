using System;

public interface IRainCollectorWatcher
{
	void Register(IRainCollector collector);

	void UnRegister(IRainCollector collector);
}
