namespace DistributedFiltering.Abstractions.Contracts;

[GenerateSerializer, Immutable]
public readonly struct Rectangle(int x, int y, int w, int h)
{
	[Id(0)]
	public int X { get; init; } = x;
	[Id(1)]
	public int Y { get; init; } = y;
	[Id(2)]
	public int Width { get; init; } = w;
	[Id(3)]
	public int Height { get; init; } = h;

	public override string ToString() => $"[{X},{Y},{Width},{Height}]";
}
