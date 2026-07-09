import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  HotelSearchRequest,
  HotelSearchResponse,
  RoomWithDetails,
  ReservationRequest,
  ReservationResponse
} from '../models';

@Injectable({ providedIn: 'root' })
export class HotelService {
  private readonly http = inject(HttpClient);

  searchHotels(request: HotelSearchRequest): Observable<RoomWithDetails[]> {
    let params = new HttpParams()
      .set('destination', request.destination)
      .set('checkIn', request.checkIn)
      .set('checkOut', request.checkOut);

    if (request.roomType) {
      params = params.set('roomType', request.roomType);
    }

    return this.http
      .get<HotelSearchResponse>('/hotels/search', { params })
      .pipe(
        map(response => 
          response.results?.map(result => ({
            ...result.room,
            numberOfNights: result.totalNights,
            totalPrice: result.totalPrice
          })) ?? []
        )
      );
  }

  reserveRoom(request: ReservationRequest): Observable<string> {
    return this.http.post('/hotels/reserve', request, { responseType: 'text' }).pipe(
      map(reference => reference.replace(/^['"]|['"]$/g, '').trim())
    );
  }

  getReservation(reference: string): Observable<ReservationResponse> {
    return this.http.get<ReservationResponse>(`/hotels/reservation/${reference}`);
  }
}
