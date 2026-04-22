# Copies MudBlazor static assets from the global NuGet package cache into the Web.Client wwwroot lib folder.
# Run from repository root in PowerShell: .\tools\copy-mudblazor.ps1

param()

try {
    Write-Host "Detecting global-packages folder..."
    $gpOutput = dotnet nuget locals global-packages --list 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to run 'dotnet nuget locals'. Ensure dotnet CLI is installed and on PATH. Output: $gpOutput"
        exit 1
    }
    $gp = ($gpOutput -split ":" | Select-Object -Last 1).Trim()
    if (-not (Test-Path $gp)) {
        Write-Error "Global packages folder not found: $gp"
        exit 1
    }

    Write-Host "Global packages folder: $gp"

    $packageId = 'mudblazor'
    $packageVersion = '9.3.0'

    $packagePath = Join-Path $gp "$packageId\$packageVersion"
    if (-not (Test-Path $packagePath)) {
        Write-Error "Package folder not found: $packagePath"
        Write-Host "Make sure the package is restored. Try: dotnet restore ./OIL.Web.Client/OIL.Web.Client.csproj"
        exit 1
    }

    # Find CSS and JS files
    $foundCss = Get-ChildItem -Path $packagePath -Recurse -Include *.css -ErrorAction SilentlyContinue | Where-Object { $_.Name -match 'MudBlazor.*\.css' } | Select-Object -First 1
    $foundJs = Get-ChildItem -Path $packagePath -Recurse -Include *.js -ErrorAction SilentlyContinue | Where-Object { $_.Name -match 'MudBlazor.*\.js' } | Select-Object -First 1

    if (-not $foundCss) {
        Write-Error "Could not find MudBlazor CSS in package folder. Searched: $packagePath"
    } else {
        Write-Host "Found CSS: $($foundCss.FullName)"
    }
    if (-not $foundJs) {
        Write-Warning "Could not find MudBlazor JS in package folder. JS may be optional."
    } else {
        Write-Host "Found JS: $($foundJs.FullName)"
    }

    $destDir = Join-Path (Resolve-Path .\OIL.Web.Client\wwwroot).Path "lib\mudblazor"
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null

    if ($foundCss) {
        Copy-Item -Path $foundCss.FullName -Destination (Join-Path $destDir $foundCss.Name) -Force
        Write-Host "Copied CSS to $destDir\$($foundCss.Name)"
    }
    if ($foundJs) {
        Copy-Item -Path $foundJs.FullName -Destination (Join-Path $destDir $foundJs.Name) -Force
        Write-Host "Copied JS to $destDir\$($foundJs.Name)"
    }

    Write-Host "Done. Now run the client app and verify: http://localhost:5000/lib/mudblazor/$($foundCss.Name)"
    Write-Host "If the file is accessible, index.html already references lib/mudblazor/MudBlazor.min.css as a local fallback."
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
