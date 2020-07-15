# PS C:\Projekte\Cloud\SMEF\smef-logging-service\src>  docker build -t logging-service -f .\Dockerfile . --build-arg NUGET_USERNAME=xxx --build-arg NUGET_PASSWORD=xxx
ARG DOTNETCORE_VERSION=3.1
FROM mcr.microsoft.com/dotnet/core/sdk:${DOTNETCORE_VERSION} AS build-env
WORKDIR /app

ARG NUGET_USERNAME
ARG NUGET_PASSWORD

# Copy everything else and build
COPY BumpitCardExchangeService/ ./BumpitCardExchangeService/

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
#RUN dotnet sonarscanner begin /k:"abatplus_bumpit-card-provider" \
#  /o:"abatplus" \
#  /d:sonar.sources=BumpitCardProvider \
#  /d:sonar.branch.name=$BRANCH \
#  /d:sonar.host.url=https://sonarcloud.io \
#  /d:sonar.login=${SONAR_TOKEN} \
#  /d:sonar.cs.analyzer.projectOutPaths=out

RUN dotnet publish -c Release -o out ./BumpitCardExchangeService

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
ENTRYPOINT ["dotnet", "BumpitCardExchangeService.dll"]