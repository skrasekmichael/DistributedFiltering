using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Filters.Filters;
using DistributedFiltering.Grains;
using DistributedFiltering.Server;
using DistributedFiltering.Server.Requests;
using DistributedFiltering.Server.Services;
using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans((context, siloBuilder) =>
{
	siloBuilder.UseLocalhostClustering();
	siloBuilder.AddMemoryGrainStorage("distributed-filtering-storage");
	siloBuilder.Configure<ClusterOptions>(options =>
	{
		options.ClusterId = "dev";
		options.ServiceId = "distributed-filtering";
	});
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<OnStartupService>();

builder.Services.AddSingleton(serviceProvider =>
{
	var grainFactory = serviceProvider.GetRequiredService<IGrainFactory>();
	var workManagerGrain = grainFactory.GetGrain<IWorkManagerGrain>(0);

	var collectorLogger = serviceProvider.GetRequiredService<ILogger<ResultCollector>>();
	var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();

	var collector = new ResultCollector(collectorLogger, workManagerGrain, environment.WebRootPath);
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

app.AddFilter<BilateralFilter, BilateralFilterParams, CreateBilateralJobRequest>("/bilateral-filter", (parameters => new BilateralFilterParams
{
	RangeSigma = parameters.RangeSigma,
	SpatialSigma = parameters.SpatialSigma,
}));

app.AddFilter<AddGaussianNoiseFilter, GaussianNoiseParams, CreateAddNoiseJobRequest>("/add-gaussian-noise-filter", (parameters => new GaussianNoiseParams
{
	Sigma = parameters.Sigma
}));

app.MapGet("/status", async (IGrainFactory grainFactory) =>
{
	var fitlerGrain = grainFactory.GetGrain<IWorkManagerGrain>(0);
	return await fitlerGrain.GetStatusAsync();
}).WithOpenApi();

app.MapDelete("/cancel", async (IGrainFactory grainFactory) =>
{
	var fitlerGrain = grainFactory.GetGrain<IWorkManagerGrain>(0);
	await fitlerGrain.StopProcessingAsync();
}).WithOpenApi();

app.Run();
