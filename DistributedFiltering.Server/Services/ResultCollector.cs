using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DistributedFiltering.Server.Services;

public sealed class ResultCollector(ILogger<ResultCollector> logger, string destination) : IResultCollector
{
	public async Task SaveAsync(ImageData imageData, string name)
	{
		var path = Path.Combine(destination, $"{name}.png");
		logger.LogInformation("Storing image {path}.", path);

		using var image = Image.LoadPixelData<Rgba32>(imageData.Data, imageData.Width, imageData.Height);
		await image.SaveAsPngAsync(path);
	}
}
