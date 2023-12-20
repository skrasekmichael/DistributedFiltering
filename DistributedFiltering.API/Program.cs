using DistributedFiltering.Abstractions.Grains;
using DistributedFiltering.API;
using DistributedFiltering.API.Requests;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans((context, siloBuilder) =>
{
	siloBuilder.UseLocalhostClustering();
	siloBuilder.AddMemoryGrainStorage("distributed-filters");
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.AddFilter<IBilateralFilterGrain, BilateralFilterParams, CreateBilateralJobRequest>("/bilateral-filter", (parameters => new BilateralFilterParams
{
	RangeSigma = parameters.RangeSigma,
	SpatialSigma = parameters.SpatialSigma,
	UnitCount = parameters.UnitCount
}));

app.Run();

