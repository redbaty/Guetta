docker build -f ./Guetta/Dockerfile -t redbaty/guetta:latest .
docker build -f ./Guetta.Player/Dockerfile -t redbaty/guetta:player-latest .

docker push redbaty/guetta:latest
docker push redbaty/guetta:player-latest