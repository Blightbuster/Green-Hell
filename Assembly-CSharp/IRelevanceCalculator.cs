using System;

public interface IRelevanceCalculator
{
	float CalculateRelevance(IPeerWorldRepresentation peer, bool is_owner);

	bool CanBeRemovedByRelevance(bool on_owner);
}
