using DistributedFiltering.Abstractions.Contracts;

namespace DistributedFiltering.Abstractions.Interfaces;

public interface IResultCollector : IGrainObserver
{
	public Task InitAsync(int count, Size imageSize);

	public Task ReportResultAsync(byte[] data, int index);
}
