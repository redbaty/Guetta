import { AudioResource, createAudioResource, demuxProbe } from '@discordjs/voice';
import { raw as ytdl } from 'youtube-dl-exec';

/**
 * This is the data required to create a Track object
 */
export interface TrackData {
	url: string;
	title: string;
	onStart: () => void;
	onFinish: () => void;
	onError: (error: Error) => void;
}

/**
 * A Track represents information about a YouTube video (in this context) that can be added to a queue.
 * It contains the title and URL of the video, as well as functions onStart, onFinish, onError, that act
 * as callbacks that are triggered at certain points during the track's lifecycle.
 *
 * Rather than creating an AudioResource for each video immediately and then keeping those in a queue,
 * we use tracks as they don't pre-emptively load the videos. Instead, once a Track is taken from the
 * queue, it is converted into an AudioResource just in time for playback.
 */
export class Track implements TrackData {
	public readonly url: string;
	public readonly title: string;
	public readonly onStart: () => void;
	public readonly onFinish: () => void;
	public readonly onError: (error: Error) => void;

	private constructor({ url, title, onStart, onFinish, onError }: TrackData) {
		this.url = url;
		this.title = title;
		this.onStart = onStart;
		this.onFinish = onFinish;
		this.onError = onError;
	}

	/**
	 * Creates an AudioResource from this Track.
	 */
	public createAudioResource(): Promise<AudioResource<Track>> {
		return new Promise((resolve, reject) => {
			const process = ytdl(
				this.url,
				{
					o: '-',
					q: '',
					f: 'bestaudio[ext=webm+acodec=opus+asr=48000]/bestaudio',
					r: '100K',
				},
				{ stdio: ['ignore', 'pipe', 'ignore'] },
			);
			if (!process.stdout) {
				reject(new Error('No stdout'));
				return;
			}
			const stream = process.stdout;
			const onError = (error: Error) => {
				if (!process.killed) process.kill();
				stream.resume();
				reject(error);
			};
			
			process.once('close', () => {
				this.onFinish();
			})

			process
				.once('spawn', () => {
					this.onStart();

					demuxProbe(stream)
						.then((probe) => resolve(createAudioResource(probe.stream, { metadata: this, inputType: probe.type, inlineVolume: true })))
						.catch(onError);
				})
				.catch(onError);
		});
	}

	/**
	 * Creates a Track from a video URL and lifecycle callback methods.
	 *
	 * @param url The URL of the video
	 * @param methods Lifecycle callbacks
	 * @returns The created Track
	 */
	public static async from(url: string, title: string, methods: Pick<Track, 'onStart' | 'onFinish' | 'onError'>): Promise<Track> {
		const wrappedMethods = {
			onStart() {
				methods.onStart();
			},
			onFinish() {
				methods.onFinish();
			},
			onError(error: Error) {
				methods.onError(error);
			},
		};

		return new Track({
			title,
			url,
			...wrappedMethods,
		});
	}
}