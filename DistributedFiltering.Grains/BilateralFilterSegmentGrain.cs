using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Grains;
using DistributedFiltering.Filters.Filters;
using Orleans.Concurrency;

namespace DistributedFiltering.Grains;

[Reentrant]
public sealed class BilateralFilterSegmentGrain : Grain, IBilateralFilterSegmentGrain
{
	private BilateralFilter? bilateralFilter;
	private SegmentFilteringState state = SegmentFilteringState.NotStarted;

	public async Task<byte[]> ApplyFilterAsync(Batch batch, BilateralFilterParams parameters)
	{
		bilateralFilter = new(new(batch.FilteringWindow.Width, batch.FilteringWindow.Height), parameters);
		state = SegmentFilteringState.InProgress;
		var data = await Task.Run(() => bilateralFilter.FilterBatch(batch));
		state = SegmentFilteringState.Completed;
		return data;
	}

	public Task CancelAsync()
	{
		bilateralFilter?.Cancel();
		state = SegmentFilteringState.Canceled;
		return Task.CompletedTask;
	}

	[ReadOnly]
	public Task<SegmentFilteringStatus> GetStatusAsync()
	{
		return Task.FromResult(new SegmentFilteringStatus()
		{
			State = state,
			Progress = bilateralFilter?.Progress ?? 0
		});
	}
}
