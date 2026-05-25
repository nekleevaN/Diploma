FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/CatalogService/TrustMarket.CatalogService.Api/TrustMarket.CatalogService.Api.csproj", "src/Services/CatalogService/TrustMarket.CatalogService.Api/"]
COPY ["src/Services/CatalogService/TrustMarket.CatalogService.Application/TrustMarket.CatalogService.Application.csproj", "src/Services/CatalogService/TrustMarket.CatalogService.Application/"]
COPY ["src/Services/CatalogService/TrustMarket.CatalogService.Domain/TrustMarket.CatalogService.Domain.csproj", "src/Services/CatalogService/TrustMarket.CatalogService.Domain/"]
COPY ["src/Services/CatalogService/TrustMarket.CatalogService.Infrastructure/TrustMarket.CatalogService.Infrastructure.csproj", "src/Services/CatalogService/TrustMarket.CatalogService.Infrastructure/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Common/TrustMarket.Shared.Common.csproj", "src/BuildingBlocks/TrustMarket.Shared.Common/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Contracts/TrustMarket.Shared.Contracts.csproj", "src/BuildingBlocks/TrustMarket.Shared.Contracts/"]
RUN dotnet restore "src/Services/CatalogService/TrustMarket.CatalogService.Api/TrustMarket.CatalogService.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/CatalogService/TrustMarket.CatalogService.Api"
RUN dotnet build "TrustMarket.CatalogService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrustMarket.CatalogService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TrustMarket.CatalogService.Api.dll"]
