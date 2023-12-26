using DistributedFiltering.Abstractions.Contracts;

namespace DistributedFiltering.Abstractions.Interfaces;

public interface IDistributedFilter : IBaseDistributedFilter
{
	public byte[] Filter(Batch data);
}
