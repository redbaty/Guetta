docker build -f .\Guetta\Dockerfile -t redbaty/homelab:guetta .
docker build -f .\Guetta.Player\Dockerfile -t redbaty/homelab:guetta-player .

docker push redbaty/homelab:guetta
docker push redbaty/homelab:guetta-player