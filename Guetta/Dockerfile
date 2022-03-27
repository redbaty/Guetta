FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
ARG TARGETPLATFORM
ARG BUILDPLATFORM
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY . .
WORKDIR "/src/Guetta"
RUN dotnet publish "Guetta.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN apk add curl python3
RUN curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o /usr/local/bin/yt-dlp
RUN chmod a+rx /usr/local/bin/yt-dlp
ENTRYPOINT ["dotnet", "Guetta.dll"]
