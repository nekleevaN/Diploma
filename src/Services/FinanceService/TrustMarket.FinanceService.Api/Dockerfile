FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/FinanceService/TrustMarket.FinanceService.Api/TrustMarket.FinanceService.Api.csproj", "src/Services/FinanceService/TrustMarket.FinanceService.Api/"]
COPY ["src/Services/FinanceService/TrustMarket.FinanceService.Application/TrustMarket.FinanceService.Application.csproj", "src/Services/FinanceService/TrustMarket.FinanceService.Application/"]
COPY ["src/Services/FinanceService/TrustMarket.FinanceService.Domain/TrustMarket.FinanceService.Domain.csproj", "src/Services/FinanceService/TrustMarket.FinanceService.Domain/"]
COPY ["src/Services/FinanceService/TrustMarket.FinanceService.Infrastructure/TrustMarket.FinanceService.Infrastructure.csproj", "src/Services/FinanceService/TrustMarket.FinanceService.Infrastructure/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Common/TrustMarket.Shared.Common.csproj", "src/BuildingBlocks/TrustMarket.Shared.Common/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Contracts/TrustMarket.Shared.Contracts.csproj", "src/BuildingBlocks/TrustMarket.Shared.Contracts/"]
RUN dotnet restore "src/Services/FinanceService/TrustMarket.FinanceService.Api/TrustMarket.FinanceService.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/FinanceService/TrustMarket.FinanceService.Api"
RUN dotnet build "TrustMarket.FinanceService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrustMarket.FinanceService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TrustMarket.FinanceService.Api.dll"]
