namespace DistributedFiltering.Server.Requests;

public enum BatchSize
{
	Default = 0,
	Tiny = 1,
	Small = 2,
	Large = 3,
	Extream = 4
}

public static class BatchSizeExtensions
{
	public static int GetSize(this BatchSize size)
	{
		return size switch
		{
			BatchSize.Tiny => 4096, //507 batches for FULL HD
			BatchSize.Small => 48_000, //44 batches for FULL HD
			BatchSize.Default => 96_000, //22 batches for FULL HD
			BatchSize.Large => 192_000, //11 batches for FULL HD
			BatchSize.Extream => 350_000, //6 batches for FULL HD
			_ => 96_000
		};
	}
}
