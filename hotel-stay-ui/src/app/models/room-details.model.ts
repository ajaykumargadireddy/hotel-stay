import { RoomType } from './enums';

export interface RoomDetails {
  roomId: string;
  provider: string;
  destination: string;
  location: string;
  roomType: string;
  perNightRate: number;
  currency: string;
  cancellationPolicy: string;
  amenities: string[];
  starRating?: number | null;
}
