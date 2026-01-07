# Script to identify connected COM devices
Write-Host "=== Detecting COM Devices ===" -ForegroundColor Cyan

# Get all COM port devices
$comDevices = Get-WmiObject Win32_PnPEntity | Where-Object {$_.Caption -like '*COM*'}

Write-Host "`nFound $($comDevices.Count) COM devices:" -ForegroundColor Yellow

foreach ($device in $comDevices) {
    Write-Host "`n----------------------------------------" -ForegroundColor Green
    Write-Host "Caption: $($device.Caption)" -ForegroundColor White
    Write-Host "DeviceID: $($device.DeviceID)" -ForegroundColor Gray
    Write-Host "PNPDeviceID: $($device.PNPDeviceID)" -ForegroundColor Gray
    
    # Extract COM port number if present
    if ($device.Caption -match 'COM(\d+)') {
        Write-Host "COM Port: COM$($matches[1])" -ForegroundColor Cyan
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Please identify which device is the PAX SP30 terminal" -ForegroundColor Yellow
