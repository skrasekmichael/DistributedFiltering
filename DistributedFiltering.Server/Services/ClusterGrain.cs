using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Filters.Utils;

namespace DistributedFiltering.Server.Services;

public sealed class ClusterGrain(ILogger<ClusterGrain> logger) : Grain, IClusterGrain
{
	private WorkState state = WorkState.NotStarted;
	private IResultCollector? resultCollector;
	private long processedCount = 0;
	private long imageLength = 1;
	private Size imageSize;
	private string outputFileName = string.Empty;
	private long timestamp;

	private readonly List<IWorker> idleWorkers = [];
	private readonly List<IWorker> activeWorkers = [];

	private readonly Dictionary<Guid, Batch> workDistribution = [];
	private readonly Dictionary<int, byte[]> completedWork = [];
	private readonly Queue<Batch> scheduledWork = [];

	public override Task OnActivateAsync(CancellationToken ct)
	{
		//heartbeat
		RegisterTimer(PingAllAsync, this, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));

		return base.OnActivateAsync(ct);
	}

	private async Task PingAllAsync(object o)
	{
		var pingHadnlers = new List<Task>();
		foreach (var worker in idleWorkers.ToArray())
		{
			var pingTask = worker.PingAsync();
			pingHadnlers.Add(Task.Run(async () =>
			{
				try
				{
					await pingTask;
				}
				catch (TimeoutException)
				{
					logger.LogWarning("Idle worker {workerId} stopped responding.", worker.GetPrimaryKey());
					idleWorkers.Remove(worker);
				}
			}));
		}

		foreach (var worker in activeWorkers.ToArray())
		{
			var pingTask = worker.PingAsync();
			pingHadnlers.Add(Task.Run(async () =>
			{
				try
				{
					await pingTask;
				}
				catch (TimeoutException)
				{
					var workerId = worker.GetPrimaryKey();
					logger.LogWarning("Active worker {workerId} stopped responding.", workerId);

					if (workDistribution.TryGetValue(workerId, out var batch))
					{
						workDistribution.Remove(workerId);

						logger.LogInformation("Redistributing batch {index}.", batch.Index);
						await ScheduleWorkAsync(batch);
					}

					activeWorkers.Remove(worker);
				}
			}));
		}

		await Task.WhenAll(pingHadnlers);
	}

	public async Task<bool> DistributeWorkAsync<TFilterParameters>(ImageData image, TFilterParameters parameters, int maxBatchSize, string output)
		where TFilterParameters : IFilterParameters
	{
		if (state is not WorkState.Completed and not WorkState.Canceled and not WorkState.NotStarted)
		{
			logger.LogError("Cluster is already working (status: {clusterStatus})", state);
			return false;
		}

		if (resultCollector is null)
		{
			logger.LogCritical("Result collector has not been registered.");
			return false;
		}

		state = WorkState.Preparing;
		logger.LogInformation("Preparing work ...");

		processedCount = 0;
		workDistribution.Clear();
		completedWork.Clear();
		imageLength = image.Width * image.Height * 4;
		imageSize = new(image.Width, image.Height);
		outputFileName = output;

		var batches = await Task.Run(() => ImageDataUtils.SplitWork(ref image, maxBatchSize, parameters));
		logger.LogInformation("Work divided into {bacthCount} batches.", batches.Length);

		logger.LogInformation("Distributing work to workers ...");
		timestamp = TimeProvider.System.GetTimestamp();

		int index = 0;
		while (index < batches.Length && idleWorkers.Count > 0)
		{
			var worker = idleWorkers.First();
			idleWorkers.Remove(worker);

			var workerId = worker.GetPrimaryKey();
			logger.LogInformation("Starting worker {workerId} ...", workerId);

			if (await worker.StartProcessingAsync(batches[index], workerId))
			{
				workDistribution.Add(workerId, batches[index]);
				activeWorkers.Add(worker);
				index++;
			}
			else
			{
				logger.LogCritical("Failed to start processing at worker {workerId}", worker.GetPrimaryKey());
			}
		}

		for (int i = index; i < batches.Length; i++)
		{
			scheduledWork.Enqueue(batches[i]);
		}

		logger.LogInformation("{workerCount} batches distributed to workers, {rest} batches waiting for available workers.", index, batches.Length - index);
		state = WorkState.InProgress;
		return true;
	}

	public async ValueTask<FilteringStatus> GetStatusAsync()
	{
		return new FilteringStatus
		{
			SegmentStatuses = await Task.WhenAll(activeWorkers.Select(worker =>
			{
				return Task.Run(async () =>
				{
					try
					{
						return await worker.GetStatusAsync();
					}
					catch (TimeoutException)
					{
						return new SegmentFilteringStatus()
						{
							Progress = 0,
							State = WorkState.Canceled
						};
					}
				});
			})),
			State = state,
			Progress = 100 * processedCount / imageLength
		};
	}

	public async ValueTask StopProcessingAsync()
	{
		await Task.WhenAll(activeWorkers.Select(worker =>
		{
			return Task.Run(async () =>
			{
				try
				{
					await worker.CancelAsync();
				}
				catch (TimeoutException) { }
			});
		}));

		idleWorkers.AddRange(activeWorkers);
		activeWorkers.Clear();
		workDistribution.Clear();
		completedWork.Clear();
		scheduledWork.Clear();
		state = WorkState.Canceled;
	}

	public async Task RegisterWorkerAsync(IWorker worker)
	{
		logger.LogInformation("Registered worker of type {workerType} ({workerId}).", worker.GetType(), worker.GetPrimaryKey());
		await AddIdleWorkerAsync(worker);
	}

	public Task UnregisterWorkerAsync(IWorker worker)
	{
		logger.LogInformation("Unregistered worker of type {workerType} ({workerId}).", worker.GetType(), worker.GetPrimaryKey());
		activeWorkers.Remove(worker);
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

	public async Task ReportBatchResultAsync(byte[] data, int orderingIndex, Guid workerId)
	{
		workDistribution.Remove(workerId);
		completedWork.Add(orderingIndex, data);
		processedCount += data.Length;

		logger.LogInformation("Data segment #{index} reported.", orderingIndex);

		var worker = activeWorkers.Find(worker => worker.GetPrimaryKey() == workerId)!;
		activeWorkers.Remove(worker);
		await AddIdleWorkerAsync(worker);

		if (processedCount == imageLength)
		{
			var elapesd = TimeProvider.System.GetElapsedTime(timestamp);
			logger.LogInformation("Data processed in {time}.", elapesd);

			logger.LogInformation("Building image ...");

			var image = ImageDataUtils.BuildImage(
				data: completedWork
					.OrderBy(x => x.Key)
					.Select(x => x.Value)
					.ToArray(),
				bounds: imageSize
			);

			if (resultCollector is null)
			{
				logger.LogCritical("Result collector is null, can't save result.");
			}
			else
			{
				await resultCollector.SaveAsync(image, outputFileName);
			}

			state = WorkState.Completed;
		}
	}

	private async Task AddIdleWorkerAsync(IWorker worker)
	{
		var workerId = worker.GetPrimaryKey();
		if (scheduledWork.Count == 0)
		{
			idleWorkers.Add(worker);
			logger.LogInformation("Worker {workerId} added to the idle queue, no work to be done.", workerId);
			return;
		}

		var batch = scheduledWork.Dequeue();
		if (await worker.StartProcessingAsync(batch, workerId))
		{
			workDistribution.Add(workerId, batch);
			activeWorkers.Add(worker);
			logger.LogInformation("Batch #{index} distributed to worker {workerId}", batch.Index, workerId);
		}
		else
		{
			logger.LogCritical("Failed to start processing at worker {workerId}", workerId);
		}
	}

	private async Task ScheduleWorkAsync(Batch batch)
	{
		if (idleWorkers.Count == 0)
		{
			scheduledWork.Enqueue(batch);
			logger.LogInformation("No workers available, batch {index} scheduled for later processing.", batch.Index);
			return;
		}

		var worker = idleWorkers.First();
		idleWorkers.Remove(worker);

		var workerId = worker.GetPrimaryKey();
		if (await worker.StartProcessingAsync(batch, workerId))
		{
			workDistribution.Add(workerId, batch);
			activeWorkers.Add(worker);
			logger.LogInformation("Batch #{index} distributed to worker {workerId}", batch.Index, workerId);
		}
		else
		{
			logger.LogCritical("Failed to start processing at worker {workerId}", workerId);
		}
	}
}
