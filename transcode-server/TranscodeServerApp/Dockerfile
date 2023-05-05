
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

RUN apt update -y && apt install -y ffmpeg

ENTRYPOINT ["dotnet", "TranscodeServerApp.dll"]