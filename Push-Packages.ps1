$mygetV2Feed = "https://www.myget.org/F/tigra-astronomy/api/v2/package"
$mygetV2SymbolFeed = "https://www.myget.org/F/tigra-astronomy/symbols/api/v2/package"
$mygetV3Feed = "https://www.myget.org/F/tigra-astronomy/api/v3/index.json"
$mygetApiKey = "231f5f9d-d1ee-4445-a9e0-5d7eafecbc46"
Push-Location .\Timtek.ReactiveCommunications\bin\Release
$packages = Get-ChildItem -Filter *.nupkg
foreach ($package in $packages)
{
	dotnet nuget push $package --api-key $mygetApiKey --symbol-api-key $mygetApiKey --source $mygetV3Feed --symbol-source $mygetV3Feed
}
