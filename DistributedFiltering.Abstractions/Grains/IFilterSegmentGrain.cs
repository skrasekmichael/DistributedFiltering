using DistributedFiltering.Abstractions.Contracts;
using Orleans;

namespace DistributedFiltering.Abstractions.Grains;

public interface IFilterSegmentGrain<TFilterParameters> : IGrainWithIntegerKey where TFilterParameters : IFilterParameters
{
	Task<byte[]> ApplyFilterAsync(Batch batch, TFilterParameters parameters);
	Task CancelAsync();
	Task<SegmentFilteringStatus> GetStatusAsync();
}
