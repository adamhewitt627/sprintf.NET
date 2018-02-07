try {
    Push-Location $PSScriptRoot

    @("x86", "x64", "ARM") | ForEach-Object {
        $props = @{
            Configuration = "Release"
            Platform = $_
            Keyword = "WindowsRuntimeComponent"
            AppContainerApplication = "true"
            ApplicationType = "`"Windows Store`""
            OutDir = "..\sprintf.NET\runtimes\win10-$($_.ToLower())\native\"
        }
        msbuild -t:Clean,Build -p:(($props.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ";")

        Get-ChildItem $props.OutDir -Recurse  -File | ForEach-Object { 
            $dest = Join-Path $_.Directory.Parent.FullName $_.Name
            Move-Item $_.FullName -Destination $dest -Force
        }
        Remove-Item "$($props.OutDir)\sprintf-native"
    }

    @("x86", "x64"<#, "ARM"#>) | ForEach-Object {
        $props = @{ 
            Keyword = "Win32Proj"
            Platform = $_
            OutDir = "..\sprintf.NET\runtimes\win-$($_.ToLower())\native\"
        }
        msbuild -t:Clean,Build -p:(($props.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ";")
    }
} finally { Pop-Location }