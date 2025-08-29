<#
.SYNOPSIS
    Automated Windows driver signing and installation for development
.DESCRIPTION
    Creates test certificates, signs driver files, and installs the driver
    with proper test-signing mode configuration.
.NOTES
    Run as Administrator
    Requires Windows SDK (for signtool and inf2cat)
#>

param(
    [switch]$Verbose = $false
)

#region Configuration
$config = Get-Content '.\config\ps-config.json' | ConvertFrom-Json
$DriverName = $config.Parameters.DriverName
$PrinterName = $config.Parameters.PrinterName
$DriverCatDebug = $config.Parameters.DriverCatDebug
$CertPassword = $config.Parameters.CertPassword
$TimeStampServer = $config.Parameters.TimeStampServer
$Signtool = $config.Parameters.Signtool
$Inf2Cat = $config.Parameters.Inf2Cat
$CertSubject = $config.Parameters.CertSubject

$WorkDir = Join-Path (Get-Location).Path $DriverCatDebug
$CatFileName = "$DriverName.cat"
$InfFileName = "$DriverName.inf"
$PfxFileName = "$DriverName.pfx"
$CatFilePath = Join-Path $WorkDir $CatFileName
$PfxFilePath = Join-Path $WorkDir $PfxFileName
$InfFilePath = Join-Path $WorkDir $InfFileName
#endregion

#region Helper Functions
function Write-VerboseOutput {
    param([string]$Message)
    if ($Verbose) {
        Write-Host "[VERBOSE] $(Get-Date -Format 'HH:mm:ss') - $Message" -ForegroundColor DarkGray
    }
}

function Handle-Error {
    param([string]$Context)
    Write-Host "[ERROR] $Context" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Line: $($_.InvocationInfo.ScriptLineNumber)" -ForegroundColor Red
    exit 1
}

function Test-Admin {
    return ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}
#endregion

#region 1. Certificate Creation
function New-TestCertificate {
    try {
        Write-Host "`n[1/6] Creating test certificates..." -ForegroundColor Cyan
        
        # Remove existing certificates
        Get-ChildItem Cert:\LocalMachine\My, Cert:\LocalMachine\Root | 
            Where-Object { $_.Subject -match $CertSubject } | 
            Remove-Item -Force -ErrorAction SilentlyContinue

        # Create self-signed certificate in LocalMachine store
        $cert = New-SelfSignedCertificate -Type CodeSigningCert `
            -Subject "CN=$CertSubject" `
            -KeyUsage DigitalSignature `
            -KeyLength 2048 `
            -HashAlgorithm SHA256 `
            -CertStoreLocation "Cert:\LocalMachine\My" `
            -KeyExportPolicy Exportable `
            -NotAfter (Get-Date).AddYears(1)

        # Export as PFX
        $securePass = ConvertTo-SecureString -String $CertPassword -Force -AsPlainText
        Export-PfxCertificate -Cert $cert -FilePath $PfxFilePath -Password $securePass | Out-Null

        # Import into Trusted Root store
        $cert | Export-Certificate -FilePath "$WorkDir\$DriverName.cer" | Out-Null
        Import-Certificate -FilePath "$WorkDir\$DriverName.cer" -CertStoreLocation "Cert:\LocalMachine\Root" | Out-Null

        Write-Host "Certificate created and trusted" -ForegroundColor Green
        return $cert
    }
    catch {
        Handle-Error "Certificate creation failed"
    }
}
#endregion

#region 2. Enable Test Signing
function Enable-TestSigning {
    try {
        Write-Host "`n[2/6] Configuring test-signing mode..." -ForegroundColor Cyan
        
        $testSigning = (bcdedit /enum | Select-String "testsigning").ToString().Contains("Yes")
        
        if (-not $testSigning) {
            bcdedit /set testsigning on | Out-Null
            Write-Host "Test-signing enabled. Reboot required." -ForegroundColor Yellow
            return $true
        }
        else {
            Write-Host "Test-signing is already enabled" -ForegroundColor Green
            return $false
        }
    }
    catch {
        Handle-Error "Test signing configuration failed"
    }
}
#endregion

