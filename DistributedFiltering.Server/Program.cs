using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Server;
using DistributedFiltering.Server.Requests;
using DistributedFiltering.Server.Services;
using Orleans.Configuration;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans((ISiloBuilder orleans) =>
{
	var clusterConfiguration = builder.Configuration.GetSection("Cluster");
	if (!IPAddress.TryParse(clusterConfiguration["Address"], out var address))
	{
		address = IPAddress.Loopback;
	}

	orleans.UseDevelopmentClustering(new IPEndPoint(address, 11_111));

	orleans.Configure<ClusterOptions>(options =>
	{
		options.ClusterId = "dev";
		options.ServiceId = "distributed-filtering";
	});

	orleans.Configure<EndpointOptions>(options =>
	{
		options.AdvertisedIPAddress = address;
	});
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<OnStartupService>();
builder.Services.AddSingleton(serviceProvider =>
{
	var collectorLogger = serviceProvider.GetRequiredService<ILogger<ResultCollector>>();
	var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();

	return new ResultCollector(collectorLogger, environment.WebRootPath);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();

app.AddFilter<BilateralFilterParams, CreateBilateralJobRequest>("/api/{targetFileName}/apply-bilateral-filter", (parameters => new BilateralFilterParams
{
	RangeSigma = parameters.RangeSigma,
	SpatialSigma = parameters.SpatialSigma,
}));

app.AddFilter<GaussianNoiseParams, CreateAddNoiseJobRequest>("/api/{targetFileName}/add-gaussian-noise", (parameters => new GaussianNoiseParams
{
	Sigma = parameters.Sigma
}));

app.MapGet("/api/status", async (IGrainFactory grainFactory) =>
{
	var fitlerGrain = grainFactory.GetGrain<IClusterGrain>(0);
	return await fitlerGrain.GetStatusAsync();
}).WithOpenApi();

app.MapDelete("/api/cancel", async (IGrainFactory grainFactory) =>
{
	var fitlerGrain = grainFactory.GetGrain<IClusterGrain>(0);
	await fitlerGrain.StopProcessingAsync();
}).WithOpenApi();

app.Run();
