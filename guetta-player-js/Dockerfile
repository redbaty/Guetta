FROM node:16-alpine

RUN apk update && apk add ffmpeg yarn python2 python3 make libsodium libtool autoconf automake curl g++

# create root application folder
WORKDIR /app

# copy configs to /app folder
COPY package*.json ./
COPY tsconfig.json ./
# copy source code to /app/src folder
COPY src /app/src

RUN curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o /usr/local/bin/yt-dlp
RUN chmod a+rx /usr/local/bin/yt-dlp
RUN yarn
RUN yarn build

EXPOSE 3000

CMD [ "node", "./dist/server.js" ]