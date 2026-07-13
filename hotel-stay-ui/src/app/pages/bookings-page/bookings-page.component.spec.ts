import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { BookingsPageComponent } from './bookings-page.component';
import { HotelService } from '../../services/hotel.service';
import { ReservationResponse, RoomType, DocumentType } from '../../models';

describe('BookingsPageComponent', () => {
  let component: BookingsPageComponent;
  let fixture: ComponentFixture<BookingsPageComponent>;
  let hotelService: jasmine.SpyObj<HotelService>;

  const mockReservation: ReservationResponse = {
    referenceNumber: 'REF-12345678',
    roomId: '11111111-1111-1111-1111-111111111111',
    destination: 'BOM',
    location: 'India',
    roomType: RoomType.Standard,
    roomCheckIn: '2026-08-01T00:00:00',
    roomCheckOut: '2026-08-05T00:00:00',
    checkIn: '2026-08-01',
    checkOut: '2026-08-05',
    numberOfNights: 4,
    totalPrice: 12000,
    currency: 'INR',
    provider: 'PremierStays',
    document: {
      holderName: 'Jane Smith',
      type: DocumentType.NationalId,
      number: 'ID123456'
    },
    reservationTimestamp: '2026-07-13 09:15:00'
  };

  beforeEach(async () => {
    const hotelServiceSpy = jasmine.createSpyObj('HotelService', ['getReservation']);

    await TestBed.configureTestingModule({
      imports: [BookingsPageComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: HotelService, useValue: hotelServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BookingsPageComponent);
    component = fixture.componentInstance;
    hotelService = TestBed.inject(HotelService) as jasmine.SpyObj<HotelService>;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('form validation', () => {
    it('should initialize with empty form', () => {
      expect(component.form.value.referenceNumber).toBe('');
      expect(component.form.valid).toBe(false);
    });

    it('should require reference number', () => {
      const control = component.form.get('referenceNumber');
      expect(control?.hasError('required')).toBe(true);

      control?.setValue('REF-12345678');
      expect(control?.hasError('required')).toBe(false);
    });

    it('should require minimum 3 characters', () => {
      const control = component.form.get('referenceNumber');
      
      control?.setValue('AB');
      expect(control?.hasError('minlength')).toBe(true);

      control?.setValue('ABC');
      expect(control?.hasError('minlength')).toBe(false);
    });
  });

  describe('onSearch', () => {
    it('should not submit if form is invalid', () => {
      component.form.setValue({ referenceNumber: '' });

      component.onSearch();

      expect(hotelService.getReservation).not.toHaveBeenCalled();
      expect(component.form.touched).toBe(true);
    });

    it('should mark all fields as touched when invalid', () => {
      component.form.setValue({ referenceNumber: 'AB' }); // Too short

      component.onSearch();

      expect(component.form.get('referenceNumber')?.touched).toBe(true);
    });

    it('should call hotel service with reference number when valid', () => {
      hotelService.getReservation.and.returnValue(of(mockReservation));
      component.form.setValue({ referenceNumber: 'REF-12345678' });

      component.onSearch();

      expect(hotelService.getReservation).toHaveBeenCalledWith('REF-12345678');
    });

    it('should set loading state and clear previous results', () => {
      hotelService.getReservation.and.returnValue(of(mockReservation));
      component.form.setValue({ referenceNumber: 'REF-12345678' });
      component.reservation = mockReservation; // Previous result
      component.errorMessage = 'Previous error';

      component.onSearch();

      expect(component.errorMessage).toBeNull();
      expect(hotelService.getReservation).toHaveBeenCalled();
    });

    it('should display reservation on successful search', (done) => {
      hotelService.getReservation.and.returnValue(of(mockReservation));
      component.form.setValue({ referenceNumber: 'REF-12345678' });

      component.onSearch();

      setTimeout(() => {
        expect(component.reservation).toEqual(mockReservation);
        expect(component.loading).toBe(false);
        expect(component.errorMessage).toBeNull();
        done();
      }, 10);
    });

    it('should show "Reservation not found" error for 404 response', (done) => {
      hotelService.getReservation.and.returnValue(throwError(() => ({ status: 404 })));
      component.form.setValue({ referenceNumber: 'REF-INVALID' });

      component.onSearch();

      setTimeout(() => {
        expect(component.errorMessage).toBe('Reservation not found. Please check the reference number.');
        expect(component.reservation).toBeNull();
        expect(component.loading).toBe(false);
        done();
      }, 10);
    });

    it('should show API error detail when available', (done) => {
      const error = { status: 400, error: { detail: 'Invalid format' } };
      hotelService.getReservation.and.returnValue(throwError(() => error));
      component.form.setValue({ referenceNumber: 'INVALID' });

      component.onSearch();

      setTimeout(() => {
        expect(component.errorMessage).toBe('Invalid format');
        done();
      }, 10);
    });

    it('should show generic error message when no detail available', (done) => {
      hotelService.getReservation.and.returnValue(throwError(() => ({ status: 500, error: {} })));
      component.form.setValue({ referenceNumber: 'REF-12345678' });

      component.onSearch();

      setTimeout(() => {
        expect(component.errorMessage).toBe('Unable to retrieve reservation. Please try again.');
        done();
      }, 10);
    });
  });

  describe('error handling logic', () => {
    beforeEach(() => {
      component.form.setValue({ referenceNumber: 'REF-TEST' });
    });

    it('should distinguish between 404 and other errors', (done) => {
      hotelService.getReservation.and.returnValue(throwError(() => ({ status: 500 })));

      component.onSearch();

      setTimeout(() => {
        expect(component.errorMessage).not.toContain('not found');
        expect(component.errorMessage).toContain('Unable to retrieve');
        done();
      }, 10);
    });

    it('should prioritize API error detail over generic message', (done) => {
      const error = { status: 400, error: { detail: 'Specific API error' } };
      hotelService.getReservation.and.returnValue(throwError(() => error));

      component.onSearch();

      setTimeout(() => {
        expect(component.errorMessage).toBe('Specific API error');
        done();
      }, 10);
    });
  });
});

