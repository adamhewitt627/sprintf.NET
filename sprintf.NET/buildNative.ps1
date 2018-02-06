try {
    Push-Location $PSScriptRoot
    $vcvarsall = (Get-ChildItem 'C:\Program Files (x86)\Microsoft Visual Studio\*\*\VC\Auxiliary\Build\vcvarsall.bat' -Recurse | Select -First 1)

    $source = Get-Item "sprintf-native.cpp"
    function build ($architecture, $output = $architecture) {
        New-Item $output -ItemType Directory -Force
        $outFile = Join-Path (Get-Location) "$output\$($source.BaseName).dll"
        Write-Host ($options -join " ")
        cmd /C "`"$vcvarsall`" $architecture & powershell -Command `"&{ cl -MD $($source) -D_ARM_WINAPI_PARTITION_DESKTOP_SDK_AVAILABLE=1 -link -appcontainer -dll -out:`"$outFile`" }`""
    }

    build x86
    build x64
    build x86_arm ARM
} finally { Pop-Location }