using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Filters.Extensions;
using DistributedFiltering.Server.Requests;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics.CodeAnalysis;

namespace DistributedFiltering.Server;

public static class WebApplicationExtensions
{
	public static void AddFilter<TFilter, TFilterParameters, TCreateJobRequest>(
		this WebApplication app,
		[StringSyntax("Route")] string route,
		Func<TCreateJobRequest, TFilterParameters> map
	)
		where TFilter : IDistributedFilter<TFilterParameters>, new()
		where TFilterParameters : IFilterParameters
		where TCreateJobRequest : ICreateJobRequest
	{
		app.MapPost(route, async (TCreateJobRequest request, IGrainFactory grainFactory, IWebHostEnvironment environment) =>
		{
			var parameters = map(request);

			var img = await Image.LoadAsync<Rgba32>($"{environment.WebRootPath}/street2.png");
			var data = img.ToImageData();
			img.Dispose();

			var filterGrain = grainFactory.GetGrain<IWorkManagerGrain>(0);
			await filterGrain.DistributeWorkAsync<TFilter, TFilterParameters>(data, parameters);
		}).WithOpenApi();
	}
}
