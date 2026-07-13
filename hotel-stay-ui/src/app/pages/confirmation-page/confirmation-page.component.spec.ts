import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router, ActivatedRoute, Navigation } from '@angular/router';
import { of, throwError, EMPTY } from 'rxjs';
import { ConfirmationPageComponent } from './confirmation-page.component';
import { HotelService } from '../../services/hotel.service';
import { ReservationResponse, RoomType, DocumentType } from '../../models';

describe('ConfirmationPageComponent', () => {
  let component: ConfirmationPageComponent;
  let fixture: ComponentFixture<ConfirmationPageComponent>;
  let hotelService: jasmine.SpyObj<HotelService>;
  let router: jasmine.SpyObj<Router>;
  let activatedRoute: any;

  const mockReservation: ReservationResponse = {
    referenceNumber: 'REF-12345678',
    roomId: '11111111-1111-1111-1111-111111111111',
    destination: 'BOM',
    location: 'India',
    roomType: RoomType.Deluxe,
    roomCheckIn: '2026-08-01T00:00:00',
    roomCheckOut: '2026-08-05T00:00:00',
    checkIn: '2026-08-01',
    checkOut: '2026-08-05',
    numberOfNights: 4,
    totalPrice: 12000,
    currency: 'INR',
    provider: 'PremierStays',
    document: {
      holderName: 'John Doe',
      type: DocumentType.Passport,
      number: 'P123456'
    },
    reservationTimestamp: '2026-07-13 10:30:00'
  };

  beforeEach(async () => {
    const hotelServiceSpy = jasmine.createSpyObj('HotelService', ['getReservation']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate', 'getCurrentNavigation', 'createUrlTree', 'serializeUrl']);
    routerSpy.serializeUrl.and.returnValue('/');
    routerSpy.events = EMPTY; // Add events observable for RouterLink

    activatedRoute = {
      snapshot: {
        paramMap: {
          get: jasmine.createSpy('get').and.returnValue('REF-12345678')
        }
      }
    };

    await TestBed.configureTestingModule({
      imports: [ConfirmationPageComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: HotelService, useValue: hotelServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: activatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmationPageComponent);
    component = fixture.componentInstance;
    hotelService = TestBed.inject(HotelService) as jasmine.SpyObj<HotelService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit - state-based fast path', () => {
    it('should use reservation from navigation state when available', () => {
      router.getCurrentNavigation.and.returnValue({
        extras: {
          state: { reservation: mockReservation }
        }
      } as any);

      component.ngOnInit();

      expect(component.reservation).toEqual(mockReservation);
      expect(component.loading).toBe(false);
      expect(hotelService.getReservation).not.toHaveBeenCalled();
    });
  });

  describe('ngOnInit - API fallback', () => {
    beforeEach(() => {
      router.getCurrentNavigation.and.returnValue(null);
    });

    it('should redirect to home if reference parameter is missing', () => {
      activatedRoute.snapshot.paramMap.get.and.returnValue(null);

      component.ngOnInit();

      expect(router.navigate).toHaveBeenCalledWith(['/']);
    });

    it('should fetch reservation from API when state is missing', (done) => {
      hotelService.getReservation.and.returnValue(of(mockReservation));

      component.ngOnInit();

      setTimeout(() => {
        expect(hotelService.getReservation).toHaveBeenCalledWith('REF-12345678');
        expect(component.reservation).toEqual(mockReservation);
        expect(component.loading).toBe(false);
        done();
      }, 10);
    });

    it('should show "Reservation not found" error for 404 response', (done) => {
      hotelService.getReservation.and.returnValue(throwError(() => ({ status: 404 })));

      component.ngOnInit();

      setTimeout(() => {
        expect(component.errorMessage).toBe('Reservation "REF-12345678" not found.');
        expect(component.loading).toBe(false);
        expect(component.reservation).toBeNull();
        done();
      }, 10);
    });

    it('should show generic error for non-404 failures', (done) => {
      hotelService.getReservation.and.returnValue(throwError(() => ({ status: 500 })));

      component.ngOnInit();

      setTimeout(() => {
        expect(component.errorMessage).toBe('Unable to load reservation.');
        expect(component.loading).toBe(false);
        done();
      }, 10);
    });
  });

  describe('error handling logic', () => {
    beforeEach(() => {
      router.getCurrentNavigation.and.returnValue(null);
    });

    it('should include reference number in 404 error message', (done) => {
      const customRef = 'REF-CUSTOM123';
      activatedRoute.snapshot.paramMap.get.and.returnValue(customRef);
      hotelService.getReservation.and.returnValue(throwError(() => ({ status: 404 })));

      component.ngOnInit();

      setTimeout(() => {
        expect(component.errorMessage).toContain(customRef);
        done();
      }, 10);
    });

    it('should distinguish between 404 and other errors', (done) => {
      hotelService.getReservation.and.returnValue(throwError(() => ({ status: 500 })));

      component.ngOnInit();

      setTimeout(() => {
        expect(component.errorMessage).not.toContain('not found');
        expect(component.errorMessage).toBe('Unable to load reservation.');
        done();
      }, 10);
    });
  });
});
