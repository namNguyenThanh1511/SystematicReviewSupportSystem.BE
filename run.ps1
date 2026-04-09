Push-Location "SRSS.IAM"

Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build

if ($?) {
    Write-Host "--- Starting Web API ---" -ForegroundColor Green
    try {
        dotnet run --project "SRSS.IAM.API/SRSS.IAM.API.csproj"
    }
    finally {
        Pop-Location
        Write-Host "--- Return root folder ---" -ForegroundColor Gray
    }
} else {
    Write-Host "--- Build failed ---" -ForegroundColor Red
    Pop-Location
}