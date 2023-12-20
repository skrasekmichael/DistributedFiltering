namespace DistributedFiltering.API.Requests;

public sealed class CreateAddNoiseJobRequest : ICreateJobRequest
{
	public int UnitCount { get; set; }
	public double Sigma { get; set; }
}
