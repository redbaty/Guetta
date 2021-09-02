import { IChannelRequest } from './IChannelRequest';

export interface IVolumeRequest extends IChannelRequest {
    volume: number;
}
