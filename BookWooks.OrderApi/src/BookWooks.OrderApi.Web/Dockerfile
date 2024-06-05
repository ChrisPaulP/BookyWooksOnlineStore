#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BookWooks.OrderApi/Directory.Packages.props", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/Directory.Build.props", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/nuget.config", "BookWooks.OrderApi/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Web/BookWooks.OrderApi.Web.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Web/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Infrastructure/BookWooks.OrderApi.Infrastructure.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Infrastructure/"]
COPY ["BookyWooks.SharedKernel/BookyWooks.SharedKernel.csproj", "BookyWooks.SharedKernel/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.Core/BookWooks.OrderApi.Core.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.Core/"]
COPY ["BookyWooks.Messaging/BookyWooks.Messaging.csproj", "BookyWooks.Messaging/"]
COPY ["BookWooks.OrderApi/src/BookWooks.OrderApi.UseCases/BookWooks.OrderApi.UseCases.csproj", "BookWooks.OrderApi/src/BookWooks.OrderApi.UseCases/"]
COPY ["Logging/Logging.csproj", "Logging/"]
COPY ["Tracing/Tracing.csproj", "Tracing/"]
RUN dotnet restore "./BookWooks.OrderApi/src/BookWooks.OrderApi.Web/BookWooks.OrderApi.Web.csproj"
COPY . .
WORKDIR "/src/BookWooks.OrderApi/src/BookWooks.OrderApi.Web"
RUN dotnet build "./BookWooks.OrderApi.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Integration Test Stage
FROM build as integrationtest
WORKDIR "/src/BookyWooks.OrderApi.IntegrationTests"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BookWooks.OrderApi.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookWooks.OrderApi.Web.dll"]