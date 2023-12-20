using DistributedFiltering.Abstractions.Contracts;

namespace DistributedFiltering.Abstractions.Grains;

public interface IAddGaussianNoiseSegmentGrain : IFilterSegmentGrain<GaussianNoiseParams>
{
}
