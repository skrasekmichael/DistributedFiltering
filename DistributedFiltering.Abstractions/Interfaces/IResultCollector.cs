using DistributedFiltering.Abstractions.Contracts;

namespace DistributedFiltering.Abstractions.Interfaces;

public interface IResultCollector : IGrainObserver
{
	public Task SaveAsync(ImageData imageData, string name);
}
