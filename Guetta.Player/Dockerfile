FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Guetta.Player/Guetta.Player.csproj", "Guetta.Player/"]
RUN dotnet restore "Guetta.Player/Guetta.Player.csproj"
COPY . .
WORKDIR "/src/Guetta.Player"
RUN dotnet build "Guetta.Player.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Guetta.Player.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update && apt-get install libopus0 libopus-dev libsodium23 libsodium-dev ffmpeg curl python -y && apt-get clean && apt-get autoremove -y
RUN curl -L https://yt-dl.org/downloads/latest/youtube-dl -o /usr/local/bin/youtube-dl && chmod a+rx /usr/local/bin/youtube-dl
ENTRYPOINT ["dotnet", "Guetta.Player.dll"]
