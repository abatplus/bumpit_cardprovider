# docker build -t abatplus/vswap_exchange_service -f Dockerfile --rm --build-arg NUGET_USERNAME=reader --build-arg NUGET_PASSWORD=abat+2010-reader --build-arg BRANCH=master --build-arg SONAR_TOKEN=482f0ce7684730ce84999f4c6eb32bfd0457dcbd --build-arg PORT_TCP_API=5000 .
ARG DOTNETCORE_VERSION=3.1
FROM mcr.microsoft.com/dotnet/core/sdk:${DOTNETCORE_VERSION} AS build-env
WORKDIR /app

ARG NUGET_USERNAME
ARG NUGET_PASSWORD

ENV REDIS_HOST="localhost"
ENV REDIS_PORT="6379"

ENV REDIS_GEORADIUS="5"
ENV REDIS_KEY_EXPIRE_TIMEOUT="2"

# Copy everything else and build
COPY CardExchangeService/ ./CardExchangeService/

# Set nuget.config
COPY nuget.config.template ./nuget.config
RUN sed -i -e "s/REPLACE_USERNAME/$NUGET_USERNAME/g;s/REPLACE_PASSWORD/$NUGET_PASSWORD/g" ./nuget.config

# sonarqube call args
#ARG BRANCH
#ARG SONAR_TOKEN

# install sonarscanner
#RUN dotnet tool install --global dotnet-sonarscanner
#ENV PATH="${PATH}:/root/.dotnet/tools"

# start sonarscanner
#RUN dotnet sonarscanner begin /k:"abatplus_vswap_exchange_service" \
#  /o:"abatplus" \
#  /d:sonar.sources=BumpitCardProvider \
#  /d:sonar.branch.name=$BRANCH \
#  /d:sonar.host.url=https://sonarcloud.io \
#  /d:sonar.login=${SONAR_TOKEN} \
#  /d:sonar.cs.analyzer.projectOutPaths=out

RUN dotnet publish -c Release -o out ./CardExchangeService

# install Java for sonarscanner end
#RUN apt-get update && \
# apt-get install -y default-jdk && \
#  apt-get clean;

# end sonarscanner
#RUN dotnet sonarscanner end \
#  /d:sonar.login=${SONAR_TOKEN}


# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:${DOTNETCORE_VERSION}

ARG PORT_TCP_API

EXPOSE ${PORT_TCP_API}/tcp
WORKDIR /app
COPY --from=build-env /app/out .

# RUN dotnet ef database update
ENTRYPOINT ["dotnet", "CardExchangeService.dll"]