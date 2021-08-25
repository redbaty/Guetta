FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/Guetta"
RUN dotnet publish "Guetta.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN apt-get update && apt-get install libopus0 libopus-dev libsodium23 libsodium-dev ffmpeg curl python -y && apt-get clean && apt-get autoremove -y
RUN curl -L https://yt-dl.org/downloads/latest/youtube-dl -o /usr/local/bin/youtube-dl && chmod a+rx /usr/local/bin/youtube-dl
ENTRYPOINT ["dotnet", "Guetta.dll"]
