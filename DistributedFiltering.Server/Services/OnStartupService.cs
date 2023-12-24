using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Grains;

namespace DistributedFiltering.Server.Services;

public sealed class OnStartupService(ResultCollector collector, IGrainFactory grainFactory) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken ct)
	{
		var workManagerGrain = grainFactory.GetGrain<IWorkManagerGrain>(0);
		var collectorRef = grainFactory.CreateObjectReference<IResultCollector>(collector);
		await workManagerGrain.RegisterCollectorAsync(collectorRef);
	}
}
