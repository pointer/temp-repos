<#
.SYNOPSIS
    Automated Windows driver signing and installation for development
.DESCRIPTION
    Creates test certificates, signs driver files, and installs the driver
    with proper test-signing mode configuration.
.NOTES
    Run as Administrator
    Requires Windows SDK (for signtool)
#>
<#
How to Use This Script
Save the script as DriverSigningAutomation.ps1

Prepare your driver files in a folder (e.g., C:\Drivers with YourDriver.sys and YourDriver.inf)

Run as Administrator:

In powershell
.\DriverSigningAutomation.ps1 -DriverName "MyDriver" -DriverPath "C:\MyDriver" -CertPassword "YourSecurePass"
#>

#>"Z:\DESKTOP_SOLUTIONS\Produkte\OSC\Kateb\_dev\10.0.22621.0\x86\Inf2Cat.exe" /driver:"Z:\DESKTOP_SOLUTIONS\Produkte\OSC\Kateb\_dev\UniLogoPrint_ULP2_PostScript\UniLogoPrint2\CPP\LPDriver\drivercatdebug" /os:10_VB_X64
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
    Write-Host "Error details:" -ForegroundColor Red
    Write-Host "  Exception: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Line: $($_.InvocationInfo.ScriptLineNumber)" -ForegroundColor Red
    Write-Host "  Command: $($_.InvocationInfo.Line.Trim())" -ForegroundColor Red
    
    if ($_.Exception.InnerException) {
        Write-Host "  Inner Exception: $($_.Exception.InnerException.Message)" -ForegroundColor Red
    }
    
    # Additional debug info for specific commands
    switch -Wildcard ($Context) {
        "*printer*" {
            $printerInfo = Get-Printer -Name $PrinterName -ErrorAction SilentlyContinue | Out-String
            Write-Host "Current printer state:`n$printerInfo" -ForegroundColor Yellow
        }
        "*driver*" {
            $driverInfo = Get-PrinterDriver -Name $DriverName -ErrorAction SilentlyContinue | Out-String
            Write-Host "Current driver state:`n$driverInfo" -ForegroundColor Yellow
        }
        "*certificate*" {
            $certInfo = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -match $DriverName } | Out-String
            Write-Host "Certificate store contents:`n$certInfo" -ForegroundColor Yellow
        }
    }
    
    exit 1
}
#endregion
#dism /online /get-driverinfo /driver:ulpdriverd.inf 
#region 1. Certificate Creation
function New-TestCertificate {
    try{
        Write-Host "`n[1/4] Creating test certificates..." -ForegroundColor Cyan
    
        # Remove existing certs if present
        $CertSubject = $global:config.Parameters.CertSubject
        Get-ChildItem  Cert:\CurrentUser\My, Cert:\CurrentUser\Root | 
            Where-Object { $_.Subject -match $CertSubject } | 
            Remove-Item -Force -ErrorAction SilentlyContinue
        
        # Create self-signed root certificate
        $rootCert = New-SelfSignedCertificate -Type CodeSigningCert `
            -Subject "$CertSubject" `
            -KeyUsage DigitalSignature, CertSign `
            -KeyUsageProperty All `
            -KeyLength 2048 `
            -HashAlgorithm SHA256 `
            -CertStoreLocation "Cert:\CurrentUser\My" `
            -KeyExportPolicy Exportable `
            -NotAfter (Get-Date).AddYears(1) `
            -FriendlyName "ULP Driver Dev Test"

        # Move to Trusted Root store
        $rootStore = Get-Item "Cert:\CurrentUser\Root"
        $rootStore.Open("ReadWrite")
        $rootStore.Add($rootCert)
        $rootStore.Close()
        $ParentDirectory = (Get-Location).Path

        $rootStore = Get-Item "Cert:\LocalMachine\My"
        $rootStore.Open("ReadWrite")
        $rootStore.Add($rootCert)
        $rootStore.Close()

        $rootStore = Get-Item "Cert:\LocalMachine\Root"
        $rootStore.Open("ReadWrite")
        $rootStore.Add($rootCert)
        $rootStore.Close()

        # Export as PFX
        $DriverCatDebug = $global:config.Parameters.DriverCatDebug
        $WorkDir = Join-Path (Get-Location).Path "DriverCatDebug"
        Set-Location -Path $WorkDir    
        $ExportedCert = New-TemporaryFile    
        Write-Output "Imported root cert to: $ExportedCert"
        $pfxFileName = -join($global:config.Parameters.DriverName, ".pfx")
        $PfxFilePath = Join-Path $WorkDir $pfxFileName         
        # $Test = [System.Environment]::GetEnvironmentVariable('TEMP','Machine')
        # Write-Host $pfxPath  #(Resolve-Path .\) .Path
        # Write-Host (Get-Location).Path  #$global:config.Parameters.CertSubject
        $RootCertStoreLocation = "Cert:\LocalMachine\Root"
        $securePass = ConvertTo-SecureString -String $global:config.Parameters.CertPassword -Force -AsPlainText
        Export-PfxCertificate -Cert $rootCert -FilePath $pfxFileName -Password $securePass | Out-Null
        # $ImportedRootCert = Import-Certificate -FilePath $ExportedCert.FullName -CertStoreLocation "Cert:\LocalMachine\Root"
        # Write-Output "Imported root cert to: $($RootCertStoreLocation)\$($ImportedRootCert.Thumbprint)"

        Write-Host "Certificate created and trusted" -ForegroundColor Green
        Set-Location -Path $ParentDirectory
        return $rootCert
    }
    catch{
        Set-Location -Path $ParentDirectory
        Handle-Error
    }
}
#endregion

#region 2. Enable Test Signing
function Enable-TestSigning {
    Write-Host "`n[2/4] Configuring test-signing mode..." -ForegroundColor Cyan
    $rebootRequired = $true 
    if (-not $isTestSigningEnabled) {
        bcdedit /set testsigning on | Out-Null
        Write-Host "Test-signing enabled. A reboot is required." -ForegroundColor Yellow
        $rebootRequired = $true
    }
    else {
        Write-Host "Test-signing is already enabled" -ForegroundColor Green
        $rebootRequired = $false
    }
    
    return $rebootRequired
}
#endregion

#region 3. Driver Signing
function Sign-Driver {
    # param (
    #     [string]$FilePath
    # )
    try{
    $ParentDirectory = (Get-Location).Path
    # Export as PFX
    $WorkDir = Join-Path (Get-Location).Path "DriverCatDebug"
    $pfxFileName = -join($global:config.Parameters.DriverName, ".pfx")
    $PfxFilePath = Join-Path $WorkDir $pfxFileName
    $CatFileName = -join($global:config.Parameters.DriverName, ".cat")
    $CatFilePath = Join-Path $WorkDir $CatFileName
    $CertPassword = $global:config.Parameters.CertPassword
    $TimeStampServer = $global:config.Parameters.TimeStampServer
    # Write-Host "Signing $($FilePath | Split-Path -Leaf)..." -ForegroundColor Cyan
        # Sign the catalog file  
    $signtool = $global:config.Parameters.Signtool
    & $signtool sign /a /debug /v /fd SHA256 /f $PfxFilePath /p $CertPassword /tr $TimeStampServer /td SHA256 /v $CatFilePath
    
    if ($LASTEXITCODE -ne 0) {
        throw "Signing failed for $FilePath"
    }
    }
    catch {
        Handle-Error
    }
    Write-Host "Successfully signed" -ForegroundColor Green
}
#endregion

#region 4. Driver Installation
function Install-Driver {
    Write-Host "`n[4/4] Installing driver..." -ForegroundColor Cyan

    Write-VerboseOutput "Starting driver installation process..."
    try {
        $ParentDirectory = (Get-Location).Path        
        $InfFileName = -join($global:config.Parameters.DriverName, ".inf")
        $DriverName = $global:config.Parameters.DriverName
        $DriverCatDebug = $global:config.Parameters.DriverCatDebug
        $InfPath = Join-Path $ParentDirectory $DriverCatDebug
        $InfFilePath = Join-Path $InfPath $InfFileName
        # Write-Host $InfFilePath
        Set-Location -Path $InfPath
        # Install driver package
        Write-VerboseOutput "Installing driver package using pnputil..."
        $pnpResult = pnputil.exe /add-driver $InfFileName /install 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "PNPUtil failed with exit code $LASTEXITCODE" -ForegroundColor Red
            Write-Host "PNPUtil output:`n$pnpResult" -ForegroundColor Yellow
            exit $LASTEXITCODE
        }
        Write-VerboseOutput "Driver package installed successfully"
        Set-Location -Path $ParentDirectory
        # Add printer driver
        Write-VerboseOutput "Adding printer driver to system..."
        $driverResult = Add-PrinterDriver -Name $DriverName -ErrorAction Stop 2>&1
        Write-VerboseOutput "Add-PrinterDriver output: $driverResult"

        # Add printer
        $PrinterName = $global:config.Parameters.PrinterName
        Write-VerboseOutput "Creating printer: $PrinterName"
        $printerResult = Add-Printer -Name $PrinterName -DriverName $DriverName -ErrorAction Stop 2>&1
        Write-VerboseOutput "Add-Printer output: $printerResult"
    }
    catch {
        Handle-Error "Failed during driver installation"
    }

}
#endregion

