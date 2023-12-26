using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Filters.Extensions;
using DistributedFiltering.Server.Requests;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics.CodeAnalysis;

namespace DistributedFiltering.Server;

public static class WebApplicationExtensions
{
	public static void AddFilter<TFilterParameters, TCreateJobRequest>(
		this WebApplication app,
		[StringSyntax("Route")] string route,
		Func<TCreateJobRequest, TFilterParameters> map
	)
		where TFilterParameters : IFilterParameters
		where TCreateJobRequest : ICreateJobRequest
	{
		app.MapPost(route, async (
			[FromRoute] string targetFileName,
			[FromBody] TCreateJobRequest request,
			[FromServices] IGrainFactory grainFactory,
			[FromServices] IWebHostEnvironment environment,
			[FromServices] ILogger<Program> logger) =>
		{
			var targetFile = Path.Combine(environment.WebRootPath, $"{targetFileName}.png");
			if (!File.Exists(targetFile))
			{
				logger.LogError("Target file [{path}] doesn't exist.", targetFile);
				return Results.BadRequest("Target file doesn't exist.");
			}

			var parameters = map(request);

			using var image = await Image.LoadAsync<Rgba32>(targetFile);
			var data = image.ToImageData();
			image.Dispose();

			var cluster = grainFactory.GetGrain<IClusterGrain>(0);
			if (await cluster.DistributeWorkAsync(data, parameters, request.BatchSize.GetSize(), request.ResultFileName))
			{
				return Results.Created("/api/status", "Job created.");
			}
			return Results.BadRequest();
		}).WithOpenApi();
	}
}
