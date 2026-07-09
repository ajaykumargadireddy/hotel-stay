import { RoomType } from './enums';

export interface Room {
  roomId: string;
  provider: string;
  destination: string;
  location: string;
  roomType: RoomType;
  checkIn: string;
  checkOut: string;
  perNightRate: number;
  currency: string;
  cancellationPolicy: string;
  amenities: string[];
  starRating?: number | null;
}

// Extended Room interface with calculated fields for UI
export interface RoomWithDetails extends Room {
  numberOfNights: number;
  totalPrice: number;
}
