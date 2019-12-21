using System;

public enum P2PNetworkError
{
	Ok,
	WrongHost,
	WrongConnection,
	WrongChannel,
	NoResources,
	BadMessage,
	Timeout,
	MessageToLong,
	WrongOperation,
	VersionMismatch,
	CRCMismatch,
	DNSFailure
}
