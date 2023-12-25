using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Filters.Extensions;
using DistributedFiltering.Filters.Utils;
using OpenTK.Mathematics;

namespace DistributedFiltering.Filters.Filters;

public sealed class BilateralFilter : BaseDistributedFilter<BilateralFilterParams>
{
	protected override unsafe byte[] FilterBatch(Batch batch, BilateralFilterParams parameters)
	{
		var radius = parameters.GetRadius();
		var radius2 = radius * radius;

		var spaceGauss = new GaussianFunction(parameters.SpatialSigma);
		var rangeGauss = new GaussianFunction(parameters.RangeSigma);

		var window = batch.FilteringWindow;
		var output = new byte[window.Width * window.Height * 4];

		byte* Coords2SrcPtr(byte* ptr, int x, int y) => ptr + 4 * (y * batch.Size.Width + x);
		byte* Coords2DstPtr(byte* ptr, int x, int y) => ptr + 4 * ((y - window.Y) * window.Width + x - window.X);

		fixed (byte* inPtr = batch.Input)
		fixed (byte* outPtr = output)
		{
			for (int cy = window.Y; cy < window.Y + window.Height; cy++)
			{
				int starty = Math.Max(cy - radius, 0);
				int endy = Math.Min(cy + radius, batch.Size.Height - 1);

				for (int cx = window.X; cx < window.X + window.Width; cx++)
				{
					int startx = Math.Max(cx - radius, 0);
					int endx = Math.Min(cx + radius, batch.Size.Width - 1);

					Vector4i centerColor = GetColor(Coords2SrcPtr(inPtr, cx, cy));

					Vector4d weightedSum = Vector4d.Zero, normalizationFactor = Vector4d.Zero;
					for (int y = starty; y <= endy; y++)
					{
						int dy = y - cy;
						int dy2 = dy * dy;

						for (int x = startx; x <= endx; x++)
						{
							int dx = x - cx;
							int dz2 = dx * dx + dy2;

							if (dz2 < radius2)
							{
								Vector4i color = GetColor(Coords2SrcPtr(inPtr, x, y));
								double gs = spaceGauss.Gauss2(dz2);
								Vector4d fr = rangeGauss.Gauss((color - centerColor).Abs());

								Vector4d weight = gs * fr;
								weightedSum += weight * color;
								normalizationFactor += weight;
							}
						}
					}

					Vector4i newColor = (Vector4i)weightedSum.Div(normalizationFactor);

					SetColor(Coords2DstPtr(outPtr, cx, cy), newColor);
					doneCount++;
				}

				if (IsCanceled) return output;
				UpdateProgress();
			}
		}

		return output;
	}
}
