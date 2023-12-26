using DistributedFiltering.Abstractions.Contracts;
using Orleans.Concurrency;

namespace DistributedFiltering.Abstractions.Interfaces;

public interface IWorker : IGrainObserver, IAsyncCancelable
{
	public Task<bool> StartProcessingAsync(Batch batch, Guid id);

	[ReadOnly]
	public ValueTask<SegmentFilteringStatus> GetStatusAsync();

	[ReadOnly]
	[ResponseTimeout("00:00:02")]
	public Task PingAsync();
}
