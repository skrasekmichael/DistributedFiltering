namespace DistributedFiltering.Server.Requests;

public sealed class CreateBilateralJobRequest : ICreateJobRequest
{
	public double SpatialSigma { get; set; }
	public double RangeSigma { get; set; }
}
