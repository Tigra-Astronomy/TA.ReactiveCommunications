$feed = "https://www.myget.org/F/tigra-astronomy/api/v2/package"
$symbolFeed = "https://www.myget.org/F/tigra-astronomy/symbols/api/v2/package"
Push-Location .\TA.Ascom.ReactiveCommunications\bin\Release
$packages = Get-ChildItem -Filter *.nupkg
foreach ($package in $packages)
{
    if ($package.Name -like "*.symbols.nupkg")
    {
        NuGet.exe push -Source $symbolFeed $package
    }
    else
    {
        NuGet.exe push -Source $feed $package
    }
}
Pop-Location