using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Server;
using DistributedFiltering.Server.Requests;
using DistributedFiltering.Server.Services;
using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans((context, siloBuilder) =>
{
	siloBuilder.UseLocalhostClustering();
	//siloBuilder.AddMemoryGrainStorage("distributed-filtering-storage");
	siloBuilder.Configure<ClusterOptions>(options =>
	{
		options.ClusterId = "dev";
		options.ServiceId = "distributed-filtering";
	});
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<OnStartupService>();
builder.Services.AddSingleton(serviceProvider =>
{
	var collectorLogger = serviceProvider.GetRequiredService<ILogger<ResultCollector>>();
	var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();

	var collector = new ResultCollector(collectorLogger, environment.WebRootPath);
	return collector;
});

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

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
