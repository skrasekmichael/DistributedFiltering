namespace DistributedFiltering.Abstractions.Contracts;

[GenerateSerializer, Immutable]
public struct SegmentFilteringStatus
{
	[Id(0)]
	public double Progress { get; set; }
	[Id(1)]
	public WorkState State { get; set; }
}
