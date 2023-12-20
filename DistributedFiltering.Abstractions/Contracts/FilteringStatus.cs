using Orleans;

namespace DistributedFiltering.Abstractions.Contracts;

[GenerateSerializer, Immutable]
public readonly struct FilteringStatus
{
	[Id(0)]
	public required FilteringState State { get; init; }
	[Id(1)]
	public required IReadOnlyList<SegmentFilteringStatus> SegmentStatuses { get; init; }
}
