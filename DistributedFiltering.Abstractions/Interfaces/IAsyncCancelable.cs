namespace DistributedFiltering.Abstractions.Interfaces;

public interface IAsyncCancelable
{
	ValueTask CancelAsync();
}
