<#
.SYNOPSIS
    Removes and reinstalls printer driver for debugging purposes
.DESCRIPTION
    This script handles the complete removal of printer driver components
    and installs updated DLL and PDB files for debugging
.NOTES
    Run as Administrator
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$DriverName,
    
    [Parameter(Mandatory=$true)]
    [string]$DriverPath,
    
    [switch]$Verbose = $false
)

#region Helper Functions
function Write-VerboseOutput {
    param([string]$Message)
    if ($Verbose) {
        Write-Host "[VERBOSE] $(Get-Date -Format 'HH:mm:ss') - $Message" -ForegroundColor DarkGray
    }
}

function Stop-PrintSpooler {
    Write-Host "Stopping Print Spooler service..." -ForegroundColor Cyan
    try {
        Stop-Service -Name Spooler -Force -ErrorAction Stop
        Write-Host "Print Spooler stopped successfully" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Failed to stop Print Spooler: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Start-PrintSpooler {
    Write-Host "Starting Print Spooler service..." -ForegroundColor Cyan
    try {
        Start-Service -Name Spooler -ErrorAction Stop
        Write-Host "Print Spooler started successfully" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Failed to start Print Spooler: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Remove-PrinterAndDriver {
    param([string]$PrinterName, [string]$DriverName)
    
    Write-Host "Removing printer and driver..." -ForegroundColor Cyan
    
    try {
        # Remove printer if it exists
        $printer = Get-Printer -Name $PrinterName -ErrorAction SilentlyContinue
        if ($printer) {
            Write-VerboseOutput "Removing printer: $PrinterName"
            Remove-Printer -Name $PrinterName -ErrorAction Stop
            Write-Host "Printer removed: $PrinterName" -ForegroundColor Green
        }
        
        # Remove printer driver if it exists
        $driver = Get-PrinterDriver -Name $DriverName -ErrorAction SilentlyContinue
        if ($driver) {
            Write-VerboseOutput "Removing printer driver: $DriverName"
            Remove-PrinterDriver -Name $DriverName -ErrorAction Stop
            Write-Host "Printer driver removed: $DriverName" -ForegroundColor Green
        }
        
        return $true
    }
    catch {
        Write-Host "Failed to remove printer/driver: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Remove-DriverFromDriverStore {
    param([string]$InfPath)
    
    Write-Host "Removing driver from DriverStore..." -ForegroundColor Cyan
    
    try {
        # Get the driver package name from the INF file
        $infContent = Get-Content $InfPath
        $className = ($infContent | Where-Object { $_ -match '^Class\s*=' }) -replace '^Class\s*=\s*', ''
        $provider = ($infContent | Where-Object { $_ -match '^Provider\s*=' }) -replace '^Provider\s*=\s*', ''
        
        if (-not $className -or -not $provider) {
            throw "Could not extract Class or Provider from INF file"
        }
        
        # Find the driver package in DriverStore
        $driverPackages = pnputil.exe /enum-drivers | Where-Object { $_ -match "$provider.*$className" }
        
        if ($driverPackages) {
            foreach ($package in $driverPackages) {
                if ($package -match 'Published Name : (oem\d+\.inf)') {
                    $oemFile = $matches[1]
                    Write-VerboseOutput "Removing driver package: $oemFile"
                    pnputil.exe /delete-driver $oemFile /force | Out-Null
                    
                    if ($LASTEXITCODE -eq 0) {
                        Write-Host "Driver package removed: $oemFile" -ForegroundColor Green
                    } else {
                        Write-Host "Failed to remove driver package: $oemFile" -ForegroundColor Yellow
                    }
                }
            }
        }
        
        return $true
    }
    catch {
        Write-Host "Failed to remove driver from DriverStore: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Copy-DebugFiles {
    param([string]$SourcePath, [string]$DriverName)
    
    Write-Host "Copying debug files to DriverStore..." -ForegroundColor Cyan
    
    try {
        # Find the driver store path for our driver
        $driverStorePath = "C:\Windows\System32\DriverStore\FileRepository"
        $driverDir = Get-ChildItem $driverStorePath -Recurse -Filter "$DriverName*" -Directory | Select-Object -First 1
        
        if (-not $driverDir) {
            throw "Could not find driver directory in DriverStore"
        }
        
        Write-VerboseOutput "Found driver directory: $($driverDir.FullName)"
        
        # Copy DLL and PDB files
        $dllSource = Join-Path $SourcePath "$DriverName.dll"
        $pdbSource = Join-Path $SourcePath "$DriverName.pdb"
        
        if (Test-Path $dllSource) {
            Copy-Item -Path $dllSource -Destination $driverDir.FullName -Force
            Write-Host "DLL file updated: $DriverName.dll" -ForegroundColor Green
        }
        
        if (Test-Path $pdbSource) {
            Copy-Item -Path $pdbSource -Destination $driverDir.FullName -Force
            Write-Host "PDB file updated: $DriverName.pdb" -ForegroundColor Green
        }
        
        return $true
    }
    catch {
        Write-Host "Failed to copy debug files: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Install-Driver {
    param([string]$InfPath, [string]$DriverName)
    
    Write-Host "Installing driver..." -ForegroundColor Cyan
    
    try {
        # Install the driver
        $result = pnputil.exe /add-driver $InfPath /install 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Driver installed successfully" -ForegroundColor Green
            return $true
        } else {
            throw "pnputil failed: $result"
        }
    }
    catch {
        Write-Host "Failed to install driver: $($_.Exception.Message)" -ForegroundColor Red
        return $false
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
    
    Write-Host "Starting driver update process for: $DriverName" -ForegroundColor Green
    
    # Step 1: Stop print spooler
    if (-not (Stop-PrintSpooler)) {
        Write-Host "Warning: Continuing without stopping print spooler" -ForegroundColor Yellow
    }
    
    # Step 2: Remove printer and driver
    $printerName = "Debug_$DriverName"
    if (-not (Remove-PrinterAndDriver -PrinterName $printerName -DriverName $DriverName)) {
        Write-Host "Warning: Could not remove all printer components" -ForegroundColor Yellow
    }
    
    # Step 3: Remove driver from DriverStore
    $infPath = Join-Path $DriverPath "$DriverName.inf"
    if (-not (Remove-DriverFromDriverStore -InfPath $infPath)) {
        Write-Host "Warning: Could not remove driver from DriverStore" -ForegroundColor Yellow
    }
    
    # Step 4: Copy debug files
    if (-not (Copy-DebugFiles -SourcePath $DriverPath -DriverName $DriverName)) {
        Write-Host "Error: Could not copy debug files" -ForegroundColor Red
        exit 1
    }
    
    # Step 5: Install driver
    if (-not (Install-Driver -InfPath $infPath -DriverName $DriverName)) {
        Write-Host "Error: Could not install driver" -ForegroundColor Red
        exit 1
    }
    
    # Step 6: Start print spooler
    if (-not (Start-PrintSpooler)) {
        Write-Host "Warning: Could not start print spooler" -ForegroundColor Yellow
    }
    
    Write-Host "`nDriver update completed successfully!" -ForegroundColor Green
    Write-Host "You can now attach your debugger to the print spooler process." -ForegroundColor Cyan
}
catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Line: $($_.InvocationInfo.ScriptLineNumber)" -ForegroundColor Red
    Write-Host "Script: $($_.InvocationInfo.ScriptName)" -ForegroundColor Red
    
    # Try to restart print spooler on error
    Start-PrintSpooler | Out-Null
    
    exit 1
}
#endregion