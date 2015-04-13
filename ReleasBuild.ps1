$msBuildDirectory = "C:\Windows\Microsoft.NET\Framework\v4.0.30319"

$project ="Dagent"

$netVersions = "v3.5", "v4.0", "v4.5"

$mainProject = $project + ".csproj";

$directory = Split-Path $script:myInvocation.MyCommand.path -parent

Remove-Item $directory\Release -Force -ErrorAction Ignore -Recurse

$directory = $directory + "\Dagent"

foreach($version in $netVersions)
{
    $versionName = $version.Replace(".", "")

    $text = Get-Content $directory\$mainProject -Raw

    $verText = $text.Replace("<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>", "<TargetFrameworkVersion>" + $version + "</TargetFrameworkVersion>")
    $verText = $verText.Replace("<DebugType>pdbonly</DebugType>", "")    
    $verText = $verText.Replace("<OutputPath>bin\Release\</OutputPath>", "<OutputPath>..\Release\" + $versionName + "\</OutputPath>")

    $verProject = $project + "." + $versionName + ".csproj";

    $verText| Out-File $directory\$verProject -Encoding UTF8

    cmd /c $msBuildDirectory\msbuild.exe /p:Configuration=Release $directory\$verProject    

    Remove-Item $directory\$verProject -Force -ErrorAction Ignore
}