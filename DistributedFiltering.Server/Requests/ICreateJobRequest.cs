﻿namespace DistributedFiltering.Server.Requests;

public interface ICreateJobRequest
{
	string ResultFileName { get; init; }
	BatchSize BatchSize { get; init; }
}
