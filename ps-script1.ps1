function Add-PrinterInstance {
    try {
        Write-Host "`n[6/6] Adding printer instance..." -ForegroundColor Cyan
        
        # First, verify the driver is actually installed
        $driverCheck = Get-PrinterDriver -Name $DriverName -ErrorAction SilentlyContinue
        if (-not $driverCheck) {
            Write-Host "Warning: Printer driver not found. Attempting to re-register..." -ForegroundColor Yellow
            
            # Try to register the driver using rundll32
            $infPath = Join-Path $WorkDir $InfFileName
            $result = rundll32.exe setupapi,InstallHinfSection DefaultInstall 132 $infPath 2>&1
            Write-Host "Driver registration result: $result" -ForegroundColor Gray
        }
        
        # Check if printer port exists, create if needed
        $portName = "ULP2_DebugPort"
        $portCheck = Get-PrinterPort -Name $portName -ErrorAction SilentlyContinue
        if (-not $portCheck) {
            Write-Host "Creating printer port: $portName" -ForegroundColor Cyan
            Add-PrinterPort -Name $portName -DriverName "winprint" -PortType "Local" -ErrorAction Stop
        }
        
        # Check if printer already exists
        $printerCheck = Get-Printer -Name $PrinterName -ErrorAction SilentlyContinue
        if ($printerCheck) {
            Write-Host "Removing existing printer: $PrinterName" -ForegroundColor Yellow
            Remove-Printer -Name $PrinterName -ErrorAction Stop
        }
        
        # Add printer with specific parameters
        $printerParams = @{
            Name = $PrinterName
            DriverName = $DriverName
            PortName = $portName
            Shared = $false
            Published = $false
            ErrorAction = 'Stop'
        }
        
        Add-Printer @printerParams
        Write-Host "Printer '$PrinterName' added successfully" -ForegroundColor Green
        
        # Verify printer was created
        $finalPrinter = Get-Printer -Name $PrinterName -ErrorAction Stop
        Write-Host "Printer details:" -ForegroundColor Gray
        $finalPrinter | Format-List Name, DriverName, PortName, Shared, Published
    }
    catch {
        Write-Host "Warning: Printer creation failed (driver may still be installed): $($_.Exception.Message)" -ForegroundColor Yellow
        
        # Provide manual instructions as fallback
        Write-Host "`nYou can manually add the printer using:" -ForegroundColor Cyan
        Write-Host "1. Open 'Devices and Printers' in Control Panel" -ForegroundColor White
        Write-Host "2. Click 'Add a printer'" -ForegroundColor White
        Write-Host "3. Select 'The printer that I want isn't listed'" -ForegroundColor White
        Write-Host "4. Choose 'Add a local printer or network printer with manual settings'" -ForegroundColor White
        Write-Host "5. Select an existing port or create a new one" -ForegroundColor White
        Write-Host "6. Click 'Have Disk' and browse to: $WorkDir" -ForegroundColor White
        Write-Host "7. Select the INF file and complete installation" -ForegroundColor White
    }
}