using DistributedFiltering.Abstractions.Contracts;

namespace DistributedFiltering.Abstractions.Interfaces;

public interface IClusterGrain : IGrainWithIntegerKey
{
	Task<bool> DistributeWorkAsync<TFilterParameters>(ImageData input, TFilterParameters parameters)
		where TFilterParameters : IFilterParameters;

	ValueTask StopProcessingAsync();
	ValueTask<FilteringStatus> GetStatusAsync();

	Task RegisterWorkerAsync(IWorker worker);
	Task UnregisterWorkerAsync(IWorker worker);

	Task RegisterCollectorAsync(IResultCollector collector);
	Task UnregisterCollectorAsync(IResultCollector collector);

	Task ReportBatchResultAsync(byte[] data, int orderingIndex, Guid workerId);
}
