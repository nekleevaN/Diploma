FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/NotificationService/TrustMarket.NotificationService.Api/TrustMarket.NotificationService.Api.csproj", "src/Services/NotificationService/TrustMarket.NotificationService.Api/"]
COPY ["src/Services/NotificationService/TrustMarket.NotificationService.Application/TrustMarket.NotificationService.Application.csproj", "src/Services/NotificationService/TrustMarket.NotificationService.Application/"]
COPY ["src/Services/NotificationService/TrustMarket.NotificationService.Infrastructure/TrustMarket.NotificationService.Infrastructure.csproj", "src/Services/NotificationService/TrustMarket.NotificationService.Infrastructure/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Contracts/TrustMarket.Shared.Contracts.csproj", "src/BuildingBlocks/TrustMarket.Shared.Contracts/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Common/TrustMarket.Shared.Common.csproj", "src/BuildingBlocks/TrustMarket.Shared.Common/"]
RUN dotnet restore "src/Services/NotificationService/TrustMarket.NotificationService.Api/TrustMarket.NotificationService.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/NotificationService/TrustMarket.NotificationService.Api"
RUN dotnet build "TrustMarket.NotificationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrustMarket.NotificationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TrustMarket.NotificationService.Api.dll"]
