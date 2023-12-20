using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Grains;
using DistributedFiltering.Filters.Filters;
using Orleans.Concurrency;
using SixLabors.ImageSharp;

namespace DistributedFiltering.Grains;

[Reentrant]
public sealed class FilterGrain<TFilterSegmentGrain, TFilterParameters> : Grain, IFilterGrain<TFilterSegmentGrain, TFilterParameters>
	where TFilterSegmentGrain : IFilterSegmentGrain<TFilterParameters>
	where TFilterParameters : IFilterParameters
{
	private Batch[] batches = [];
	private TFilterSegmentGrain[] segments = [];
	private FilteringState state = FilteringState.NotStarted;

	[ReadOnly]
	public async Task<FilteringStatus> GetStatusAsync()
	{
		return state switch
		{
			FilteringState.InProgress or
			FilteringState.Completed => new FilteringStatus
			{
				State = state,
				SegmentStatuses = await Task.WhenAll(segments.Select(segment => segment.GetStatusAsync()))
			},
			_ => new FilteringStatus()
			{
				State = state,
				SegmentStatuses = []
			}
		};
	}

	public async Task<ImageData> FilterAsync(ImageData image, TFilterParameters parameters)
	{
		state = FilteringState.Preparing;

		batches = await Task.Run(() => BaseDistributedFilter.SplitWork(ref image, parameters.UnitCount, parameters.GetOverlap()));
		segments = new TFilterSegmentGrain[parameters.UnitCount];

		var tasks = new Task<byte[]>[parameters.UnitCount];
		for (int i = 0; i < parameters.UnitCount; i++)
		{
			segments[i] = GrainFactory.GetGrain<TFilterSegmentGrain>(i);
			tasks[i] = segments[i].ApplyFilterAsync(batches[i], parameters);
		}

		state = FilteringState.InProgress;
		var output = await Task.WhenAll(tasks);

		var newImage = BaseDistributedFilter.BuildImage(output, new(image.Width, image.Height));
		state = FilteringState.Completed;
		return newImage;
	}

	public Task StopFilteringAsync()
	{
		state = FilteringState.Canceled;
		return Task.WhenAll(segments.Select(s => s.CancelAsync()));
	}
}
