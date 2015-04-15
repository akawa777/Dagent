$msBuildDirectory = "C:\Windows\Microsoft.NET\Framework\v4.0.30319"

$project ="Dagent"

$netVersions = "v3.5", "v4.0", "v4.5"

$mainProject = $project + ".csproj";

$myDirectory = Split-Path $script:myInvocation.MyCommand.path -parent

$releaseDirectory = "ReleaseDlls"

Remove-Item $myDirectory\$releaseDirectory -Force -ErrorAction Ignore -Recurse

$directory = $myDirectory + "\Dagent"

foreach($version in $netVersions)
{
    $versionName = $version.Replace(".", "")

    $text = Get-Content $directory\$mainProject -Raw

    $verText = $text.Replace("<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>", "<TargetFrameworkVersion>" + $version + "</TargetFrameworkVersion>")
    $verText = $verText.Replace("<DebugType>pdbonly</DebugType>", "")    
    $verText = $verText.Replace("<OutputPath>bin\Release\</OutputPath>", "<OutputPath>..\$releaseDirectory\" + $versionName + "\</OutputPath>")

    $verProject = $project + "." + $versionName + ".csproj";

    $verText| Out-File $directory\$verProject -Encoding UTF8

    cmd /c $msBuildDirectory\msbuild.exe /p:Configuration=Release $directory\$verProject    

    Remove-Item $directory\$verProject -Force -ErrorAction Ignore
}

Remove-Item $myDirectory\Dagent.v12.suo -Force -ErrorAction Ignore -Recurse
Remove-Item $myDirectory\Dagent\bin -Force -ErrorAction Ignore -Recurse
Remove-Item $myDirectory\Dagent\obj -Force -ErrorAction Ignore -Recurse
Remove-Item $myDirectory\Dagent.Tests\bin -Force -ErrorAction Ignore -Recurse
Remove-Item $myDirectory\Dagent.Tests\obj -Force -ErrorAction Ignore -Recurse
Remove-Item $myDirectory\TestResults -Force -ErrorAction Ignore -Recurse
Remove-Item $myDirectory\packages -Force -ErrorAction Ignore -Recurse