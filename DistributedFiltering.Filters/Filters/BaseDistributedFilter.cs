﻿using DistributedFiltering.Abstractions.Contracts;
using OpenTK.Mathematics;

namespace DistributedFiltering.Filters.Filters;

public abstract class BaseDistributedFilter(Size bounds)
{
	protected int doneCount = 0;
	protected readonly double sizeCoeff = 100.0 / (bounds.Width * bounds.Height);

	public bool IsCanceled { get; private set; } = false;

	public double Progress { get; private set; }

	protected unsafe Vector4i GetColor(byte* ptr)
	{
		return new(
			*ptr,
			*(ptr + 1),
			*(ptr + 2),
			*(ptr + 3)
		);
	}

	protected unsafe void SetColor(byte* ptr, Vector4i color) => SetColor(ptr, (byte)color.X, (byte)color.Y, (byte)color.Z, (byte)color.W);

	protected unsafe void SetColor(byte* ptr, byte R, byte G, byte B, byte A)
	{
		*ptr = R;
		*(ptr + 1) = G;
		*(ptr + 2) = B;
		*(ptr + 3) = A;
	}

	protected virtual void UpdateProgress()
	{
		Progress = doneCount * sizeCoeff;
	}

	public void Cancel()
	{
		IsCanceled = true;
	}

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

	public unsafe static Batch[] SplitWork(ref ImageData image, int count, int overlap = 0)
	{
		if (count <= 1)
		{
			return
			[
				new()
				{
					Input = image.Data,
					FilteringWindow = new(0, 0, image.Width, image.Height),
					Size = new(image.Width, image.Height),
					ImageSize = new(image.Width, image.Height)
				}
			];
		}

		overlap = Math.Max(overlap, 0);
		int windowHeight = (int)Math.Round((double)image.Height / count);

		var batches = new Batch[count];

		for (int i = 0; i < count; i++)
		{
			var startY = Math.Max(i * windowHeight - overlap, 0);
			var endY = Math.Min((i + 1) * windowHeight + overlap, image.Height - 1);

			batches[i] = new()
			{
				Input = GetImageData(image.Data, image.Width, startY, endY),
				FilteringWindow = new(
					x: 0,
					y: i * windowHeight - startY,
					w: image.Width,
					h: Math.Min((i + 1) * windowHeight, image.Height) - i * windowHeight
				),
				Size = new(image.Width, endY - startY + 1),
				ImageSize = new(image.Width, image.Height)
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
