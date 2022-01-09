FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
RUN apk update && apk add curl python3 wget
RUN wget https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-linux64-gpl.tar.xz
RUN mkdir /ffmpeg
RUN tar -C /ffmpeg -xvf ffmpeg-master-latest-linux64-gpl.tar.xz --strip-components=1

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/Guetta"
RUN dotnet publish "Guetta.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN curl -L https://yt-dl.org/downloads/latest/youtube-dl -o /usr/local/bin/youtube-dl && chmod a+rx /usr/local/bin/youtube-dl
COPY --from=base /ffmpeg/bin/ /usr/local/bin/
RUN chmod a+rx /usr/local/bin/ffmpeg
ENTRYPOINT ["dotnet", "Guetta.dll"]
