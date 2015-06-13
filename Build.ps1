param(
    [String] $majorMinor = "0.0",  # 1.4
    [String] $patch = "0",         # $env:APPVEYOR_BUILD_VERSION
    [String] $branch = "private",  # $env:APPVEYOR_REPO_BRANCH
    [String] $customLogger = "",   # C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll
    [Switch] $notouch
)

function Set-AssemblyVersions($informational, $file, $assembly)
{
    (Get-Content assets/CommonAssemblyInfo.cs) |
        ForEach-Object { $_ -replace """1.0.0.0""", """$assembly""" } |
        ForEach-Object { $_ -replace """1.0.0""", """$informational""" } |
        ForEach-Object { $_ -replace """1.1.1.1""", """$file""" } |
        Set-Content assets/CommonAssemblyInfo.cs
}

function Install-NuGetPackages($solution)
{
    nuget restore $solution
}

function Invoke-MSBuild($solution, $customLogger)
{
	if ($customLogger)
    {
        msbuild "$solution" /verbosity:minimal /p:Configuration=Release /logger:"$customLogger"
    }
    else
    {
        msbuild "$solution" /verbosity:minimal /p:Configuration=Release
    }
}

function Invoke-NuGetPackProj($csproj)
{
    nuget pack -Prop Configuration=Release -Symbols $csproj
}

function Invoke-NuGetPackSpec($nuspec, $version)
{
    nuget pack $nuspec -Version $version -OutputDirectory ..\..\
}

function Invoke-NuGetPack($version)
{
    pushd .\src\Serilog.Sinks.Literate
    Invoke-NuGetPackSpec "Serilog.Sinks.Literate.nuspec" $version
    popd
}

function Invoke-Build($majorMinor, $patch, $branch, $customLogger, $notouch)
{
    $target = (Get-Content ./CHANGES.md -First 1).Trim()
    $file = "$target.$patch"
    $package = $target
    if ($branch -ne "master")
    {
        $package = "$target-pre-$patch"
    }

    if (-not $notouch)
    {
        $assembly = "$majorMinor.0.0"

        Write-Output "Assembly version will be set to $assembly"
        Set-AssemblyVersions $package $file $assembly
    }

    Install-NuGetPackages "serilog-sinks-literate.sln"
    
    Invoke-MSBuild "serilog-sinks-literate-net40.sln" $customLogger
    Invoke-MSBuild "serilog-sinks-literate.sln" $customLogger

    Invoke-NuGetPack $package
}

$ErrorActionPreference = "Stop"
Invoke-Build $majorMinor $patch $branch $customLogger $notouch

