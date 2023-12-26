using DistributedFiltering.Abstractions.Contracts;
using DistributedFiltering.Abstractions.Interfaces;
using DistributedFiltering.Filters.Filters;

namespace DistributedFiltering.Filters.Extensions;

public static class FilterParametersExtensions
{
	public static IDistributedFilter GetFilter(this IFilterParameters filterParameters)
	{
		return filterParameters switch
		{
			GaussianNoiseParams => new AddGaussianNoiseFilter(),
			BilateralFilterParams => new BilateralFilter(),
			_ => throw new Exception()
		};
	}
}

