FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/DeFinance.Domain/DeFinance.Domain.csproj             src/DeFinance.Domain/
COPY src/DeFinance.Application/DeFinance.Application.csproj   src/DeFinance.Application/
COPY src/DeFinance.Infrastructure/DeFinance.Infrastructure.csproj src/DeFinance.Infrastructure/
COPY src/DeFinance.Api/DeFinance.Api.csproj                   src/DeFinance.Api/
RUN dotnet restore src/DeFinance.Api/DeFinance.Api.csproj

COPY src/ src/
RUN dotnet publish src/DeFinance.Api/DeFinance.Api.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DeFinance.Api.dll"]
