using DistributedFiltering.Abstractions.Contracts;

namespace DistributedFiltering.Abstractions.Grains;

public interface IFilterGrain<TFilterParameters> : IGrainWithGuidKey where TFilterParameters : IFilterParameters
{
	Task<ImageData> FilterAsync(ImageData input, TFilterParameters parameters);
	Task StopFilteringAsync();
	Task<FilteringStatus> GetStatusAsync();
}
