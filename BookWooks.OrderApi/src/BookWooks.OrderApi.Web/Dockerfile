# Base Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BookWooks.OrderApi/Directory.Packages.props", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/Directory.Build.props", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/nuget.config", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Web/BookWooks.OrderApi.Web.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Web/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Infrastructure/BookWooks.OrderApi.Infrastructure.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Infrastructure/"]
COPY ["BookyWooks.SharedKernel/BookyWooks.SharedKernel.csproj", "BookyWooks.SharedKernel/"]
COPY ["RabbitMQ/RabbitMQ.csproj", "RabbitMQ/"]
COPY ["BookyWooks.MessageBus/EventBus.csproj", "BookyWooks.MessageBus/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Core/BookWooks.OrderApi.Core.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Core/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.UseCases/BookWooks.OrderApi.UseCases.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.UseCases/"]
COPY ["IntegrationEventsSetUp/OutBoxPattern.csproj", "IntegrationEventsSetUp/"]
RUN dotnet restore "./BookWooks.OrderApi/src/BookWooks.OrderApi.Web/./BookWooks.OrderApi.Web.csproj"
COPY . .
WORKDIR "/src/BookWooks.OrderApi/src/BookWooks.OrderApi.Web"
RUN dotnet build "./BookWooks.OrderApi.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Integration Test Stage
FROM build as integrationtest
WORKDIR "/src/BookyWooks.OrderApi.IntegrationTests"

# Publish Stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BookWooks.OrderApi.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final Stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookWooks.OrderApi.Web.dll"]
