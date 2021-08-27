podman build -f ./Guetta/Dockerfile -t redbaty/homelab:guetta-arm . &
podman build -f ./Guetta.Player/Dockerfile -t redbaty/homelab:guetta-player-arm . &
wait

podman push redbaty/homelab:guetta-arm
podman push redbaty/homelab:guetta-player-arm