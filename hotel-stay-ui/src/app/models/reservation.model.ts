import { Document } from './document.model';
import { RoomType } from './enums';

export interface ReservationRequest {
  roomId: string;
  checkIn: string;
  checkOut: string;
  document: Document;
}

export interface ReservationResponse {
  referenceNumber: string;
  roomId: string;
  destination: string;
  location: string;
  roomType: RoomType;
  roomCheckIn: string;
  roomCheckOut: string;
  checkIn: string;
  checkOut: string;
  numberOfNights: number;
  totalPrice: number;
  currency: string;
  provider: string;
  cancellationPolicy?: string;
  document: Document;
  reservationTimestamp: string;
}
