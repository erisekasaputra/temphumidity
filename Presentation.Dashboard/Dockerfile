FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

EXPOSE 5007 

COPY ["Presentation.Dashboard/Presentation.Dashboard.csproj", "Presentation.Dashboard/"]
COPY ["Assets.Domain/Assets.Domain.csproj", "Assets.Domain/"]
COPY ["LoggerManager/LoggerManager.csproj", "LoggerManager/"]
COPY ["MqttHub/MqttHub.csproj", "MqttHub/"]

RUN dotnet restore "./Presentation.Dashboard/Presentation.Dashboard.csproj"

COPY . . 

WORKDIR "/src/Presentation.Dashboard"

RUN dotnet build "./Presentation.Dashboard.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN dotnet nuget locals all --clear

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Presentation.Dashboard.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 

ENTRYPOINT [ "dotnet", "Presentation.Dashboard.dll" ]