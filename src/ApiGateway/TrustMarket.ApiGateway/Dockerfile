FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ApiGateway/TrustMarket.ApiGateway/TrustMarket.ApiGateway.csproj", "src/ApiGateway/TrustMarket.ApiGateway/"]
RUN dotnet restore "src/ApiGateway/TrustMarket.ApiGateway/TrustMarket.ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/ApiGateway/TrustMarket.ApiGateway"
RUN dotnet publish "TrustMarket.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TrustMarket.ApiGateway.dll"]
