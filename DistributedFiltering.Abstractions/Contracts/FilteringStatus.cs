using Orleans;

namespace DistributedFiltering.Abstractions.Contracts;

[GenerateSerializer, Immutable]
public readonly struct FilteringStatus
{
	[Id(0)]
	public required WorkState State { get; init; }
	[Id(1)]
	public required double Progress { get; init; }
	[Id(2)]
	public required IReadOnlyList<SegmentFilteringStatus> SegmentStatuses { get; init; }
}
