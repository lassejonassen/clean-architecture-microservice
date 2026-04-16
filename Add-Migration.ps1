# Add-Migration.ps1
# Usage: .\Add-Migration.ps1 -Name "InitialCreate"
# 1. Define the Migration Name (Defaults to "NewMigration" if not provided)
param (
    [Parameter(Mandatory=$false)]
    [string]$Name = "NewMigration"
)

Write-Host "🚀 Starting migration: $Name" -ForegroundColor Cyan

# 2. Set the variables for easy maintenance
$InfrastructureProject = "./src/CleanArchitecture.Infrastructure"
$OutputDir = "./Persistence/Migrations"

# 3. Execute the dotnet ef command
dotnet ef migrations add $Name `
    --project $InfrastructureProject `
    --output-dir $OutputDir

# 4. Check if the command succeeded
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Migration '$Name' created successfully in $OutputDir" -ForegroundColor Green
} else {
    Write-Host "❌ Error: Migration failed. Check the logs above." -ForegroundColor Red
}