using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Filters.Utils;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DistributedFiltering.Grains;

public sealed class ResultCollector(ILogger<ResultCollector> logger, IWorkManagerGrain workManagerGrain, string webRoot) : IResultCollector
{
	private readonly object mutex = new();

	private byte[][]? batches;
	private int done = 0;
	private Abstractions.Contracts.Size imageSize;

	public Task InitAsync(int count, Abstractions.Contracts.Size imageSize)
	{
		done = 0;
		batches = new byte[count][];
		this.imageSize = imageSize;
		return Task.CompletedTask;
	}

	public Task ReportResultAsync(byte[] data, int index)
	{
		logger.LogInformation("Received result (len: {dataLength}) for segment {segmentIndex}", data.Length, index);

		if (batches is null)
		{
			logger.LogError("Result collector has not been initialized.");
			return Task.CompletedTask;
		}

		batches[index] = data;

		lock (mutex)
		{
			done++;
		}

		logger.LogInformation("Received {receivedCount} out of {resultCount}.", done, batches.Length);

		if (done == batches.Length)
		{
			logger.LogInformation("All {resultCount} received.", batches.Length);

			Task.Run(async () =>
			{
				logger.LogInformation("Building image.");

				var output = ImageDataUtils.BuildImage(batches, imageSize);

				if (!output.IsEmpty())
				{
					logger.LogInformation("Storing image.");
					var outImg = Image.LoadPixelData<Rgba32>(output.Data, output.Width, output.Height);
					await outImg.SaveAsPngAsync($"{webRoot}/result.png");
					logger.LogInformation("Image stored.");
				}

				await workManagerGrain.FinalizeAsync();
			});
		}

		return Task.CompletedTask;
	}
}
