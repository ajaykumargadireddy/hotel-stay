import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { SearchPageComponent } from './search-page.component';
import { HotelService } from '../../services/hotel.service';
import { RoomWithDetails, RoomType } from '../../models';

describe('SearchPageComponent', () => {
  let component: SearchPageComponent;
  let fixture: ComponentFixture<SearchPageComponent>;
  let hotelService: jasmine.SpyObj<HotelService>;
  let router: jasmine.SpyObj<Router>;

  const mockRoom: RoomWithDetails = {
    roomId: '11111111-1111-1111-1111-111111111111',
    provider: 'PremierStays',
    destination: 'BOM',
    location: 'India',
    roomType: RoomType.Standard,
    checkIn: '2026-08-01',
    checkOut: '2026-08-05',
    perNightRate: 3000,
    currency: 'INR',
    cancellationPolicy: 'Free Cancellation',
    amenities: ['WiFi', 'AC'],
    starRating: 3,
    numberOfNights: 4,
    totalPrice: 12000
  };

  beforeEach(async () => {
    const hotelServiceSpy = jasmine.createSpyObj('HotelService', ['searchHotels']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [SearchPageComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: HotelService, useValue: hotelServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SearchPageComponent);
    component = fixture.componentInstance;
    hotelService = TestBed.inject(HotelService) as jasmine.SpyObj<HotelService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('onSearch', () => {
    it('should set loading state and call hotel service', () => {
      hotelService.searchHotels.and.returnValue(of([mockRoom]));

      component.onSearch({
        destination: 'BOM',
        checkIn: '2026-08-01',
        checkOut: '2026-08-05',
        countryCode: 'IN',
        cityName: 'Mumbai',
        roomType: undefined
      });

      expect(component.hasSearched).toBe(true);
      expect(hotelService.searchHotels).toHaveBeenCalled();
    });

    it('should populate rooms on successful search', (done) => {
      hotelService.searchHotels.and.returnValue(of([mockRoom]));

      component.onSearch({
        destination: 'BOM',
        checkIn: '2026-08-01',
        checkOut: '2026-08-05',
        countryCode: 'IN',
        cityName: 'Mumbai',
        roomType: undefined
      });

      setTimeout(() => {
        expect(component.rooms.length).toBe(1);
        expect(component.rooms[0]).toEqual(mockRoom);
        expect(component.loading).toBe(false);
        expect(component.errorMessage).toBeNull();
        done();
      }, 10);
    });

    it('should show network error message when status is 0', (done) => {
      const networkError = { status: 0, error: null };
      hotelService.searchHotels.and.returnValue(throwError(() => networkError));

      component.onSearch({
        destination: 'BOM',
        checkIn: '2026-08-01',
        checkOut: '2026-08-05',
        countryCode: 'IN',
        cityName: 'Mumbai',
        roomType: undefined
      });

      setTimeout(() => {
        expect(component.errorMessage).toBe('Cannot reach the server. Check your connection.');
        expect(component.rooms.length).toBe(0);
        expect(component.loading).toBe(false);
        done();
      }, 10);
    });

    it('should show API error detail when available', (done) => {
      const apiError = { status: 400, error: { detail: 'Invalid destination' } };
      hotelService.searchHotels.and.returnValue(throwError(() => apiError));

      component.onSearch({
        destination: 'INVALID',
        checkIn: '2026-08-01',
        checkOut: '2026-08-05',
        countryCode: 'XX',
        cityName: 'Invalid',
        roomType: undefined
      });

      setTimeout(() => {
        expect(component.errorMessage).toBe('Invalid destination');
        expect(component.rooms.length).toBe(0);
        done();
      }, 10);
    });

    it('should show generic error when no detail available', (done) => {
      const genericError = { status: 500, error: {} };
      hotelService.searchHotels.and.returnValue(throwError(() => genericError));

      component.onSearch({
        destination: 'BOM',
        checkIn: '2026-08-01',
        checkOut: '2026-08-05',
        countryCode: 'IN',
        cityName: 'Mumbai',
        roomType: undefined
      });

      setTimeout(() => {
        expect(component.errorMessage).toBe('Unable to search hotels. Please try again.');
        done();
      }, 10);
    });
  });

  describe('onBook', () => {
    beforeEach(() => {
      component['lastSearch'] = {
        destination: 'BOM',
        checkIn: '2026-08-01',
        checkOut: '2026-08-05',
        countryCode: 'IN',
        cityName: 'Mumbai',
        roomType: undefined
      };
    });

    it('should navigate to reservation page with query params and state', () => {
      component.onBook(mockRoom);

      expect(router.navigate).toHaveBeenCalledWith(
        ['/reserve', mockRoom.roomId],
        {
          queryParams: {
            checkIn: '2026-08-01',
            checkOut: '2026-08-05'
          },
          state: {
            room: mockRoom,
            countryCode: 'IN'
          }
        }
      );
    });

    it('should pass room details in navigation state for fast rendering', () => {
      component.onBook(mockRoom);

      const navCall = router.navigate.calls.mostRecent();
      expect(navCall.args[1]?.state?.['room']).toEqual(mockRoom);
    });

    it('should include country code for document validation', () => {
      component.onBook(mockRoom);

      const navCall = router.navigate.calls.mostRecent();
      expect(navCall.args[1]?.state?.['countryCode']).toBe('IN');
    });
  });
});
