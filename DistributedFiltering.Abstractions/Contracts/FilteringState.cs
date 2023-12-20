namespace DistributedFiltering.Abstractions.Contracts;

public enum FilteringState
{
	NotStarted,
	Preparing,
	InProgress,
	Completed,
	Canceled
}
