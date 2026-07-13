import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router, ActivatedRoute, Navigation } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReservationPageComponent } from './reservation-page.component';
import { HotelService } from '../../services/hotel.service';
import { RoomDetails, RoomWithDetails, RoomType, DocumentType } from '../../models';

describe('ReservationPageComponent', () => {
  let component: ReservationPageComponent;
  let fixture: ComponentFixture<ReservationPageComponent>;
  let hotelService: jasmine.SpyObj<HotelService>;
  let router: jasmine.SpyObj<Router>;
  let activatedRoute: any;

  const mockRoomDetails: RoomDetails = {
    roomId: '11111111-1111-1111-1111-111111111111',
    provider: 'PremierStays',
    destination: 'LON',
    location: 'United Kingdom',
    roomType: 'Deluxe',
    perNightRate: 150,
    currency: 'GBP',
    cancellationPolicy: 'Free Cancellation',
    amenities: ['WiFi', 'AC', 'TV'],
    starRating: 4
  };

  const mockRoomWithDetails: RoomWithDetails = {
    roomId: mockRoomDetails.roomId,
    provider: mockRoomDetails.provider,
    destination: mockRoomDetails.destination,
    location: mockRoomDetails.location,
    roomType: RoomType.Deluxe,
    checkIn: '2026-08-01',
    checkOut: '2026-08-05',
    perNightRate: mockRoomDetails.perNightRate,
    currency: mockRoomDetails.currency,
    cancellationPolicy: mockRoomDetails.cancellationPolicy,
    amenities: mockRoomDetails.amenities,
    starRating: mockRoomDetails.starRating,
    numberOfNights: 4,
    totalPrice: 600
  };

  beforeEach(async () => {
    const hotelServiceSpy = jasmine.createSpyObj('HotelService', ['getRoomById', 'reserveRoom', 'getReservation']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate', 'getCurrentNavigation']);

    activatedRoute = {
      queryParams: of({ checkIn: '2026-08-01', checkOut: '2026-08-05' }),
      snapshot: {
        params: { roomId: '11111111-1111-1111-1111-111111111111' }
      }
    };

    await TestBed.configureTestingModule({
      imports: [ReservationPageComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: HotelService, useValue: hotelServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: activatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ReservationPageComponent);
    component = fixture.componentInstance;
    hotelService = TestBed.inject(HotelService) as jasmine.SpyObj<HotelService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit - query params validation', () => {
    it('should redirect to home if checkIn query param is missing', () => {
      activatedRoute.queryParams = of({ checkOut: '2026-08-05' });
      router.getCurrentNavigation.and.returnValue(null);

      component.ngOnInit();

      expect(router.navigate).toHaveBeenCalledWith(['/']);
    });

    it('should redirect to home if checkOut query param is missing', () => {
      activatedRoute.queryParams = of({ checkIn: '2026-08-01' });
      router.getCurrentNavigation.and.returnValue(null);

      component.ngOnInit();

      expect(router.navigate).toHaveBeenCalledWith(['/']);
    });
  });

  describe('ngOnInit - state-based fast path', () => {
    it('should use room from navigation state when available', (done) => {
      router.getCurrentNavigation.and.returnValue({
        extras: {
          state: {
            room: mockRoomWithDetails,
            countryCode: 'GB'
          }
        }
      } as any);

      component.ngOnInit();

      setTimeout(() => {
        expect(component.room).toEqual(mockRoomWithDetails);
        expect(component.isInternational).toBe(true);
        expect(hotelService.getRoomById).not.toHaveBeenCalled();
        done();
      }, 10);
    });
  });

  describe('ngOnInit - API fallback for deep-linking', () => {
    beforeEach(() => {
      router.getCurrentNavigation.and.returnValue(null);
    });

    it('should fetch room details from API when state is missing', (done) => {
      hotelService.getRoomById.and.returnValue(of(mockRoomDetails));

      component.ngOnInit();

      setTimeout(() => {
        expect(hotelService.getRoomById).toHaveBeenCalledWith('11111111-1111-1111-1111-111111111111');
        expect(component.loading).toBe(false);
        expect(component.room).toBeTruthy();
        done();
      }, 10);
    });

    it('should calculate numberOfNights from date range', (done) => {
      hotelService.getRoomById.and.returnValue(of(mockRoomDetails));

      component.ngOnInit();

      setTimeout(() => {
        expect(component.room?.numberOfNights).toBe(4);
        done();
      }, 10);
    });

    it('should calculate totalPrice as perNightRate * nights', (done) => {
      hotelService.getRoomById.and.returnValue(of(mockRoomDetails));

      component.ngOnInit();

      setTimeout(() => {
        expect(component.room?.totalPrice).toBe(600); // 150 * 4
        done();
      }, 10);
    });

    it('should set isInternational to false for India locations', (done) => {
      const indiaRoom = { ...mockRoomDetails, location: 'India' };
      hotelService.getRoomById.and.returnValue(of(indiaRoom));

      component.ngOnInit();

      setTimeout(() => {
        expect(component.isInternational).toBe(false);
        done();
      }, 10);
    });

    it('should set isInternational to true for non-India locations', (done) => {
      hotelService.getRoomById.and.returnValue(of(mockRoomDetails));

      component.ngOnInit();

      setTimeout(() => {
        expect(component.isInternational).toBe(true);
        done();
      }, 10);
    });

    it('should show "Room not found" error for 404 response', (done) => {
      hotelService.getRoomById.and.returnValue(throwError(() => ({ status: 404 })));

      component.ngOnInit();

      setTimeout(() => {
        expect(component.errorMessage).toBe('Room not found. Please search again.');
        expect(component.loading).toBe(false);
        done();
      }, 10);
    });

    it('should show generic error for non-404 failures', (done) => {
      hotelService.getRoomById.and.returnValue(throwError(() => ({ status: 500 })));

      component.ngOnInit();

      setTimeout(() => {
        expect(component.errorMessage).toBe('Unable to load room details.');
        done();
      }, 10);
    });
  });

  describe('onReserve', () => {
    beforeEach(() => {
      component.room = mockRoomWithDetails;
    });

    it('should set submitting state and call reserve service', () => {
      component.checkIn = '2026-08-01';
      component.checkOut = '2026-08-05';
      hotelService.reserveRoom.and.returnValue(of('REF-12345678'));
      hotelService.getReservation.and.returnValue(of({} as any));

      const request = {
        roomId: '11111111-1111-1111-1111-111111111111',
        checkIn: '2026-08-01',
        checkOut: '2026-08-05',
        document: {
          holderName: 'John Doe',
          type: DocumentType.Passport,
          number: 'P123456'
        }
      };

      component.onReserve(request as any);

      expect(hotelService.reserveRoom).toHaveBeenCalled();
    });

    it('should navigate to confirmation page on success', (done) => {
      const mockReservation = { referenceNumber: 'REF-12345678' } as any;
      hotelService.reserveRoom.and.returnValue(of('REF-12345678'));
      hotelService.getReservation.and.returnValue(of(mockReservation));

      component.onReserve({} as any);

      setTimeout(() => {
        expect(router.navigate).toHaveBeenCalledWith(
          ['/confirmation', 'REF-12345678'],
          { state: { reservation: mockReservation } }
        );
        done();
      }, 10);
    });

    it('should show document validation error for 422 status', (done) => {
      hotelService.reserveRoom.and.returnValue(throwError(() => ({ status: 422 })));

      component.onReserve({} as any);

      setTimeout(() => {
        expect(component.errorMessage).toBe('Document validation failed.');
        expect(component.submitting).toBe(false);
        done();
      }, 10);
    });

    it('should show error detail from API when available', (done) => {
      hotelService.reserveRoom.and.returnValue(
        throwError(() => ({ status: 400, error: { detail: 'Room unavailable' } }))
      );

      component.onReserve({} as any);

      setTimeout(() => {
        expect(component.errorMessage).toBe('Room unavailable');
        done();
      }, 10);
    });
  });
});
