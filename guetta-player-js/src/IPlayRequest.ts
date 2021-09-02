export interface IPlayRequest {
    textChannelId: string;
    voiceChannelId: string;
    requestedByUser: string;
    initialVolume: number;
    videoInformation: { url: string; title: string; };
}
