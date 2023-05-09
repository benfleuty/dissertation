#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TranscodeNowWebServer/TranscodeNowWebServer.csproj", "TranscodeNowWebServer/"]
COPY ["UploadedFilesLibrary/UploadedFilesLibrary.csproj", "UploadedFilesLibrary/"]
RUN dotnet restore "TranscodeNowWebServer/TranscodeNowWebServer.csproj"
COPY . .
WORKDIR "/src/TranscodeNowWebServer"
RUN dotnet build "TranscodeNowWebServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TranscodeNowWebServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=mwader/static-ffmpeg:6.0 /ffmpeg /usr/local/bin/
COPY --from=mwader/static-ffmpeg:6.0 /ffprobe /usr/local/bin/
ENTRYPOINT ["dotnet", "TranscodeNowWebServer.dll"]