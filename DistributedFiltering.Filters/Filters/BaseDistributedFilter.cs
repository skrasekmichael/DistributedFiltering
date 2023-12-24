using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;
using OpenTK.Mathematics;

namespace DistributedFiltering.Filters.Filters;

public abstract class BaseDistributedFilter<TFilterParameters> : IDistributedFilter<TFilterParameters>
	where TFilterParameters : IFilterParameters
{
	protected int doneCount = 0;
	protected double sizeCoeff = 0;

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

	public byte[] Filter(Batch data, TFilterParameters parameters)
	{
		doneCount = 0;
		sizeCoeff = 100.0 / (data.ImageSize.Width * data.ImageSize.Height);
		return FilterBatch(data, parameters);
	}

	protected abstract byte[] FilterBatch(Batch data, TFilterParameters parameters);

	public void Cancel()
	{
		IsCanceled = true;
	}

	public double GetProgress() => Progress;
}
