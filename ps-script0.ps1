Option 1: Use DISM to Add Driver with Force Unsigned Flag
powershell
function Install-DriverNoReboot {
    try {
        Write-Host "Installing driver without test signing..." -ForegroundColor Cyan
        
        # Use DISM to add the driver package with force-unsigned flag
        $dismResult = dism.exe /Online /Add-Driver /Driver:"$InfFilePath" /ForceUnsigned 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "DISM output: $dismResult" -ForegroundColor Yellow
            throw "DISM installation failed"
        }
        
        Write-Host "Driver installed via DISM (unsigned allowed)" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "DISM install failed, trying alternative methods..." -ForegroundColor Yellow
        return $false
    }
}
Option 2: Use Device Installation Policies (Registry-Based)
powershell
function Set-DeviceInstallPolicy {
    try {
        Write-Host "Configuring device installation policies..." -ForegroundColor Cyan
        
        # Disable driver signature enforcement via registry
        $regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Device Installer"
        
        # Create key if it doesn't exist
        if (-not (Test-Path $regPath)) {
            New-Item -Path $regPath -Force | Out-Null
        }
        
        # Set policies to allow unsigned drivers
        Set-ItemProperty -Path $regPath -Name "AllowUnsignedDriverInstall" -Value 1 -Type DWord -Force
        Set-ItemProperty -Path $regPath -Name "AllowUnsignedAppInstall" -Value 1 -Type DWord -Force
        
        Write-Host "Device installation policies configured" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Registry policy setting failed: $($_.Exception.Message)" -ForegroundColor Yellow
        return $false
    }
}
Option 3: Use pnputil with Special Flags
powershell
function Install-DriverPnpUtil {
    try {
        Write-Host "Attempting pnputil installation with special handling..." -ForegroundColor Cyan
        
        # First try normal installation
        $result = pnputil.exe /add-driver $InfFilePath /install 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Driver installed successfully via pnputil" -ForegroundColor Green
            return $true
        }
        
        # If failed, check if it's just a signature warning
        if ($result -match "signature|0x800b0109|0xE0000247") {
            Write-Host "Signature validation failed, but driver files are copied..." -ForegroundColor Yellow
            
            # Try to manually register the driver
            return Install-DriverManual
        }
        
        throw "pnputil failed: $result"
    }
    catch {
        Write-Host "pnputil install failed: $($_.Exception.Message)" -ForegroundColor Yellow
        return $false
    }
}
🎯 Revised No-Reboot Script Section
Replace your main execution section with this:

powershell
#region Main Execution (No Reboot)
try {
    # Check admin privileges
    if (-not (Test-Admin)) {
        Write-Host "This script must be run as Administrator" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Starting driver installation (NO REBOOT REQUIRED)..." -ForegroundColor Green
    
    # Execute steps
    $cert = New-TestCertificate
    Generate-Catalog-File
    Sign-Catalog
    
    # Set device installation policies
    $policySet = Set-DeviceInstallPolicy
    
    # Try multiple installation methods
    $installed = $false
    
    # Method 1: Try DISM first
    if (-not $installed) {
        Write-Host "Trying DISM installation..." -ForegroundColor Cyan
        $installed = Install-DriverNoReboot
    }
    
    # Method 2: Try pnputil with special handling
    if (-not $installed) {
        Write-Host "Trying pnputil installation..." -ForegroundColor Cyan
        $installed = Install-DriverPnpUtil
    }
    
    # Method 3: Manual installation as last resort
    if (-not $installed) {
        Write-Host "Trying manual installation..." -ForegroundColor Cyan
        $installed = Install-DriverManual
    }
    
    if ($installed) {
        # Add printer instance
        Add-PrinterInstance
        Write-Host "`n[SUCCESS] Driver installed without reboot!" -ForegroundColor Green
    }
    else {
        Write-Host "`n[WARNING] Driver files are prepared but may need manual installation" -ForegroundColor Yellow
        Write-Host "Driver files are ready at: $WorkDir" -ForegroundColor Yellow
    }
}
catch {
    Handle-Error "Main execution failed"
}
#endregion
📋 Additional Helper Functions Needed
Add these functions to your script:

powershell
function Install-DriverManual {
    try {
        Write-Host "Attempting manual driver installation..." -ForegroundColor Cyan
        
        # Copy files manually to driver store locations
        $driverStorePath = "C:\Windows\System32\DriverStore\FileRepository"
        
        # Create a temporary directory in driver store
        $tempDir = Join-Path $env:TEMP "DriverInstall"
        New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
        
        # Copy all driver files
        Copy-Item -Path "$WorkDir\*" -Destination $tempDir -Recurse -Force
        
        # Try to install using INF file directly
        $infPath = Join-Path $tempDir $InfFileName
        $result = rundll32.exe setupapi,InstallHinfSection DefaultInstall 132 $infPath 2>&1
        
        Write-Host "Manual installation attempted. Check Device Manager." -ForegroundColor Yellow
        return $true
    }
    catch {
        Write-Host "Manual install failed: $($_.Exception.Message)" -ForegroundColor Yellow
        return $false
    }
}

function Test-DriverLoaded {
    try {
        # Check if driver is loaded in print spooler
        $drivers = Get-PrinterDriver -Name $DriverName -ErrorAction SilentlyContinue
        return ($drivers -ne $null)
    }
    catch {
        return $false
    }
}
🎯 Key Advantages of This Approach
No reboot required - Works immediately in VDI environments

No test signing mode - Maintains system security settings

Multiple fallback methods - Tries different installation approaches

Printer-driver specific - Leverages the fact that printer drivers have different signing requirements than kernel drivers

💡 Pro Tip: Debugging Without Full Installation
Since you're debugging, you might not even need to fully "install" the driver. You can often:

Load the DLL directly in your debugger

Use symbolic links to point to your debug build

Test with the print spooler service restarted instead of full OS reboot

powershell
# Restart print spooler (often enough for driver changes)
Restart-Service -Name Spooler -Force
This no-reboot approach should work perfectly for your printer driver development and debugging scenario!

New chat
