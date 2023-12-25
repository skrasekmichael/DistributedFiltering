using DistributedFiltering.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;

await new HostBuilder()
	.UseOrleansClient(orleans =>
	{
		orleans.UseLocalhostClustering();
		orleans.Configure<ClusterOptions>(options =>
		{
			options.ClusterId = "dev";
			options.ServiceId = "distributed-filtering";
		});
	})
	.ConfigureServices(services =>
	{
		services.AddHostedService<WorkerConnectionService>();
	})
	.ConfigureLogging(logging =>
	{
		logging.AddConsole();
	}).RunConsoleAsync();
