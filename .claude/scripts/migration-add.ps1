param(
    [Parameter(Mandatory)][string]$Name
)

dotnet ef migrations add $Name `
    --project src/DeFinance.Infrastructure `
    --startup-project src/DeFinance.Api `
    --output-dir Persistence/Migrations
