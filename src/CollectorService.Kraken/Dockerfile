#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CollectorService.Kraken/CollectorService.Kraken.csproj", "CollectorService.Kraken/"]
RUN dotnet restore "CollectorService.Kraken/CollectorService.Kraken.csproj"
COPY . .
WORKDIR "/src/CollectorService.Kraken"
RUN dotnet build "CollectorService.Kraken.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CollectorService.Kraken.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CollectorService.Kraken.dll"]