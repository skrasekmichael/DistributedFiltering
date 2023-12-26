namespace DistributedFiltering.Server.Requests;

public sealed class CreateBilateralJobRequest : ICreateJobRequest
{
	public double SpatialSigma { get; set; }
	public double RangeSigma { get; set; }
	public required string ResultFileName { get; init; }
	public BatchSize BatchSize { get; init; } = BatchSize.Default;
}
