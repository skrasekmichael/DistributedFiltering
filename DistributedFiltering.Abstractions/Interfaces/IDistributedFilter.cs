using DistributedFiltering.Abstractions.Contracts;

namespace DistributedFiltering.Abstractions.Interfaces;

public interface IDistributedFilter<TFilterParameters> : IBaseDistributedFilter
	where TFilterParameters : IFilterParameters
{
	public byte[] Filter(Batch data, TFilterParameters parameters);
}
