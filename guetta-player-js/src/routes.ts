import {FastifyInstance} from "fastify";
import {IChannelRequest} from "./IChannelRequest";
import {AudioResource, createAudioPlayer, entersState, joinVoiceChannel, VoiceConnectionStatus} from "@discordjs/voice";
import {Track} from "./track";
import {IVolumeRequest} from "./IVolumeRequest";
import {IPlayRequest} from "./IPlayRequest";
import {Client, VoiceChannel} from "discord.js";
import {nanoid} from "nanoid";
import {Redis} from "ioredis";

export function setupSkipRoute(server: FastifyInstance, playingMap: Map<String, AudioResource<Track>>) {
    server.post<{ Body: IChannelRequest }>('/skip', async (request, reply) => {
        const currentlyPlaying = playingMap.get(request.body.voiceChannelId);
        if (currentlyPlaying && currentlyPlaying.audioPlayer) {
            const stopped = currentlyPlaying.audioPlayer.stop();

            if (stopped) {
                playingMap.delete(request.body.voiceChannelId);
            }

            return stopped;
        }

        return false;
    });
}

export function setupPlayingRoute(server: FastifyInstance, playingMap: Map<String, AudioResource<Track>>) {
    server.post<{ Body: IChannelRequest }>('/playing', async (request, reply) => {
        const currentlyPlaying = playingMap.get(request.body.voiceChannelId);
        if (currentlyPlaying && currentlyPlaying.audioPlayer) {
            return true;
        }

        return false;
    });
}

export function setupVolumeRoute(server: FastifyInstance, playingMap: Map<String, AudioResource<Track>>) {
    server.post<{ Body: IVolumeRequest }>('/volume', async (request, reply) => {
        const currentlyPlaying = playingMap.get(request.body.voiceChannelId);
        if (currentlyPlaying) {
            currentlyPlaying.volume?.setVolume(request.body.volume);
            return true;
        }

        return false;
    });
}

export function setupPlayRoute(server: FastifyInstance, playingMap: Map<String, AudioResource<Track>>, redis: Redis, client: Client) {
    server.post<{ Body: IPlayRequest }>('/play', async (request, reply) => {

        const channel = await client.channels.fetch(request.body.voiceChannelId);

        if (channel && channel instanceof VoiceChannel) {
            const connection = joinVoiceChannel({
                channelId: channel.id,
                guildId: channel.guild.id,
                adapterCreator: channel.guild.voiceAdapterCreator,
            });

            console.log(`Going to play video: ${request.body.videoInformation.url}`);

            await entersState(connection, VoiceConnectionStatus.Ready, 20e3);
            const id = nanoid();

            const track = await Track.from(request.body.videoInformation.url, request.body.videoInformation.title, {
                onStart() {
                    console.log('started')
                },
                onFinish() {
                    console.log('finished')
                    playingMap.delete(request.body.voiceChannelId);
                    redis.publish(`${id}:ended`, 'success');
                },
                onError(e) {
                    console.log('fuck')
                    console.log(e)
                }
            });

            const player = createAudioPlayer();
            const resource = await track.createAudioResource();

            if (request.body.initialVolume) {
                console.log(`Setting volume to ${request.body.initialVolume}`)
                resource.volume?.setVolume(request.body.initialVolume);
            }

            player.play(resource);
            connection.subscribe(player);


            playingMap.set(request.body.voiceChannelId, resource);
            return id;
        }

        reply.code(400);
    });
}

export function setupRoutes(server: FastifyInstance, playingMap: Map<String, AudioResource<Track>>, redis: Redis, client: Client) {
    setupSkipRoute(server, playingMap);
    setupPlayingRoute(server, playingMap);
    setupVolumeRoute(server, playingMap);
    setupPlayRoute(server, playingMap, redis, client);
}