import { Room } from './room.model';
import { RoomType } from './enums';

export interface HotelSearchRequest {
  destination: string;
  checkIn: string;
  checkOut: string;
  roomType?: RoomType;
}

export interface SearchResult {
  room: Room;
  totalNights: number;
  totalPrice: number;
}

export interface HotelSearchResponse {
  results: SearchResult[];
}
