using DistributedFiltering.Abstractions.Interfaces;
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
		var cluster = client.GetGrain<IClusterGrain>(0);

		var worker = new Worker(serviceProvider.GetRequiredService<ILogger<Worker>>(), cluster);
		var wokrerRef = client.CreateObjectReference<IWorker>(worker);
		await cluster.RegisterWorkerAsync(wokrerRef);

		while (!ct.IsCancellationRequested)
		{
			await Task.Delay(2000, ct);
		}
	}
}
