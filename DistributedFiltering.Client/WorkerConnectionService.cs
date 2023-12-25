using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Grains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DistributedFiltering.Client;

public sealed class WorkerConnectionService(
	IClusterClient client,
	IServiceProvider serviceProvider,
	IHostApplicationLifetime applicationLifetime) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken ct)
	{
		var workManager = client.GetGrain<IWorkManagerGrain>(0);

		var worker = new Worker(serviceProvider.GetRequiredService<ILogger<Worker>>());
		var wokrerRef = client.CreateObjectReference<IWorker>(worker);
		await workManager.RegisterWorkerAsync(wokrerRef);

		while (!ct.IsCancellationRequested)
		{
			await Task.Delay(2000, ct);
		}
	}
}