#region Generate Catalog file
function Generate-Catalog-File {
    try{
        Write-Host "Generate-Catalog-File" -ForegroundColor Green
        $ParentDirectory = (Get-Location).Path
        $DriverCatDebug = $global:config.Parameters.DriverCatDebug
        $CatalogFileName = -join($global:config.Parameters.DriverName, ".cat")
        $WorkDir = Join-Path $ParentDirectory $DriverCatDebug
        # Set-Location -Path $WorkDir
        $CatFilePath = Join-Path $WorkDir $CatalogFileName 
        # if(Test-Path $CatFilePath){
        #     Remove-Item $CatFilePath
        # }
        # else{    
        #     New-Item $CatFilePath -type file
        # }
        New-FileCatalog -Path $WorkDir -CatalogFilePath $CatalogFileName -CatalogVersion 2.0
        Write-Host "Test Catalog" -ForegroundColor Green
        Test-FileCatalog -CatalogFilePath $CatalogFileName -Path $WorkDir
        $CatFilePath.Close
        Set-Location -Path $ParentDirectory
    }
    catch{
        Handle-Error
    }
}

#endregion

#region Main Execution
try {
    # Check admin privileges
    if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Host "This script must be run as Administrator" -ForegroundColor Red
        exit 1
    }

    #region Cleanup Existing Printer
    Write-VerboseOutput "Starting printer cleanup..."
    # try {
    #     $Printer = Get-Printer -Name $PrinterName -ErrorAction SilentlyContinue
    #     if ($Printer) {
    #         Write-VerboseOutput "Found existing printer: $($Printer | Out-String)"
    #         Remove-Printer -Name $PrinterName -ErrorAction Stop
    #         Write-VerboseOutput "Printer removed successfully"
    #     }
        
    #     $Driver = Get-PrinterDriver -Name $DriverName -ErrorAction SilentlyContinue
    #     if ($Driver) {
    #         Write-VerboseOutput "Found existing driver: $($Driver | Out-String)"
    #         Remove-PrinterDriver -Name $DriverName -ErrorAction Stop
    #         Write-VerboseOutput "Driver removed successfully"
    #     }
    # }
    # catch {
    #     Handle-Error "Failed during printer/driver cleanup"
    # }
    #endregion
    
    $global:config = get-content '.\config\ps-config.json' | Out-String | convertfrom-json

    # Create Test Certificate
    $Certificate = New-TestCertificate
    #Generate Catalog File
    # $CatalogFile = Generate-Catalog-File
    Write-VerboseOutput "Catalog file created successfully"
    # # 2. Enable test signing
    # $needsReboot = Enable-TestSigning
    # 3. Sign all driver files
    Write-Host "`n[3/4] Signing driver files..." -ForegroundColor Cyan

    # $SignDriver = Sign-Driver 
    Write-Host "`n[3/4] Signing driver files..." $SignDriver -ForegroundColor Cyan  
    # 4. Install driver (if no reboot needed)
    if (-not $needsReboot) {
        Write-Host '>>>>>>' #Install-Driver
    }
    else {
        Write-Host "`nREBOOT REQUIRED to enable test-signing mode." -ForegroundColor Red
        Write-Host "After reboot, run just the installation part:" -ForegroundColor Yellow
        Write-Host "PS> Install-Driver" -ForegroundColor Yellow
    }

    # Write-VerboseOutput "Starting verification..."
    # try {
    #     Write-Host "`nFinal Verification Results:" -ForegroundColor Green
    #     Write-Host "-------------------------" -ForegroundColor Green

    #     # Printer verification
    #     $finalPrinter = Get-Printer -Name $PrinterName -ErrorAction Stop
    #     Write-Host "[SUCCESS] Printer installed:" -ForegroundColor Green
    #     $finalPrinter | Format-List Name, DriverName, PortName, Shared

    #     # Driver verification
    #     $finalDriver = Get-PrinterDriver -Name $DriverName -ErrorAction Stop
    #     Write-Host "[SUCCESS] Driver installed:" -ForegroundColor Green
    #     $finalDriver | Format-List Name, DriverPath, ConfigFile

    #     # Signature verification
    #     $finalSig = Get-AuthenticodeSignature -FilePath $CatFile
    #     Write-Host "[SUCCESS] Catalog file signature:" -ForegroundColor Green
    #     $finalSig | Format-List Status, StatusMessage, SignerCertificate

    #     # Certificate verification
    #     $finalCert = Get-ChildItem Cert:\LocalMachine\My\$($sert.Thumbprint) -ErrorAction Stop
    #     Write-Host "[SUCCESS] Certificate in store:" -ForegroundColor Green
    #     $finalCert | Format-List Subject, Thumbprint, NotAfter
    # }
    # catch {
    #     Handle-Error "Failed during verification"
    # }
    # #endregion

    # Write-Host "`n[COMPLETE] All operations completed successfully!" -ForegroundColor Green

}
catch {
    Write-Host "`nERROR: $_" -ForegroundColor Red
    Handle-Error
    exit 1
}
#endregion
