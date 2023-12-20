namespace DistributedFiltering.Abstractions.Contracts;

[GenerateSerializer, Immutable]
public sealed class ImageData
{
	[Id(0), Immutable]
	public required byte[] Data { get; init; }
	[Id(1)]
	public required int Width { get; init; }
	[Id(2)]
	public required int Height { get; init; }
}
