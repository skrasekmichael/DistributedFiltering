using DistributedFiltering.Abstractions.Interfaces;

namespace DistributedFiltering.Server.Services;

public sealed class OnStartupService(IGrainFactory grainFactory, ResultCollector collector) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var cluster = grainFactory.GetGrain<IClusterGrain>(0);

		var collectorRef = grainFactory.CreateObjectReference<IResultCollector>(collector);
		await cluster.RegisterCollectorAsync(collectorRef);
	}
}
