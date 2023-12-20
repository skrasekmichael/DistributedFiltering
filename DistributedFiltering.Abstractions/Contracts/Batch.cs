namespace DistributedFiltering.Abstractions.Contracts;

[GenerateSerializer, Immutable]
public sealed class Batch
{
	[Id(0), Immutable]
	public required byte[] Input { get; init; }
	[Id(1)]
	public required Size Size { get; init; }
	[Id(2)]
	public required Rectangle FilteringWindow { get; init; }
	[Id(3)]
	public required Size ImageSize { get; init; }
}
