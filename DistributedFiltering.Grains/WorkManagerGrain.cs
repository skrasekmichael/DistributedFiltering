using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Filters.Utils;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Utilities;

namespace DistributedFiltering.Grains;

[Reentrant]
public sealed class WorkManagerGrain(ILogger<WorkManagerGrain> logger) : Grain, IWorkManagerGrain
{
	private WorkState state = WorkState.NotStarted;
	private IResultCollector? resultCollector;

	//private readonly List<IWorker> workers = [];
	private readonly ObserverManager<IWorker> workers = new(TimeSpan.FromMinutes(1), logger);

	public async Task DistributeWorkAsync<TFilter, TFilterParameters>(ImageData image, TFilterParameters parameters)
		where TFilter : IDistributedFilter<TFilterParameters>, new()
		where TFilterParameters : IFilterParameters
	{
		if (state is not WorkState.Completed and not WorkState.Canceled and not WorkState.NotStarted)
		{
			logger.LogError("Cluster is already working ... (status: {clusterStatus})", state);
			return;
		}

		if (resultCollector is null)
		{
			logger.LogError("Result collector has not been registered.");
			return;
		}

		state = WorkState.Preparing;
		var workerCount = this.workers.Count;
		if (workerCount < 1)
		{
			logger.LogError("Insufficient number of workers: {workerCount} (required 1+).", workerCount);
			state = WorkState.Canceled;
			return;
		}

		var workers = this.workers.ToList();

		logger.LogInformation("Preparing work for {workerCount} workers.", workerCount);
		var batches = await Task.Run(() => ImageDataUtils.SplitWork(ref image, workerCount, parameters.GetOverlap()));

		await resultCollector.InitAsync(workerCount, new(image.Width, image.Height));

		logger.LogInformation("Distributing work to {workerCount} workers.", workerCount);
		var tasks = new Task[workerCount];
		for (int i = 0; i < workerCount; i++)
		{
			logger.LogInformation("Starting worker {workerId}.", workers[i].GetPrimaryKey());
			tasks[i] = workers[i].StartProcessingAsync<TFilter, TFilterParameters>(batches[i], parameters, i, resultCollector);
		}

		logger.LogInformation("All workers started.");
		state = WorkState.InProgress;
	}

	public async ValueTask<FilteringStatus> GetStatusAsync()
	{
		return new FilteringStatus
		{
			SegmentStatuses = await Task.WhenAll(workers.Select(worker => worker.GetStatusAsync().AsTask())),
			State = state
		};
	}

	public async ValueTask StopProcessingAsync()
	{
		state = WorkState.Canceled;
		await Task.WhenAll(workers.Select(worker => worker.CancelAsync().AsTask()));
	}

	public Task RegisterWorkerAsync(IWorker worker)
	{
		logger.LogInformation("Registered worker of type {workerType} ({workerId}).", worker.GetType(), worker.GetPrimaryKey());
		workers.Subscribe(worker, worker);
		return Task.CompletedTask;
	}

	public Task UnregisterWorkerAsync(IWorker worker)
	{
		logger.LogInformation("Unregistered worker of type {workerType} ({workerId}).", worker.GetType(), worker.GetPrimaryKey());
		workers.Unsubscribe(worker);
		return Task.CompletedTask;
	}

	public Task RegisterCollectorAsync(IResultCollector collector)
	{
		logger.LogInformation("Registered collector.");
		resultCollector = collector;
		return Task.CompletedTask;
	}

	public Task UnregisterCollectorAsync(IResultCollector worker)
	{
		logger.LogInformation("Unregistered collector.");
		resultCollector = null;
		return Task.CompletedTask;
	}

	public Task FinalizeAsync()
	{
		logger.LogInformation("Work completed.");
		state = WorkState.Completed;
		return Task.CompletedTask;
	}
}
