using DistributedFiltering.Abstractions.Contracts;
using Orleans.Concurrency;

namespace DistributedFiltering.Abstractions.Interfaces;

public interface IClusterGrain : IGrainWithIntegerKey
{
	Task<bool> DistributeWorkAsync<TFilterParameters>(ImageData input, TFilterParameters parameters, int maxBatchSize, string output)
		where TFilterParameters : IFilterParameters;

	ValueTask StopProcessingAsync();

	[AlwaysInterleave]
	ValueTask<FilteringStatus> GetStatusAsync();

	Task RegisterWorkerAsync(IWorker worker);
	Task UnregisterWorkerAsync(IWorker worker);

	Task RegisterCollectorAsync(IResultCollector collector);
	Task UnregisterCollectorAsync(IResultCollector collector);

	Task ReportBatchResultAsync(byte[] data, int orderingIndex, Guid workerId);
}
