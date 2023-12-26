using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;

namespace DistributedFiltering.Filters.Utils;

public static class ImageDataUtils
{
	public static unsafe ImageData BuildImage(byte[][] data, Size bounds)
	{
		var output = new ImageData()
		{
			Width = bounds.Width,
			Height = bounds.Height,
			Data = new byte[bounds.Width * bounds.Height * 4]
		};

		int offset = 0;
		for (int i = 0; i < data.Length; i++)
		{
			Buffer.BlockCopy(data[i], 0, output.Data, offset, data[i].Length);
			offset += data[i].Length;
		}

		return output;
	}

	public unsafe static Batch[] SplitWork<TFilterParameters>(
		ref ImageData image,
		int segmentMaxSize,
		TFilterParameters parameters)
		where TFilterParameters : IFilterParameters
	{
		var overlap = Math.Clamp(parameters.GetOverlap(), 0, image.Height);

		var windowHeight = Math.Max((int)Math.Floor((double)segmentMaxSize / image.Width), 1);
		var count = (int)Math.Ceiling((double)image.Height / windowHeight);

		var batches = new Batch[count];

		for (int y = 0; y < count; y++)
		{
			var startY = Math.Max(y * windowHeight - overlap, 0);
			var endY = Math.Min((y + 1) * windowHeight + overlap, image.Height - 1);

			batches[y] = new()
			{
				Input = GetImageData(image.Data, image.Width, startY, endY),
				FilteringWindow = new(
					x: 0,
					y: y * windowHeight - startY,
					w: image.Width,
					h: Math.Min((y + 1) * windowHeight, image.Height) - y * windowHeight
				),
				Size = new(image.Width, endY - startY + 1),
				Index = y,
				Parameters = parameters
			};
		}

		return batches;
	}

	private unsafe static byte[] GetImageData(byte[] input, int width, int startY, int endY)
	{
		byte[] imageData = new byte[width * (endY - startY + 1) * 4];
		Buffer.BlockCopy(input, startY * width * 4, imageData, 0, imageData.Length);
		return imageData;
	}
}
