# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BookWooks.OrderApi/Directory.Packages.props", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/Directory.Build.props", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/nuget.config", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi.TestContainersIntegrationTests/BookWooks.OrderApi.TestContainersIntegrationTests.csproj", "BookWooks.OrderApi.TestContainersIntegrationTests/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Web/BookWooks.OrderApi.Web.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Web/"]
COPY ["BookWooks.MCPServer/BookWooks.MCPServer.csproj", "BookWooks.MCPServer/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Infrastructure/BookWooks.OrderApi.Infrastructure.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Infrastructure/"]
COPY ["BookyWooks.SharedKernel/BookyWooks.SharedKernel.csproj", "BookyWooks.SharedKernel/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Core/BookWooks.OrderApi.Core.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Core/"]
COPY ["BookyWooks.Messaging/BookyWooks.Messaging.csproj", "BookyWooks.Messaging/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.UseCases/BookWooks.OrderApi.UseCases.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.UseCases/"]
COPY ["Tracing/Tracing.csproj", "Tracing/"]
COPY ["Logging/Logging.csproj", "Logging/"]
COPY ["IntegrationTestingSetup/IntegrationTestingSetup.csproj", "IntegrationTestingSetup/"]
RUN dotnet restore "./BookWooks.OrderApi.TestContainersIntegrationTests/BookWooks.OrderApi.TestContainersIntegrationTests.csproj"
COPY . .
WORKDIR "/src/BookWooks.OrderApi.TestContainersIntegrationTests"
RUN dotnet build "./BookWooks.OrderApi.TestContainersIntegrationTests.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BookWooks.OrderApi.TestContainersIntegrationTests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookWooks.OrderApi.TestContainersIntegrationTests.dll"]