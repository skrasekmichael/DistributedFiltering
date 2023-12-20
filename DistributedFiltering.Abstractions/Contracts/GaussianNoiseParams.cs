namespace DistributedFiltering.Abstractions.Contracts;

[GenerateSerializer, Immutable]
public readonly struct GaussianNoiseParams : IFilterParameters
{
	[Id(0)]
	public int UnitCount { get; init; }
	[Id(1)]
	public double Sigma { get; init; }

	public int GetOverlap() => 0;
}
