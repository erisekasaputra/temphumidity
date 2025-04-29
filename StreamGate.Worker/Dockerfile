FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["StreamGate.Worker/StreamGate.Worker.csproj", "StreamGate.Worker/"]
COPY ["Assets.Domain/Assets.Domain.csproj", "Assets.Domain/"]
COPY ["LoggerManager/LoggerManager.csproj", "LoggerManager/"]
COPY ["MqttHub/MqttHub.csproj", "MqttHub/"]

RUN dotnet restore "./StreamGate.Worker/StreamGate.Worker.csproj"

COPY . . 

WORKDIR "/src/StreamGate.Worker"

RUN dotnet build "./StreamGate.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN dotnet nuget locals all --clear

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./StreamGate.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 

ENTRYPOINT [ "dotnet", "StreamGate.Worker.dll" ]





 