# Parse command-line arguments.
param (
   [Parameter(Mandatory=$true)][string]$cert,
   [string]$timestamp = "http://timestamp.digicert.com", # http://timestamp.comodoca.com
   [string]$inno = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
   [string]$signtool = 'signtool.exe',
   [switch]$v = $false
)

# Any error kills the script.
$ErrorActionPreference = 'Stop'

if ($v) {
    $msbuildVerbose = '/v:n'
    $signtoolVerbose = "/v"
    $innoVerbose = ""
} else {
    $msbuildVerbose = "/v:q"
    $signtoolVerbose = "/q"
    $innoVerbose = "/Qp"
}

# Read the password.
$p = Read-Host 'Enter key password' -AsSecureString
$pass = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($p))

# Build the code.
Write-Host -ForegroundColor Green 'Building x86'
& msbuild /t:Rebuild '/p:Configuration=Release;Platform=x86' /nologo $msbuildVerbose
Write-Host -ForegroundColor Green 'Building x64'
& msbuild /t:Rebuild '/p:Configuration=Release;Platform=x64' /nologo $msbuildVerbose

# Sign the binaries.
#$files = (Get-ChildItem -Path bin\x86\Release -Recurse -Filter "*.exe").FullName
$files = 'bin\x86\Release\x3270is.dll', 'bin\x64\Release\x3270is.dll', 'bin\x86\Release\Import\x86\s3270.exe', 'bin\x64\Release\Import\x86\s3270.exe'
Write-Host -ForegroundColor Green 'Signing', $files.Length, 'files'
& $signtool sign $signtoolVerbose /f $cert /p $pass /td SHA256 /tr $timestamp $files

# Run Inno Setup to create the installer.
Write-Host -ForegroundColor Green 'Running Inno Setup'
$signparm = '/ssigntool="' + "$signtool sign $signtoolVerbose /f `$q$cert`$q /p `$q$pass`$q /td SHA256 /tr $timestamp `$f" + '"'
& $inno $signparm $innoVerbose x3270is.iss