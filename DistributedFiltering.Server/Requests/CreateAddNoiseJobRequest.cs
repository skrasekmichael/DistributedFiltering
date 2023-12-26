namespace DistributedFiltering.Server.Requests;

public sealed class CreateAddNoiseJobRequest : ICreateJobRequest
{
	public double Sigma { get; set; }
	public required string ResultFileName { get; init; }
}
