FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/ReviewService/TrustMarket.ReviewService.Api/TrustMarket.ReviewService.Api.csproj", "src/Services/ReviewService/TrustMarket.ReviewService.Api/"]
COPY ["src/Services/ReviewService/TrustMarket.ReviewService.Application/TrustMarket.ReviewService.Application.csproj", "src/Services/ReviewService/TrustMarket.ReviewService.Application/"]
COPY ["src/Services/ReviewService/TrustMarket.ReviewService.Domain/TrustMarket.ReviewService.Domain.csproj", "src/Services/ReviewService/TrustMarket.ReviewService.Domain/"]
COPY ["src/Services/ReviewService/TrustMarket.ReviewService.Infrastructure/TrustMarket.ReviewService.Infrastructure.csproj", "src/Services/ReviewService/TrustMarket.ReviewService.Infrastructure/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Common/TrustMarket.Shared.Common.csproj", "src/BuildingBlocks/TrustMarket.Shared.Common/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Contracts/TrustMarket.Shared.Contracts.csproj", "src/BuildingBlocks/TrustMarket.Shared.Contracts/"]
RUN dotnet restore "src/Services/ReviewService/TrustMarket.ReviewService.Api/TrustMarket.ReviewService.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/ReviewService/TrustMarket.ReviewService.Api"
RUN dotnet build "TrustMarket.ReviewService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrustMarket.ReviewService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TrustMarket.ReviewService.Api.dll"]
