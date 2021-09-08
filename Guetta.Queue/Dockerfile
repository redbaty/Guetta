FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
ARG TARGETPLATFORM
ARG BUILDPLATFORM
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY . .
WORKDIR "/src/Guetta.Queue"
RUN dotnet publish "Guetta.Queue.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Guetta.Queue.dll"]
