using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Grains;
using DistributedFiltering.Filters.Filters;
using Orleans.Concurrency;

namespace DistributedFiltering.Grains;

[Reentrant]
public sealed class AddGaussianNoiseSegmentGrain : Grain, IAddGaussianNoiseSegmentGrain
{
	private AddGaussianNoiseFilter? addGauissianNoiseFitler;
	private SegmentFilteringState state = SegmentFilteringState.NotStarted;

	public async Task<byte[]> ApplyFilterAsync(Batch batch, GaussianNoiseParams parameters)
	{
		addGauissianNoiseFitler = new(new(batch.FilteringWindow.Width, batch.FilteringWindow.Height), parameters, DateTime.Now.Millisecond);
		state = SegmentFilteringState.InProgress;
		var data = await Task.Run(() => addGauissianNoiseFitler.FilterBatch(batch));
		state = SegmentFilteringState.Completed;
		return data;
	}

	public Task CancelAsync()
	{
		addGauissianNoiseFitler?.Cancel();
		state = SegmentFilteringState.Canceled;
		return Task.CompletedTask;
	}

	[ReadOnly]
	public Task<SegmentFilteringStatus> GetStatusAsync()
	{
		return Task.FromResult(new SegmentFilteringStatus()
		{
			State = state,
			Progress = addGauissianNoiseFitler?.Progress ?? 0
		});
	}
}
