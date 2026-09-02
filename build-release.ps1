<# 
.SYNOPSIS
    PvZWSTools One-Click Build + Package + Upload GitHub Release

.USAGE
    .\build-release.ps1
    .\build-release.ps1 -Upload
    .\build-release.ps1 -Upload -Tag v2026.09.03
#>

param(
    [string]$Tag = "",
    [switch]$Upload,
    [string]$GithubToken = "",
    [string]$Repo = "AmourLing/PvZWSTools",
    [switch]$SkipAndroid,
    [switch]$SkipSetup
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$PublishDir = Join-Path $ProjectRoot "publish"
$WpfCsproj = Join-Path $ProjectRoot "PvZWSTools_WPF\PvZWSTools_WPF.csproj"
$AndroidCsproj = Join-Path $ProjectRoot "PvZWSTools_Avalonia\PvZWSTools_Avalonia.csproj"

function Write-Step($msg) { Write-Host "`n=== $msg ===" -ForegroundColor Cyan }
function Write-Ok($msg) { Write-Host "    OK  $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "    WARN  $msg" -ForegroundColor Yellow }
function Write-Fail($msg) { Write-Host "    FAIL  $msg" -ForegroundColor Red; exit 1 }

# ============ Read Version ============
Write-Step "Read version from csproj"

[xml]$wpfXml = Get-Content $WpfCsproj
$verNode = $wpfXml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
$infoVerNode = $wpfXml.Project.PropertyGroup.InformationalVersion | Where-Object { $_ } | Select-Object -First 1

if(-not $verNode) { Write-Fail "No Version in csproj" }
$Version = "$verNode"
if($infoVerNode) { $InformationalVersion = "$infoVerNode" } else { $InformationalVersion = $Version }
if($Tag) { $ReleaseTag = $Tag } else { $ReleaseTag = "v$InformationalVersion" }

Write-Ok "Version: $Version"
Write-Ok "InformationalVersion: $InformationalVersion"
Write-Ok "Release Tag: $ReleaseTag"

# ============ Prepare Dirs ============
Write-Step "Clean and create publish dir"
if(Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }
New-Item -ItemType Directory -Path $PublishDir | Out-Null
New-Item -ItemType Directory -Path (Join-Path $PublishDir "win-self-contained") | Out-Null
New-Item -ItemType Directory -Path (Join-Path $PublishDir "win-fwdep") | Out-Null

# ============ WPF self-contained ============
Write-Step "Build WPF self-contained"
dotnet publish $WpfCsproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true -o "$PublishDir\win-self-contained" 2>&1 | Out-Null
if($LASTEXITCODE -ne 0) { Write-Fail "WPF self-contained build failed" }
Write-Ok "WPF self-contained done"

# ============ WPF framework-dependent ============
Write-Step "Build WPF framework-dependent"
dotnet publish $WpfCsproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false -o "$PublishDir\win-fwdep" 2>&1 | Out-Null
if($LASTEXITCODE -ne 0) { Write-Fail "WPF framework-dependent build failed" }
Write-Ok "WPF framework-dependent done"

# ============ Zip ============
Write-Step "Create zip packages"
$scZip = Join-Path $PublishDir "PvZWSTools_windows_self-contained.zip"
$fdZip = Join-Path $PublishDir "PvZWSTools_windows_framework-dependent.zip"
Compress-Archive -Path "$PublishDir\win-self-contained\*" -DestinationPath $scZip -Force
Compress-Archive -Path "$PublishDir\win-fwdep\*" -DestinationPath $fdZip -Force
Write-Ok "self-contained: $([math]::Round((Get-Item $scZip).Length/1MB,1)) MB"
Write-Ok "framework-dependent: $([math]::Round((Get-Item $fdZip).Length/1MB,1)) MB"

# ============ Android APK ============
$apkPath = $null
if(-not $SkipAndroid) {
    Write-Step "Build Android APK"
    dotnet publish $AndroidCsproj -c Release -r android-arm64 -p:AndroidPackageFormat=apk 2>&1 | Out-Null
    if($LASTEXITCODE -ne 0) {
        Write-Warn "Android APK build failed, skip"
    } else {
        $apkSrc = Get-ChildItem "$ProjectRoot\PvZWSTools_Avalonia\bin\Release\net10.0-android\android-arm64\publish" -Filter "*Signed.apk" -ErrorAction SilentlyContinue | Select-Object -First 1
        if($apkSrc) {
            $apkPath = Join-Path $PublishDir "PvZWSTools_android.apk"
            Copy-Item $apkSrc.FullName $apkPath -Force
            Write-Ok "APK: $([math]::Round((Get-Item $apkPath).Length/1MB,1)) MB"
        } else {
            Write-Warn "No signed APK found, skip"
        }
    }
} else {
    Write-Warn "Skip Android"
}

# ============ Inno Setup ============
$setupExe = $null
if(-not $SkipSetup) {
    Write-Step "Compile Inno Setup"
    $iscc = "D:\InnoSetup\Inno Setup 6\ISCC.exe"
    $allIss = Get-ChildItem "D:\InnoSetup\Inno Setup 6\ms\*.iss"
    $iss = $allIss | Where-Object { $_.Name -match "^PvZWSTools" } | Select-Object -First 1 -ExpandProperty FullName
    
    if(-not $iss) {
        Write-Warn "PvZWSTools ISS not found. All ISS files:"
        $allIss | ForEach-Object { Write-Warn "  $($_.Name)" }
    }
    
    if(-not (Test-Path $iscc)) {
        Write-Warn "ISCC not found: $iscc"
    } elseif(-not $iss -or -not (Test-Path $iss)) {
        Write-Warn "ISS not found"
    } else {
        & $iscc $iss "/DMyAppVersion=$InformationalVersion" "/DSourcePath=$PublishDir\win-self-contained" 2>&1 | Out-Null
        if($LASTEXITCODE -ne 0) {
            Write-Warn "Inno Setup compile failed, skip"
        } else {
            $setupDir = Split-Path $iss
            $setupOutput = Join-Path $setupDir "Output"
            $setupSrc = Get-ChildItem $setupOutput -Filter "*.exe" | Select-Object -First 1
            if($setupSrc) {
                $setupExe = Join-Path $PublishDir "PvZWSTools_windows_setup.exe"
                Copy-Item $setupSrc.FullName $setupExe -Force
                Write-Ok "Setup: $([math]::Round((Get-Item $setupExe).Length/1MB,1)) MB"
            }
        }
    }
} else {
    Write-Warn "Skip Inno Setup"
}

# ============ Cleanup intermediate dirs ============
Write-Step "Cleanup intermediate dirs"
Remove-Item "$PublishDir\win-self-contained" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$PublishDir\win-fwdep" -Recurse -Force -ErrorAction SilentlyContinue
Write-Ok "Removed win-self-contained/ and win-fwdep/"

# ============ Summary ============
Write-Step "Build artifacts"
$allFiles = @($scZip, $fdZip)
if($setupExe) { $allFiles += $setupExe }
if($apkPath) { $allFiles += $apkPath }
foreach($f in $allFiles) {
    $sizeMB = [math]::Round((Get-Item $f).Length/1MB, 1)
    Write-Ok "$([System.IO.Path]::GetFileName($f))  ($sizeMB MB)"
}

# ============ Upload ============
if($Upload) {
    $token = $GithubToken
    if(-not $token) { $token = $env:GITHUB_TOKEN }
    
    if(-not $token) {
        Write-Warn "GITHUB_TOKEN not found."
        $secureToken = Read-Host "Enter your GitHub Personal Access Token" -AsSecureString
        $token = [System.Net.NetworkCredential]::new("", $secureToken).Password
        
        if(-not $token) { Write-Fail "No token provided, abort upload." }
        
        Write-Host "    Save to user environment variable for next time? (Y/N): " -NoNewline
        $save = Read-Host
        if($save -match "^[Yy]") {
            [Environment]::SetEnvironmentVariable("GITHUB_TOKEN", $token, "User")
            $env:GITHUB_TOKEN = $token
            Write-Ok "Saved to user env var. Will be auto-loaded next time."
        }
    }
    
    Write-Step "Upload to GitHub Release: $ReleaseTag"
    $headers = @{ "Authorization" = "token $token" }
    $releaseId = $null
    $releaseUrl = ""
    
    try {
        $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/releases/tags/$ReleaseTag" -Headers $headers
        Write-Ok "Release exists: $($release.html_url)"
        $releaseId = $release.id
        $releaseUrl = $release.html_url
        foreach($a in $release.assets) {
            Write-Warn "Remove old asset: $($a.name)"
            Invoke-RestMethod -Uri $a.url -Method Delete -Headers $headers | Out-Null
        }
    } catch {
        Write-Ok "Creating new Release..."
        $body = @{
            tag_name = $ReleaseTag
            name = "PvZWSTools $ReleaseTag"
            body = "Release $InformationalVersion"
            draft = $false
            prerelease = $false
        } | ConvertTo-Json
        $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/releases" -Method Post -Headers $headers -Body $body -ContentType "application/json"
        $releaseId = $release.id
        $releaseUrl = $release.html_url
    }
    
    $mimeMap = @{
        ".zip" = "application/zip"
        ".exe" = "application/x-msdownload"
        ".apk" = "application/vnd.android.package-archive"
    }
    
    foreach($f in $allFiles) {
        $name = [System.IO.Path]::GetFileName($f)
        $ext = [System.IO.Path]::GetExtension($f).ToLower()
        if($mimeMap.ContainsKey($ext)) {
            $mime = $mimeMap[$ext]
        } else {
            $mime = "application/octet-stream"
        }
        $sizeMB = [math]::Round((Get-Item $f).Length/1MB, 1)
        
        Write-Host "    Uploading $name ($sizeMB MB)..." -NoNewline
        $h = @{ "Authorization" = "token $token"; "Content-Type" = $mime }
        $bytes = [System.IO.File]::ReadAllBytes($f)
        Invoke-RestMethod -Uri "https://uploads.github.com/repos/$Repo/releases/$releaseId/assets?name=$name" -Method Post -Headers $h -Body $bytes | Out-Null
        Write-Host " OK" -ForegroundColor Green
    }
    
    Write-Ok "Release URL: $releaseUrl"
}

# ============ Done ============
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  DONE! Artifacts: $PublishDir" -ForegroundColor Green
if($Upload) { Write-Host "  Uploaded to GitHub Release" -ForegroundColor Green }
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
