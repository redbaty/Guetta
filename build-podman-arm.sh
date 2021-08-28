podman build -f ./Guetta/Dockerfile -t redbaty/guetta:latest .
podman build -f ./Guetta.Player/Dockerfile -t redbaty/guetta:player-latest .

podman push redbaty/guetta:latest
podman push redbaty/guetta:player-latest