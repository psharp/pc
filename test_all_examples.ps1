# Test all example Pascal programs
$examples = Get-ChildItem examples\*.pas
$passed = 0
$failed = 0
$failedFiles = @()

foreach ($file in $examples) {
    Write-Host "Testing: $($file.Name)" -ForegroundColor Cyan

    $output = & dotnet run --project PascalCompiler.csproj $file.FullName 2>&1 | Out-String

    if ($output -match "Error:") {
        Write-Host "  FAILED" -ForegroundColor Red
        $failed++
        $failedFiles += $file.Name
        # Show the error
        $output -split "`n" | Select-String "Error:" | Select-Object -First 3 | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
    }
    elseif ($output -match "completed|No semantic errors") {
        Write-Host "  PASSED" -ForegroundColor Green
        $passed++
    }
    else {
        Write-Host "  UNKNOWN" -ForegroundColor Yellow
    }
}

Write-Host "`n========================================" -ForegroundColor White
Write-Host "Test Results:" -ForegroundColor White
Write-Host "  Passed: $passed" -ForegroundColor Green
Write-Host "  Failed: $failed" -ForegroundColor Red

if ($failed -gt 0) {
    Write-Host "`nFailed files:" -ForegroundColor Red
    $failedFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
}
