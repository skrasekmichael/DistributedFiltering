namespace DistributedFiltering.Abstractions.Contracts;

public interface IFilterParameters
{
	public int UnitCount { get; init; }

	public int GetOverlap();
}
