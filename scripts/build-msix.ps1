[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.2.0",
    [string]$IdentityName = "Guts444.Dialkin",
    [string]$Publisher = "CN=0CE853A2-4D9A-43A6-B4CC-9371C515BC1C",
    [string]$PublisherDisplayName = "Guts444",
    [string]$WindowsSdkVersion = "10.0.26100.0",
    [switch]$RequireCleanWorktree,
    [switch]$SignWithTestCertificate
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src\Dialkin.App\Dialkin.App.csproj"
$artifacts = Join-Path $root "artifacts\msix"
$publish = Join-Path $artifacts "publish"
$stage = Join-Path $artifacts "stage"
$output = Join-Path $artifacts "Dialkin-$Version-x64.msix"

if ($Version -notmatch '^([1-9]\d{0,4})\.(\d{1,5})\.(\d{1,5})\.0$' -or
    ([int[]]$Version.Split('.') | Where-Object { $_ -gt 65535 }).Count -gt 0) {
    throw "MSIX version must have a nonzero major version, four numeric parts, and a zero Store-reserved revision."
}

$gitCommit = (git -C $root rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw "Unable to determine the source revision." }
$gitStatus = @(git -C $root status --porcelain)
if ($RequireCleanWorktree -and $gitStatus.Count -gt 0) {
    throw "The working tree must be clean when -RequireCleanWorktree is used."
}

Remove-Item $publish, $stage -Recurse -Force -ErrorAction SilentlyContinue
New-Item $publish, $stage -ItemType Directory -Force | Out-Null

dotnet publish $project `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $publish
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }

Copy-Item (Join-Path $publish '*') $stage -Recurse -Force
Remove-Item (Join-Path $stage '*.pdb') -Force -ErrorAction SilentlyContinue
Copy-Item (Join-Path $root 'packaging\Assets') $stage -Recurse -Force

[xml]$manifest = Get-Content (Join-Path $root 'packaging\AppxManifest.xml') -Raw
$manifest.Package.Identity.Name = $IdentityName
$manifest.Package.Identity.Publisher = $Publisher
$manifest.Package.Identity.Version = $Version
$manifest.Package.Properties.PublisherDisplayName = $PublisherDisplayName
$manifest.Save((Join-Path $stage 'AppxManifest.xml'))

$kitRoot = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
$kitVersion = Join-Path $kitRoot $WindowsSdkVersion
if (-not (Test-Path (Join-Path $kitVersion 'x64\MakeAppx.exe'))) {
    throw "Windows SDK $WindowsSdkVersion with MakeAppx.exe was not found."
}

$makeAppx = Join-Path $kitVersion 'x64\MakeAppx.exe'
$signTool = Join-Path $kitVersion 'x64\SignTool.exe'
Remove-Item $output -Force -ErrorAction SilentlyContinue
& $makeAppx pack /d $stage /p $output /o
if ($LASTEXITCODE -ne 0) { throw "MakeAppx failed." }

if ($SignWithTestCertificate) {
    $certificate = Get-ChildItem Cert:\CurrentUser\My |
        Where-Object { $_.Subject -eq $Publisher -and $_.HasPrivateKey } |
        Sort-Object NotAfter -Descending |
        Select-Object -First 1

    if (-not $certificate) {
        $certificate = New-SelfSignedCertificate `
            -Type Custom `
            -Subject $Publisher `
            -FriendlyName 'Dialkin MSIX Development' `
            -CertStoreLocation 'Cert:\CurrentUser\My' `
            -KeyAlgorithm RSA `
            -KeyLength 2048 `
            -HashAlgorithm SHA256 `
            -KeyUsage DigitalSignature `
            -TextExtension @(
                '2.5.29.19={text}'
                '2.5.29.37={text}1.3.6.1.5.5.7.3.3'
            )
    }

    $certificatePath = Join-Path $artifacts 'Dialkin-Development.cer'
    Export-Certificate -Cert $certificate -FilePath $certificatePath -Force | Out-Null
    & $signTool sign /fd SHA256 /sha1 $certificate.Thumbprint $output
    if ($LASTEXITCODE -ne 0) { throw "SignTool failed." }

    Write-Host "Signed with development certificate: $certificatePath"
}

$packageHash = (Get-FileHash $output -Algorithm SHA256).Hash.ToLowerInvariant()
$provenancePath = Join-Path $artifacts "Dialkin-$Version-x64.provenance.json"
[ordered]@{
    package = Split-Path $output -Leaf
    sha256 = $packageHash
    sourceCommit = $gitCommit
    sourceDirty = $gitStatus.Count -gt 0
    dotnetSdk = (dotnet --version).Trim()
    windowsSdk = $WindowsSdkVersion
    identityName = $IdentityName
    publisher = $Publisher
    version = $Version
} | ConvertTo-Json | Set-Content $provenancePath -Encoding utf8

Write-Host "SHA-256: $packageHash"
Write-Host "Provenance: $provenancePath"
Write-Host "MSIX created: $output"
