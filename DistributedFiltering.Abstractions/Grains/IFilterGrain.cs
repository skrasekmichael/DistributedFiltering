using DistributedFiltering.Abstractions.Contracts;

namespace DistributedFiltering.Abstractions.Grains;

public interface IFilterGrain<TFilterSegmentGrain, TFilterParameters> : IGrainWithGuidKey
	where TFilterSegmentGrain : IFilterSegmentGrain<TFilterParameters>
	where TFilterParameters : IFilterParameters
{
	Task<ImageData> FilterAsync(ImageData input, TFilterParameters parameters);
	Task StopFilteringAsync();
	Task<FilteringStatus> GetStatusAsync();
}
