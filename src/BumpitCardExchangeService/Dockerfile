#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

ENV REDIS_HOST="localhost"
ENV REDIS_PORT="6379"

ENV REDIS_GEORADIUS="5"
ENV REDIS_KEY_EXPIRE_TIMEOUT="2"

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["BumpitCardExchangeService/BumpitCardExchangeService.csproj", "BumpitCardExchangeService/"]
RUN dotnet restore "BumpitCardExchangeService/BumpitCardExchangeService.csproj"
COPY . .
WORKDIR "/src/BumpitCardExchangeService"
RUN dotnet build "BumpitCardExchangeService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BumpitCardExchangeService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BumpitCardExchangeService.dll"]