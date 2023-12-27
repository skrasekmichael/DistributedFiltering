using DistributedFiltering.Abstractions.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DistributedFiltering.Client;

public sealed class ConnectWorkerOnStartup(IClusterClient client, ILogger<Worker> workerLogger) : BackgroundService
{
	private static Worker? worker; //needs to be stored to avoid garbage collection

	protected override async Task ExecuteAsync(CancellationToken ct)
	{
		var cluster = client.GetGrain<IClusterGrain>(0);
		worker = new Worker(workerLogger, cluster);

		var wokrerRef = client.CreateObjectReference<IWorker>(worker);
		await cluster.RegisterWorkerAsync(wokrerRef);
	}
}
