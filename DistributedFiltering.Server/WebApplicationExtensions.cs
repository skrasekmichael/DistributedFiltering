using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Filters.Extensions;
using DistributedFiltering.Server.Requests;
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
		app.MapPost(route, async (TCreateJobRequest request, IGrainFactory grainFactory, IWebHostEnvironment environment) =>
		{
			var parameters = map(request);

			var img = await Image.LoadAsync<Rgba32>($"{environment.WebRootPath}/street.png");
			var data = img.ToImageData();
			img.Dispose();

			var cluster = grainFactory.GetGrain<IClusterGrain>(0);
			if (await cluster.DistributeWorkAsync(data, parameters))
			{
				return Results.Created("/status", "Job created.");
			}
			return Results.BadRequest();
		}).WithOpenApi();
	}
}
