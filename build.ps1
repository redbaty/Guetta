docker build -f .\Guetta\Dockerfile -t redbaty/guetta:latest .
docker build -f .\Guetta.Queue\Dockerfile -t redbaty/guetta-queue:latest .

Set-Location guetta-player-js
docker build -t redbaty/guetta-player-js:latest .
Set-Location ..

docker push redbaty/guetta:latest
docker push redbaty/guetta-queue:latest
docker push redbaty/guetta-player-js:latest