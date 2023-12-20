namespace DistributedFiltering.Abstractions.Contracts;

[GenerateSerializer, Immutable]
public readonly struct Size(int w, int h)
{
	[Id(0)]
	public int Width { get; init; } = w;
	[Id(1)]
	public int Height { get; init; } = h;

	public override string ToString() => $"{{{Width},{Height}}}";
}
