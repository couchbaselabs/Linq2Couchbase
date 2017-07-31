# is this a tagged build?
if ($env:APPVEYOR_REPO_TAG -eq "true") {
    # use tag as version
    $versionNumber = "$env:APPVEYOR_REPO_TAG_NAME"
} else {
    # create pre-release build number based on AppVeyor build number
    $buildCounter = "$env:APPVEYOR_BUILD_NUMBER".PadLeft(6, "0")
    $versionNumber = .\build-utils\AutoVersionNumber.ps1 -VersionSuffix "alpha-$buildCounter"
}

Write-Host "Using version: $versionNumber"
Update-AppveyorBuild -Version $versionNumber

# run unit tests
dotnet restore Src\couchbase-net-linq.sln
dotnet test -l "trx;LogFileName=tests.trx" -c Release Src\Couchbase.Linq.UnitTests\Couchbase.Linq.UnitTests.csproj

# upload test results
$wc = New-Object 'System.Net.WebClient'
$wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$($env:APPVEYOR_JOB_ID)", (Resolve-Path .\Src\Couchbase.Linq.UnitTests\TestResults\tests.trx))

# replace AssemblyInfo with version that doesn't include IntervalsVisibleTo attributes
Copy-Item .\build-utils\AssemblyInfo.cs .\Src\Couchbase.Linq\Properties\AssemblyInfo.cs -Force

# clean then build with version number creating nuget package
msbuild Src\Couchbase.Linq\Couchbase.Linq.csproj /t:Clean /p:Configuration=Release
msbuild Src\Couchbase.Linq\Couchbase.Linq.csproj /t:Restore,Pack /p:Configuration=Release /p:version=$versionNumber /p:PackageOutputPath=..\..\ /v:quiet

# create zip from release folder
Compress-Archive -Path .\Src\Couchbase.Linq\bin\Release\* -CompressionLevel Optimal -DestinationPath .\Couchbase.Linq-$versionNumber.zip
