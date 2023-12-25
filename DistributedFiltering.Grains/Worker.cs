using AsyncAwaitBestPractices;
using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;

namespace DistributedFiltering.Grains;

[Reentrant]
public sealed class Worker(ILogger<Worker> logger) : IWorker
{
	private IBaseDistributedFilter? distributedFilter;
	private WorkState state = WorkState.NotStarted;

	public ValueTask CancelAsync()
	{
		logger.LogInformation("Canceling request for worker.");
		state = WorkState.Canceled;

		if (distributedFilter is not null)
		{
			logger.LogInformation("Canceling filter.");
			distributedFilter.Cancel();
		}

		return ValueTask.CompletedTask;
	}

	[ReadOnly]
	public ValueTask<SegmentFilteringStatus> GetStatusAsync()
	{
		var status = new SegmentFilteringStatus
		{
			Progress = distributedFilter?.GetProgress() ?? 0,
			State = state
		};

		logger.LogInformation("Worker (state: {state}) progress: {progress}", status.State, status.Progress);

		return ValueTask.FromResult(status);
	}

	public Task StartProcessingAsync<TFilter, TFilterParameters>(
		Batch batch,
		TFilterParameters parameters,
		int segmentIndex,
		IResultCollector collector)
		where TFilter : IDistributedFilter<TFilterParameters>, new()
		where TFilterParameters : IFilterParameters
	{
		if (state is not WorkState.Completed and not WorkState.Canceled and not WorkState.NotStarted)
		{
			logger.LogError("Worker is already working ... (status: {clusterStatus})", state);
			return Task.CompletedTask;
		}

		logger.LogInformation("Starting filter of type {filterType}.", typeof(TFilter));

		Task.Run(async () =>
		{
			state = WorkState.Preparing;

			var filter = new TFilter();
			distributedFilter = filter;

			state = WorkState.InProgress;
			logger.LogInformation("Filtering segment {segmentIndex}.", segmentIndex);
			var output = filter.Filter(batch, parameters);
			logger.LogInformation("Segment {segmentIndex} completed.", segmentIndex);

			await collector.ReportResultAsync(output, segmentIndex);
			state = WorkState.Completed;
		}).SafeFireAndForget();

		return Task.CompletedTask;
	}
}
