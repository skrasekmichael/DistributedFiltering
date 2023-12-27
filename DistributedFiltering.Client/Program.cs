using DistributedFiltering.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using System.Net;

var builder = Host.CreateApplicationBuilder(args);

builder.UseOrleansClient(orleans =>
{
	var clusterConfiguration = builder.Configuration.GetSection("Cluster");

	if (!IPAddress.TryParse(clusterConfiguration["Address"], out var address))
	{
		address = IPAddress.Loopback;
	}

	if (!int.TryParse(clusterConfiguration["Port"], out var port))
	{
		port = 30_000;
	}

	orleans.UseStaticClustering(new IPEndPoint(address, port));
	orleans.Configure<ClusterOptions>(options =>
	{
		options.ClusterId = "dev";
		options.ServiceId = "distributed-filtering";
	});
});

builder.Services.AddHostedService<ConnectWorkerOnStartup>();

using var app = builder.Build();
await app.RunAsync();
