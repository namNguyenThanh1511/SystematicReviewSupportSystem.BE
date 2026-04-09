param (
    [string]$action,
    [string]$name
)

$REPO = "SRSS.IAM/SRSS.IAM.Repositories/SRSS.IAM.Repositories.csproj"
$API = "SRSS.IAM/SRSS.IAM.API/SRSS.IAM.API.csproj"

switch ($action) {
    "add" {
        if (-not $name) { Write-Host "Missing migration name !" -ForegroundColor Red }
        else { dotnet ef migrations add $name -p $REPO -s $API }
    }
    "update" {
        dotnet ef database update -p $REPO -s $API
    }
    "drop" {
        dotnet ef database drop -p $REPO -s $API
    }
    default {
        Write-Host "Add : .\ef-helper.ps1 -action add -name MigrationName"
        Write-Host "Update Database : .\ef-helper.ps1 -action update"
        Write-Host "Drop Database : .\ef-helper.ps1 -action drop"
    }
}