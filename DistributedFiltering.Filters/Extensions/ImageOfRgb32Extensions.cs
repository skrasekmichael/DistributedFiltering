using DistributedFiltering.Abstractions.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DistributedFiltering.Filters.Extensions;

public static class ImageOfRgb32Extensions
{
	public unsafe static ImageData ToImageData(this Image<Rgba32> img)
	{
		var imageData = new ImageData
		{
			Width = img.Width,
			Height = img.Height,
			Data = new byte[img.Width * img.Height * 4]
		};

		img.CopyPixelDataTo(imageData.Data);

		return imageData;
	}
}
