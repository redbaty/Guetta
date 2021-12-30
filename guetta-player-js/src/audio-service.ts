import {Redis} from "ioredis";
import {Client, VoiceChannel} from "discord.js";
import {
    AudioPlayerStatus,
    createAudioPlayer, DiscordGatewayAdapterCreator,
    entersState,
    joinVoiceChannel,
    NoSubscriberBehavior,
    VoiceConnectionStatus
} from "@discordjs/voice";
import {nanoid} from "nanoid";
import {Track} from "./track.js";

export interface PlayRequest {
    voiceChannelId: string;
    videoInformation: { url: string; title: string; }
}

export class AudioService {
    constructor(private redis: Redis, private subConnection: Redis, private client: Client) {
    }

    public play(request: PlayRequest): Promise<boolean> {
        return new Promise<boolean>(async (resolve, reject) => {
            const channel = await this.client.channels.fetch(request.voiceChannelId);

            if (channel && channel instanceof VoiceChannel) {
                const connection = joinVoiceChannel({
                    channelId: channel.id,
                    guildId: channel.guild.id,
                    adapterCreator: channel.guild.voiceAdapterCreator as DiscordGatewayAdapterCreator,
                });

                console.log(`Going to play video: ${request.videoInformation.url}`);

                await entersState(connection, VoiceConnectionStatus.Ready, 20e3);
                const id = nanoid();

                const player = createAudioPlayer({
                    behaviors: {
                        noSubscriber: NoSubscriberBehavior.Stop
                    }
                });

                await entersState(player, AudioPlayerStatus.Idle, 20e3);

                const track = await Track.from(request.videoInformation.url, id, request.videoInformation.title, {
                    onStart() {
                        console.log('started');
                    },
                    onFinish() {
                        console.log('finished')
                    },
                    onError(e) {
                        reject('Player failed')
                    }
                });


                const resource = await track.createAudioResource();

                const currentVolume = await this.redis.hget(request.voiceChannelId, 'volume');
                resource.volume?.setVolume(currentVolume ? +currentVolume : 1);

                player.play(resource);
                connection.subscribe(player);

                this.subConnection.subscribe(`${request.voiceChannelId}:volume`, `${request.voiceChannelId}:stop`, (err, count) => {
                    if(!err){
                        this.subConnection.on("message", (channel, message) => {
                            if(channel.endsWith(':volume')) {
                                console.log(`Altering volume to: ${message}`)
                                resource.volume?.setVolume(+message);
                            }

                            if(channel.endsWith(':stop')){
                                console.log('Stopping')
                                player.stop();
                            }
                        });
                    }
                });

                player.once(AudioPlayerStatus.Idle, () => {
                    this.subConnection.unsubscribe(`${request.voiceChannelId}:volume`, `${request.voiceChannelId}:stop`);
                    resolve(true);
                });
            } else {
                resolve(false);
            }
        });

    }
}