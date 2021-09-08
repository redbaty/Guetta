cd guetta-player-js
docker buildx build --platform linux/arm/v7,linux/arm64/v8,linux/amd64 -t redbaty/guetta-player-js:latest --push .
cd ..

docker buildx build --platform linux/arm/v7,linux/arm64/v8,linux/amd64 -f Guetta/Dockerfile -t redbaty/guetta:latest --push .
docker buildx build --platform linux/arm/v7,linux/arm64/v8,linux/amd64 -f Guetta.Queue/Dockerfile -t redbaty/guetta-queue:latest --push .