FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/UserService/TrustMarket.UserService.Api/TrustMarket.UserService.Api.csproj", "src/Services/UserService/TrustMarket.UserService.Api/"]
COPY ["src/Services/UserService/TrustMarket.UserService.Application/TrustMarket.UserService.Application.csproj", "src/Services/UserService/TrustMarket.UserService.Application/"]
COPY ["src/Services/UserService/TrustMarket.UserService.Domain/TrustMarket.UserService.Domain.csproj", "src/Services/UserService/TrustMarket.UserService.Domain/"]
COPY ["src/Services/UserService/TrustMarket.UserService.Infrastructure/TrustMarket.UserService.Infrastructure.csproj", "src/Services/UserService/TrustMarket.UserService.Infrastructure/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Common/TrustMarket.Shared.Common.csproj", "src/BuildingBlocks/TrustMarket.Shared.Common/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Contracts/TrustMarket.Shared.Contracts.csproj", "src/BuildingBlocks/TrustMarket.Shared.Contracts/"]
RUN dotnet restore "src/Services/UserService/TrustMarket.UserService.Api/TrustMarket.UserService.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/UserService/TrustMarket.UserService.Api"
RUN dotnet build "TrustMarket.UserService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrustMarket.UserService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TrustMarket.UserService.Api.dll"]
