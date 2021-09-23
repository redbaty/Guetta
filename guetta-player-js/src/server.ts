import Fastify, {FastifyInstance} from 'fastify'
import {Client, Intents} from 'discord.js';
import {AudioPlayer, AudioResource, generateDependencyReport, VoiceConnection} from '@discordjs/voice';
import {Track} from './track.js';
import Redis from 'ioredis';
import * as Amqp from "amqp-ts";
import {AudioService, PlayRequest} from "./audio-service.js";



if (!process.env.REDIS_PORT || !process.env.REDIS_HOST) {
    console.log('Redis ENVS not set.')
    process.exit(1)
}

if (!process.env.RABBIT_MQ_USER || !process.env.RABBIT_MQ_PASS || !process.env.RABBIT_MQ_HOST) {
    console.log('RABBITMQ ENVS not set.')
    process.exit(1)
}

const redis = new Redis(+process.env.REDIS_PORT, process.env.REDIS_HOST);
const subRedis = new Redis(+process.env.REDIS_PORT, process.env.REDIS_HOST);

const server: FastifyInstance = Fastify();

const client = new Client({
    intents: [Intents.FLAGS.GUILDS, Intents.FLAGS.GUILD_MESSAGES, Intents.FLAGS.GUILD_VOICE_STATES]
});

const audioService = new AudioService(redis, subRedis, client);

client.once('ready', async () => {
    console.log('Ready!');
    
    const connection = new Amqp.Connection(`amqp://${process.env.RABBIT_MQ_USER}:${process.env.RABBIT_MQ_PASS}@${process.env.RABBIT_MQ_HOST}:5672`);
    const queue = connection.declareQueue("player");
    const queueManager = connection.declareQueue("queue_command");

    queue.activateConsumer((m) => {
        console.log(m.properties);
        console.log("Message received: " + m.getContent());

        const playRequest = JSON.parse(m.getContent()) as PlayRequest;
        audioService.play(playRequest)
            .then(t => {
                if (t) {
                    m.ack();
                    queueManager.publish({voiceChannelId: playRequest.voiceChannelId, type: 1})
                } else {
                    m.nack(undefined, true);
                }
            })
            .catch(t => {
            m.nack(undefined, true);
        })
    }, {manualAck: true})

    try {
        await server.listen(process.env.PORT || 3000, '0.0.0.0')
    } catch (err) {
        server.log.error(err)
        process.exit(1)
    }
});

console.log('Trying to log in');
await client.login(process.env.TOKEN);
console.log('Logged in');

console.log(generateDependencyReport());