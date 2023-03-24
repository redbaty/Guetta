FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/Guetta"
RUN dotnet publish "Guetta.csproj" -c Release -o /app/publish

FROM base AS final

RUN apt-get update && apt-get install libopus0 libopus-dev libsodium23 libsodium-dev python3 curl ffmpeg -y && apt-get clean && apt-get autoremove -y

WORKDIR /app
COPY --from=build /app/publish .
RUN curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o /usr/local/bin/yt-dlp
RUN chmod a+rx /usr/local/bin/yt-dlp

RUN chmod a+rx /usr/local/bin/ffmpeg
ENTRYPOINT ["dotnet", "Guetta.dll"]
