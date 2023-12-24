using DistributedFiltering.Abstractions.Contracts;
using Orleans.Concurrency;

namespace DistributedFiltering.Abstractions.Interfaces;

public interface IWorker : IGrainObserver, IAsyncCancelable
{
	public Task StartProcessingAsync<TFilter, TFilterParameters>(
		Batch batch,
		TFilterParameters parameters,
		int segmentIndex,
		IResultCollector collector)
		where TFilter : IDistributedFilter<TFilterParameters>, new()
		where TFilterParameters : IFilterParameters;

	[ReadOnly]
	public ValueTask<SegmentFilteringStatus> GetStatusAsync();
}
