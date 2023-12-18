namespace DistributedFiltering.API;

public sealed class CreateBilateralJobRequest
{
	public int UnitsCount { get; set; }
	public double SpatialSigma { get; set; }
	public double RangeSigma { get; set; }
}
