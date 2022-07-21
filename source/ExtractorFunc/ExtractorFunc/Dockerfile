FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG PROJECT_PATH=FunctionApps/ExtractorFunc/
ARG PROJECT=ExtractorFunc.csproj

# Install Credential Provider
RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh  | sh
ARG PAT

# Optional: Sometimes the http client hangs because of a .NET issue.
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
# Environment variable to enable session token cache.
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true

# Environment variable for adding endpoint credentials. More on this here: https://github.com/Microsoft/artifacts-credprovider#help
# Add "FEED_URL" AND "PAT" using --build-arg in docker build step. "endpointCredentials" field is an array, you can add multiple endpoint configurations.
# Make sure that you *do not* hard code the "PAT" here. That is a sensitive information and must not be checked in.
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS {\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/afidev/_packaging/afi-packages/nuget/v3/index.json\", \"username\":\"ArtifactsDocker\", \"password\":\"${PAT}\"}]}


COPY ["nuget.config", "."]
COPY ${PROJECT_PATH}${PROJECT} ${PROJECT_PATH}

RUN dotnet restore "$PROJECT_PATH$PROJECT"

COPY . .
WORKDIR $PROJECT_PATH
RUN dotnet publish "$PROJECT" --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:4-appservice
FROM mcr.microsoft.com/azure-functions/dotnet:4

# Globalization Dependencies
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Fixes Writing to the root filesystem
ENV COMPlus_EnableDiagnostics=0
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:8080

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
  AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]