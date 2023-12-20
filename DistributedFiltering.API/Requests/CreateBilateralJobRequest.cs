namespace DistributedFiltering.API.Requests;

public sealed class CreateBilateralJobRequest : ICreateJobRequest
{
	public int UnitCount { get; set; }
	public double SpatialSigma { get; set; }
	public double RangeSigma { get; set; }
}
