#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TPRestaurent.BackEndCore.API/TPRestaurent.BackEndCore.API.csproj", "TPRestaurent.BackEndCore.API/"]
COPY ["TPRestaurent.BackEndCore.Application/TPRestaurent.BackEndCore.Application.csproj", "TPRestaurent.BackEndCore.Application/"]
COPY ["TPRestaurent.BackEndCore.Common/TPRestaurent.BackEndCore.Common.csproj", "TPRestaurent.BackEndCore.Common/"]
COPY ["TPRestaurent.BackEndCore.Domain/TPRestaurent.BackEndCore.Domain.csproj", "TPRestaurent.BackEndCore.Domain/"]
COPY ["TPRestaurent.BackEndCore.Infrastructure/TPRestaurent.BackEndCore.Infrastructure.csproj", "TPRestaurent.BackEndCore.Infrastructure/"]
RUN dotnet restore "./TPRestaurent.BackEndCore.API/TPRestaurent.BackEndCore.API.csproj"
COPY . .
WORKDIR "/src/TPRestaurent.BackEndCore.API"
RUN dotnet build "./TPRestaurent.BackEndCore.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TPRestaurent.BackEndCore.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TPRestaurent.BackEndCore.API.dll"]