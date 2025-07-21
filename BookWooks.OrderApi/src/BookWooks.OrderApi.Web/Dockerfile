##See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
#
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER app
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["BookWooks.OrderApi/Directory.Packages.props", "BookWooks.OrderApi/"]
#COPY ["BookWooks.OrderApi/Directory.Build.props", "BookWooks.OrderApi/"]
#COPY ["BookWooks.OrderApi/nuget.config", "BookWooks.OrderApi/"]
#COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Web/BookWooks.OrderApi.Web.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Web/"]
#COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Infrastructure/BookWooks.OrderApi.Infrastructure.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Infrastructure/"]
#COPY ["BookyWooks.SharedKernel/BookyWooks.SharedKernel.csproj", "BookyWooks.SharedKernel/"]
#COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Core/BookWooks.OrderApi.Core.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Core/"]
#COPY ["BookyWooks.Messaging/BookyWooks.Messaging.csproj", "BookyWooks.Messaging/"]
#COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.UseCases/BookWooks.OrderApi.UseCases.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.UseCases/"]
#COPY ["Logging/Logging.csproj", "Logging/"]
#COPY ["Tracing/Tracing.csproj", "Tracing/"]
#RUN dotnet restore "./BookWooks.OrderApi/src/BookWooks.OrderApi.Web/BookWooks.OrderApi.Web.csproj"
#COPY . .
#WORKDIR "/src/BookWooks.OrderApi/src/BookWooks.OrderApi.Web"
#RUN dotnet build "./BookWooks.OrderApi.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## Integration Test Stage
#FROM build as integrationtest
#WORKDIR "/BookWooks.OrderApi.TestContainersIntegrationTests"
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./BookWooks.OrderApi.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "BookWooks.OrderApi.Web.dll"]

# ---- BASE IMAGE (for runtime) ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---- BUILD IMAGE (for compiling) ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for faster restore
COPY ["BookWooks.OrderApi/Directory.Packages.props", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/Directory.Build.props", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/nuget.config", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Web/BookWooks.OrderApi.Web.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Web/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Infrastructure/BookWooks.OrderApi.Infrastructure.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Infrastructure/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Core/BookWooks.OrderApi.Core.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Core/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.UseCases/BookWooks.OrderApi.UseCases.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.UseCases/"]
COPY ["BookyWooks.SharedKernel/BookyWooks.SharedKernel.csproj", "BookyWooks.SharedKernel/"]
COPY ["BookyWooks.Messaging/BookyWooks.Messaging.csproj", "BookyWooks.Messaging/"]
COPY ["Logging/Logging.csproj", "Logging/"]
COPY ["Tracing/Tracing.csproj", "Tracing/"]
COPY ["BookWooks.OrderApi.TestContainersIntegrationTests/BookWooks.OrderApi.TestContainersIntegrationTests.csproj", "BookWooks.OrderApi.TestContainersIntegrationTests/"]
COPY ["IntegrationTestingSetup/IntegrationTestingSetup.csproj", "IntegrationTestingSetup/"]

# Copy everything else and restore all projects
COPY . .
RUN dotnet restore

# Build the main web project
WORKDIR "/src/BookWooks.OrderApi/src/BookWooks.OrderApi.Web"
RUN dotnet build "./BookWooks.OrderApi.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ---- INTEGRATION TEST STAGE ----
FROM build as integrationtest
WORKDIR "/src/BookWooks.OrderApi.TestContainersIntegrationTests"
RUN dotnet build "BookWooks.OrderApi.TestContainersIntegrationTests.csproj" -c Release --no-restore
ENTRYPOINT ["dotnet", "test", "BookWooks.OrderApi.TestContainersIntegrationTests.csproj", "--logger", "trx;LogFileName=/tests/booky-wooks-integration-test-results.xml"]

# ---- PUBLISH STAGE ----
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BookWooks.OrderApi.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---- FINAL RUNTIME IMAGE ----
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookWooks.OrderApi.Web.dll"]
