﻿FROM alpine:3.15 AS ffmpeg-base
RUN apk update && apk add wget
RUN wget https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-linux64-gpl.tar.xz
RUN mkdir /ffmpeg
RUN tar -C /ffmpeg -xvf ffmpeg-master-latest-linux64-gpl.tar.xz --strip-components=1

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/Guetta"
RUN dotnet publish "Guetta.csproj" -c Release -o /app/publish

FROM base AS final

RUN apt-get update && apt-get install libopus0 libopus-dev libsodium23 libsodium-dev python3 curl -y && apt-get clean && apt-get autoremove -y

WORKDIR /app
COPY --from=build /app/publish .
RUN curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o /usr/local/bin/yt-dlp
RUN chmod a+rx /usr/local/bin/yt-dlp

COPY --from=ffmpeg-base /ffmpeg/bin/ /usr/local/bin/
RUN chmod a+rx /usr/local/bin/ffmpeg
ENTRYPOINT ["dotnet", "Guetta.dll"]
