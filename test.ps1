param (
	$Image = "street",
	$ClientCount = 6,
	$Port = 4999,
	[ValidateSet("bilateral", "noise")]
	$Filter = "bilateral"
)

$serverDir = "$PSScriptRoot/DistributedFiltering.Server/bin/Debug/net8.0/";
$clientDir = "$PSScriptRoot/DistributedFiltering.Client/bin/Debug/net8.0/";
$serverApi = "http://127.0.0.1:$Port";
$api = "$serverApi/api";

function Print-Response {
	param (
		$Response
	)

	$stateMapping = @{
		0l = "Not Started"
		1l = "Preparing"
		2l = "In Progress"
		3l = "Completed"
		4l = "Canceled"
	};

	Write-Host "State: $($stateMapping[$Response.State]) $($Response.Progress)%";

	$index = 1;
	foreach ($segment in $Response.SegmentStatuses) {
		$progress = $segment.Progress;
		$progress = "{0:N2}" -f $segment.Progress;
		$segmentState = $segment.State;
		Write-Host "-> worker #$index progress: $progress% ($($stateMapping[$segmentState]))";

		$index++;
	}
}

function Run-Server {
	return Start-Process -FilePath dotnet -ArgumentList "DistributedFiltering.Server.dll", "--urls", $serverApi -WorkingDirectory $serverDir -PassThru;
}

function Run-Clients {
	param (
		[int]$Count = 1
	)

	$clients = New-Object System.Collections.ArrayList;

	for ($i = 0; $i -lt $Count; $i++) {
		$client = Start-Process -FilePath dotnet -ArgumentList "DistributedFiltering.Client.dll" -WorkingDirectory $clientDir -PassThru;
		$clients.Add($client) | Out-Null;
	}

	return $clients;
}

$server = Run-Server
$clients = Run-Clients -Count $ClientCount

Start-Sleep -Seconds 6;

if ($Filter -eq "bilateral") {
	$parameters = @{
		"SpatialSigma" = 7
		"RangeSigma" = 30
		"ResultFileName" = "$Image.result"
	} | ConvertTo-Json;
	$filterName = "apply-bilateral-filter";
} elseif ($Filter -eq "noise") {
	$parameters = @{
		"Sigma" = 30
		"ResultFileName" = "$Image.result"
	} | ConvertTo-Json;
	$filterName = "add-gaussian-noise";
}

Invoke-RestMethod -Method Post -ContentType "application/json" -Uri "$api/$Image/$filterName" -Body $parameters;

do {
	$response = Invoke-RestMethod -ContentType "application/json" -Uri "$api/status";
	Print-Response -Response $response;
	Start-Sleep -Seconds 5;
} until ($response.State -eq "3"); # 3 = completed

Invoke-Item "$serverDir/wwwroot/$Image.result.png";
$clients | % { Stop-Process $_.Id }
Stop-Process $server.Id;
