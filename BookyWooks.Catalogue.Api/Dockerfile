#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BookyWooks.Catalogue.Api/BookyWooks.Catalogue.Api.csproj", "BookyWooks.Catalogue.Api/"]
COPY ["BookyWooks.Messaging/BookyWooks.Messaging.csproj", "BookyWooks.Messaging/"]
COPY ["Tracing/Tracing.csproj", "Tracing/"]
RUN dotnet restore "./BookyWooks.Catalogue.Api/BookyWooks.Catalogue.Api.csproj"
COPY . .
WORKDIR "/src/BookyWooks.Catalogue.Api"
RUN dotnet build "./BookyWooks.Catalogue.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BookyWooks.Catalogue.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookyWooks.Catalogue.Api.dll"]