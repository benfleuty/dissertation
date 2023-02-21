# Use the official .NET Core runtime image as the base
FROM mcr.microsoft.com/dotnet/core/runtime:3.1

RUN apt-get update -y && apt-get install -y ffmpeg file

# Copy the published app to the container
COPY TranscodeServerApp/TranscodeServerApp/bin/Release/net6.0/publish/linux-x64/TranscodeServerApp /app/
#COPY bbb_sunflower_1080p_60fps_normal.mp4 /app/input.mp4
#COPY gura2.webm /app/input.mp4
# Replace with a red herring for MIME type testing
COPY image-that-is-a.mp4 /app/input.mp4
#COPY bbb_sunflower_1080p_60fps_normal.mp4.png /app/input.mp4

#COPY txt /app/input.mp4

# Set the working directory to /app
WORKDIR /app

# Expose the port that the app listens on
EXPOSE 80

#CMD ffprobe -v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 input.mp4 && ffprobe -v error -select_streams a:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 input.mp4

# Set the entry point to the app executable
ENTRYPOINT ["./TranscodeServerApp"]