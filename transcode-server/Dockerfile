# Use the official .NET Core runtime image as the base
FROM mcr.microsoft.com/dotnet/core/runtime:3.1

RUN apt-get update -y && apt-get install -y ffmpeg

# Copy the published app to the container
COPY TranscodeServerApp/TranscodeServerApp/bin/Release/net6.0/publish/linux-x64/TranscodeServerApp /app/
COPY bbb_sunflower_1080p_60fps_normal.mp4 /app/input.mp4
WORKDIR /app

# Set the working directory to /app
WORKDIR /app

# Expose the port that the app listens on
EXPOSE 80

# Set the entry point to the app executable
ENTRYPOINT ["./TranscodeServerApp"]