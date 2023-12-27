using AsyncAwaitBestPractices;
using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Filters.Extensions;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;

namespace DistributedFiltering.Client;

public sealed class Worker(ILogger<Worker> logger, IClusterGrain workManagerGrain) : IWorker
{
	private IDistributedFilter? distributedFilter;
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

	[ReadOnly]
	public Task PingAsync() => Task.CompletedTask;

	public Task<bool> StartProcessingAsync(Batch batch, Guid id)
	{
		if (state is not WorkState.Completed and not WorkState.Canceled and not WorkState.NotStarted)
		{
			logger.LogError("Worker is already working ... (status: {clusterStatus})", state);
			return Task.FromResult(false);
		}

		state = WorkState.Preparing;

		try
		{
			var filter = batch.Parameters.GetFilter();
			distributedFilter = filter;
		}
		catch
		{
			state = WorkState.Canceled;
			logger.LogError("Worker is already working ... (status: {clusterStatus})", state);
			return Task.FromResult(false);
		}

		logger.LogInformation("Starting filter of type {filterType}.", distributedFilter.GetType());

		Task.Run(async () =>
		{
			state = WorkState.InProgress;
			logger.LogInformation("Filtering segment #{segmentIndex}.", batch.Index);
			var timestamp = TimeProvider.System.GetTimestamp();
			var output = distributedFilter.Filter(batch);
			var elapsed = TimeProvider.System.GetElapsedTime(timestamp);
			logger.LogInformation("Segment #{segmentIndex} completed in {time}.", batch.Index, elapsed);

			state = WorkState.Completed;
			await workManagerGrain.ReportBatchResultAsync(output, batch.Index, id);
		}).SafeFireAndForget();

		return Task.FromResult(true);
	}
}
