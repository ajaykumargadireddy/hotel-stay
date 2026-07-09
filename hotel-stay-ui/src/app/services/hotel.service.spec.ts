import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpParams } from '@angular/common/http';
import { of } from 'rxjs';
import { HotelService } from './hotel.service';
import { HotelSearchRequest, ReservationRequest, RoomType, DocumentType } from '../models';

describe('HotelService', () => {
  let service: HotelService;
  let httpSpy: jasmine.SpyObj<HttpClient>;

  beforeEach(() => {
    httpSpy = jasmine.createSpyObj('HttpClient', ['get', 'post']);
    TestBed.configureTestingModule({
      providers: [
        HotelService,
        { provide: HttpClient, useValue: httpSpy }
      ]
    });
    service = TestBed.inject(HotelService);
  });

  it('searchHotels calls /hotels/search with query params and unwraps results', (done) => {
    const mockResponse = {
      results: [{
        room: {
          roomId: 'r1',
          provider: 'TestProvider',
          destination: 'BOM',
          location: 'Mumbai',
          roomType: 2,
          checkIn: '2026-08-01',
          checkOut: '2026-08-05',
          perNightRate: 1000,
          currency: 'INR',
          cancellationPolicy: 'Flexible',
          amenities: [],
          starRating: 4
        },
        totalNights: 4,
        totalPrice: 4000
      }]
    };
    httpSpy.get.and.returnValue(of(mockResponse));

    const request: HotelSearchRequest = {
      destination: 'Mumbai',
      checkIn: '2026-08-01',
      checkOut: '2026-08-05',
      roomType: RoomType.Deluxe
    };

    service.searchHotels(request).subscribe(rooms => {
      expect(rooms.length).toBe(1);
      expect(rooms[0].roomId).toBe('r1');
      expect(rooms[0].numberOfNights).toBe(4);
      expect(rooms[0].totalPrice).toBe(4000);
      expect(httpSpy.get).toHaveBeenCalled();
      const [url, opts] = httpSpy.get.calls.mostRecent().args;
      expect(url).toBe('/hotels/search');
      const params = (opts as { params: HttpParams }).params;
      expect(params.get('destination')).toBe('Mumbai');
      expect(params.get('roomType')).toBe('Deluxe');
      done();
    });
  });

  it('searchHotels returns empty array when API returns no results', (done) => {
    httpSpy.get.and.returnValue(of({ results: [] }));

    service.searchHotels({
      destination: 'X',
      checkIn: '2026-08-01',
      checkOut: '2026-08-02'
    }).subscribe(rooms => {
      expect(rooms).toEqual([]);
      done();
    });
  });

  it('reserveRoom posts to /hotels/reserve with request body', (done) => {
    const mockResponse = { referenceNumber: 'REF-abc12345' };
    httpSpy.post.and.returnValue(of(mockResponse));

    const request: ReservationRequest = {
      roomId: 'r1',
      checkIn: '2026-08-01',
      checkOut: '2026-08-05',
      document: {
        holderName: 'John Doe',
        type: DocumentType.Passport,
        number: 'AB1234567'
      }
    };

    service.reserveRoom(request).subscribe(res => {
      expect(res.referenceNumber).toBe('REF-abc12345');
      expect(httpSpy.post).toHaveBeenCalledWith('/hotels/reserve', request);
      done();
    });
  });

  it('getReservation calls endpoint with reference number', (done) => {
    httpSpy.get.and.returnValue(of({ referenceNumber: 'REF-xyz' }));

    service.getReservation('REF-xyz').subscribe(res => {
      expect(res.referenceNumber).toBe('REF-xyz');
      expect(httpSpy.get).toHaveBeenCalledWith('/hotels/reservation/REF-xyz');
      done();
    });
  });
});
