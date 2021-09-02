FROM node:16-alpine

RUN apk update && apk add ffmpeg yarn python2 make libsodium libtool autoconf automake g++

# create root application folder
WORKDIR /app

# copy configs to /app folder
COPY package*.json ./
COPY tsconfig.json ./
# copy source code to /app/src folder
COPY src /app/src

RUN yarn
RUN yarn add sodium
RUN yarn remove libsodium-wrappers
RUN yarn build

EXPOSE 3000

CMD [ "node", "./dist/server.js" ]