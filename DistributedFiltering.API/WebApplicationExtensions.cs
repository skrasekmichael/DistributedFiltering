using AsyncAwaitBestPractices;
using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Grains;
using DistributedFiltering.API.Requests;
using DistributedFiltering.Filters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics.CodeAnalysis;

namespace DistributedFiltering.API;

public static class WebApplicationExtensions
{
	public static void AddFilter<TGrainFilter, TFilterParameters, TCreateJobRequest>(
		this WebApplication app, 
		[StringSyntax("Route")] string route,
		Func<TCreateJobRequest, TFilterParameters> map
	)
		where TGrainFilter : IFilterGrain<TFilterParameters>
		where TFilterParameters : IFilterParameters
		where TCreateJobRequest : ICreateJobRequest
	{
		app.MapPost(route, async (TCreateJobRequest request, IGrainFactory grainFactory, IWebHostEnvironment environment) =>
		{
			var id = Guid.NewGuid();

			Task.Run(async () =>
			{
				var parameters = map(request);

				var img = await Image.LoadAsync<Rgba32>($"{environment.WebRootPath}/street2.png");
				var data = img.ToImageData();
				img.Dispose();

				var filterGrain = grainFactory.GetGrain<TGrainFilter>(id);
				var output = await filterGrain.FilterAsync(data, parameters);
				var outImg = Image.LoadPixelData<Rgba32>(output.Data, output.Width, output.Height);

				await outImg.SaveAsPngAsync($"{environment.WebRootPath}/result.png");
			}).SafeFireAndForget();

			return id;
		}).WithOpenApi();

		app.MapGet(route + "/{id:guid}", async (Guid id, IGrainFactory grainFactory) =>
		{
			var fitlerGrain = grainFactory.GetGrain<TGrainFilter>(id);
			return await fitlerGrain.GetStatusAsync();
		}).WithOpenApi();

		app.MapDelete(route + "/{id:guid}", async (Guid id, IGrainFactory grainFactory) =>
		{
			var fitlerGrain = grainFactory.GetGrain<TGrainFilter>(id);
			await fitlerGrain.StopFilteringAsync();
		}).WithOpenApi();
	}
}
