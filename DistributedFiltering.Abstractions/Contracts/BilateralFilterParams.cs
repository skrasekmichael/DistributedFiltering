using DistributedFiltering.Abstractions.Interfaces;

namespace DistributedFiltering.Abstractions.Contracts;

[GenerateSerializer, Immutable]
public readonly struct BilateralFilterParams : IFilterParameters
{
	[Id(0)]
	public required double SpatialSigma { get; init; }
	[Id(1)]
	public required double RangeSigma { get; init; }

	public readonly int GetOverlap() => GetRadius();
	public readonly int GetRadius() => (int)(2.5 * SpatialSigma);
}