#region 3. Generate Catalog File
function Generate-Catalog-File {
    try {
        Write-Host "`n[3/6] Generating catalog file..." -ForegroundColor Cyan
        
        # Use inf2cat instead of New-FileCatalog for driver catalogs
        & $Inf2Cat /driver:"$WorkDir" /os:10_X64,10_X86 /uselocaltime
        
        if ($LASTEXITCODE -ne 0) {
            throw "inf2cat failed with exit code $LASTEXITCODE"
        }
        
        Write-Host "Catalog file generated successfully" -ForegroundColor Green
    }
    catch {
        Handle-Error "Catalog generation failed"
    }
}
#endregion

#region 4. Sign Catalog File
function Sign-Catalog {
    try {
        Write-Host "`n[4/6] Signing catalog file..." -ForegroundColor Cyan
        
        if (-not (Test-Path $CatFilePath)) {
            throw "Catalog file not found: $CatFilePath"
        }
        
        & $Signtool sign /fd SHA256 /f $PfxFilePath /p $CertPassword /tr $TimeStampServer /td SHA256 /v $CatFilePath
        
        if ($LASTEXITCODE -ne 0) {
            throw "Signing failed with exit code $LASTEXITCODE"
        }
        
        # Verify the signature
        & $Signtool verify /v /pa $CatFilePath
        
        if ($LASTEXITCODE -ne 0) {
            throw "Signature verification failed"
        }
        
        Write-Host "Catalog file signed successfully" -ForegroundColor Green
    }
    catch {
        Handle-Error "Catalog signing failed"
    }
}
#endregion

#region 5. Install Driver
function Install-Driver {
    try {
        Write-Host "`n[5/6] Installing driver..." -ForegroundColor Cyan
        
        if (-not (Test-Path $InfFilePath)) {
            throw "INF file not found: $InfFilePath"
        }
        
        # Install driver using pnputil
        $result = pnputil.exe /add-driver $InfFilePath /install 2>&1
        Write-Host "PNPUtil output: $result" -ForegroundColor Gray
        
        if ($LASTEXITCODE -ne 0) {
            throw "Driver installation failed with exit code $LASTEXITCODE"
        }
        
        Write-Host "Driver installed successfully" -ForegroundColor Green
    }
    catch {
        Handle-Error "Driver installation failed"
    }
}
#endregion

#region 6. Add Printer
function Add-PrinterInstance {
    try {
        Write-Host "`n[6/6] Adding printer instance..." -ForegroundColor Cyan
        
        # Add printer driver
        Add-PrinterDriver -Name $DriverName -ErrorAction Stop
        
        # Add printer
        Add-Printer -Name $PrinterName -DriverName $DriverName -PortName "FILE:" -ErrorAction Stop
        
        Write-Host "Printer added successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "Warning: Printer creation failed (driver may still be installed): $($_.Exception.Message)" -ForegroundColor Yellow
    }
}
#endregion

#region Main Execution
try {
    # Check admin privileges
    if (-not (Test-Admin)) {
        Write-Host "This script must be run as Administrator" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Starting driver signing and installation process..." -ForegroundColor Green
    Write-Host "Working directory: $WorkDir" -ForegroundColor Gray
    
    # Execute steps
    $cert = New-TestCertificate
    $rebootRequired = Enable-TestSigning
    Generate-Catalog-File
    Sign-Catalog
    
    if ($rebootRequired) {
        Write-Host "`nREBOOT REQUIRED to enable test-signing mode." -ForegroundColor Red
        Write-Host "After reboot, run the installation commands manually:" -ForegroundColor Yellow
        Write-Host "pnputil /add-driver `"$InfFilePath`" /install" -ForegroundColor Yellow
        Write-Host "Add-PrinterDriver -Name `"$DriverName`"" -ForegroundColor Yellow
        Write-Host "Add-Printer -Name `"$PrinterName`" -DriverName `"$DriverName`" -PortName `"FILE:`"" -ForegroundColor Yellow
    }
    else {
        Install-Driver
        Add-PrinterInstance
        Write-Host "`n[COMPLETE] All operations completed successfully!" -ForegroundColor Green
    }
}
catch {
    Handle-Error "Main execution failed"
}
#endregion
