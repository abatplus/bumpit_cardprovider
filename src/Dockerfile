# docker build -t abatplus/vswap_exchange_service -f Dockerfile --rm --build-arg NUGET_USERNAME=reader --build-arg NUGET_PASSWORD=abat+2010-reader --build-arg BRANCH=master --build-arg SONAR_TOKEN=482f0ce7684730ce84999f4c6eb32bfd0457dcbd --build-arg PORT_TCP_API=5000 .
ARG DOTNETCORE_VERSION=3.1
FROM mcr.microsoft.com/dotnet/core/sdk:${DOTNETCORE_VERSION} AS build-env
WORKDIR /app

ARG NUGET_USERNAME
ARG NUGET_PASSWORD

ENV REDIS_HOST="localhost"
ENV REDIS_PORT="6379"

ENV REDIS_GEORADIUS="25"
ENV REDIS_KEY_EXPIRE_TIMEOUT="5"

# Copy everything else and build
COPY CardExchangeService/ ./CardExchangeService/

# Set nuget.config
COPY nuget.config.template ./nuget.config
RUN sed -i -e "s/REPLACE_USERNAME/$NUGET_USERNAME/g;s/REPLACE_PASSWORD/$NUGET_PASSWORD/g" ./nuget.config

RUN dotnet publish -c Release -o out ./CardExchangeService


# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:${DOTNETCORE_VERSION}

RUN apt-get update && apt-get install -y libgdiplus

ARG PORT_TCP_API

EXPOSE ${PORT_TCP_API}/tcp
WORKDIR /app
COPY --from=build-env /app/out .

# RUN dotnet ef database update
ENTRYPOINT ["dotnet", "CardExchangeService.dll"]