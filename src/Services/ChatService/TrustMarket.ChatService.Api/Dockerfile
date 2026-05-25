FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/ChatService/TrustMarket.ChatService.Api/TrustMarket.ChatService.Api.csproj", "src/Services/ChatService/TrustMarket.ChatService.Api/"]
COPY ["src/Services/ChatService/TrustMarket.ChatService.Application/TrustMarket.ChatService.Application.csproj", "src/Services/ChatService/TrustMarket.ChatService.Application/"]
COPY ["src/Services/ChatService/TrustMarket.ChatService.Domain/TrustMarket.ChatService.Domain.csproj", "src/Services/ChatService/TrustMarket.ChatService.Domain/"]
COPY ["src/Services/ChatService/TrustMarket.ChatService.Infrastructure/TrustMarket.ChatService.Infrastructure.csproj", "src/Services/ChatService/TrustMarket.ChatService.Infrastructure/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Common/TrustMarket.Shared.Common.csproj", "src/BuildingBlocks/TrustMarket.Shared.Common/"]
COPY ["src/BuildingBlocks/TrustMarket.Shared.Contracts/TrustMarket.Shared.Contracts.csproj", "src/BuildingBlocks/TrustMarket.Shared.Contracts/"]
RUN dotnet restore "src/Services/ChatService/TrustMarket.ChatService.Api/TrustMarket.ChatService.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/ChatService/TrustMarket.ChatService.Api"
RUN dotnet build "TrustMarket.ChatService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrustMarket.ChatService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TrustMarket.ChatService.Api.dll"]
