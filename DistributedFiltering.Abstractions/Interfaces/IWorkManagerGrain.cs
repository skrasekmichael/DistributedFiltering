using DistributedFiltering.Abstractions.Contracts;

namespace DistributedFiltering.Abstractions.Interfaces;

public interface IWorkManagerGrain : IGrainWithIntegerKey
{
	Task DistributeWorkAsync<TFilterGrain, TFilterParameters>(ImageData input, TFilterParameters parameters)
		where TFilterGrain : IDistributedFilter<TFilterParameters>, new()
		where TFilterParameters : IFilterParameters;

	ValueTask StopProcessingAsync();
	ValueTask<FilteringStatus> GetStatusAsync();

	Task RegisterWorkerAsync(IWorker worker);
	Task UnregisterWorkerAsync(IWorker worker);

	Task RegisterCollectorAsync(IResultCollector collector);
	Task UnregisterCollectorAsync(IResultCollector collector);

	Task FinalizeAsync();
}
