using DistributedFiltering.Abstractions.Contracts;
using OpenTK.Mathematics;

namespace DistributedFiltering.Filters.Filters;

public sealed class AddGaussianNoiseFilter(Size size, GaussianNoiseParams parameters, int seed) : BaseDistributedFilter(size)
{
	private readonly Random generator = new(seed);
	private readonly double sigma = parameters.Sigma;

	private double GetNext()
	{
		//https://stackoverflow.com/questions/218060/random-gaussian-variables
		double u1 = 1.0 - generator.NextDouble();
		double u2 = 1.0 - generator.NextDouble();
		return sigma * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
	}

	private byte NextByte(byte src)
	{
		var val = GetNext();
		return (byte)Math.Clamp(src + (int)val, 0, 255);
	}

	public unsafe byte[] FilterBatch(Batch batch)
	{
		doneCount = 0;
		var window = batch.FilteringWindow;
		var output = new byte[window.Width * window.Height * 4];

		byte* Coords2SrcPtr(byte* ptr, int x, int y) => ptr + 4 * (y * batch.Size.Width + x);
		byte* Coords2DstPtr(byte* ptr, int x, int y) => ptr + 4 * ((y - window.Y) * window.Width + x - window.X);

		fixed (byte* inPtr = batch.Input)
		fixed (byte* outPtr = output)
		{
			for (int y = window.Y; y < window.Y + window.Height; y++)
			{
				for (int x = window.X; x < window.X + window.Width; x++)
				{
					Vector4i color = GetColor(Coords2SrcPtr(inPtr, x, y));

					var dstPtr = Coords2DstPtr(outPtr, x, y);

					*dstPtr++ = NextByte((byte)color.X);
					*dstPtr++ = NextByte((byte)color.Y);
					*dstPtr++ = NextByte((byte)color.Z);
					*dstPtr = 255;

					doneCount++;
				}

				if (IsCanceled) return output;
				UpdateProgress();
			}
		}

		return output;
	}
}
