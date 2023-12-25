using DistributedFiltering.Abstractions.Interfaces;

namespace DistributedFiltering.Abstractions.Contracts;

[GenerateSerializer, Immutable]
public readonly struct GaussianNoiseParams : IFilterParameters
{
	[Id(0)]
	public double Sigma { get; init; }

	public int GetOverlap() => 0;
}
