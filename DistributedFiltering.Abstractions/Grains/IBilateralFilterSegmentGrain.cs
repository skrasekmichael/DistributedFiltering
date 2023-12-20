using DistributedFiltering.Abstractions.Contracts;

namespace DistributedFiltering.Abstractions.Grains;

public interface IBilateralFilterSegmentGrain : IFilterSegmentGrain<BilateralFilterParams>
{
}
