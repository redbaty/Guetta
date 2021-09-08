import Fastify, {FastifyInstance} from 'fastify'
import {Client, Intents} from 'discord.js';
import {AudioResource, generateDependencyReport} from '@discordjs/voice';
import {Track} from './track.js';
import Redis from 'ioredis';
import {setupRoutes} from './routes.js';

if (!process.env.REDIS_PORT || !process.env.REDIS_HOST) {
    console.log('Redis ENVS not set.')
    process.exit(1)
}

const redis = new Redis(+process.env.REDIS_PORT, process.env.REDIS_HOST);

const server: FastifyInstance = Fastify();

const playingMap = new Map<String, AudioResource<Track>>();

const client = new Client({
    intents: [Intents.FLAGS.GUILDS, Intents.FLAGS.GUILD_MESSAGES, Intents.FLAGS.GUILD_VOICE_STATES]
});

client.once('ready', async () => {
    console.log('Ready!');
    setupRoutes(server, playingMap, redis, client);

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