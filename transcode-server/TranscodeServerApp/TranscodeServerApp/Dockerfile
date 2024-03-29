#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TranscodeServerApp/TranscodeServerApp.csproj", "TranscodeServerApp/"]
RUN dotnet restore "TranscodeServerApp/TranscodeServerApp.csproj"
COPY . .
WORKDIR "/src/TranscodeServerApp"
RUN dotnet build "TranscodeServerApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TranscodeServerApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=mwader/static-ffmpeg:6.0 /ffmpeg /usr/local/bin/
COPY --from=mwader/static-ffmpeg:6.0 /ffprobe /usr/local/bin/
ENTRYPOINT ["dotnet", "TranscodeServerApp.dll"]