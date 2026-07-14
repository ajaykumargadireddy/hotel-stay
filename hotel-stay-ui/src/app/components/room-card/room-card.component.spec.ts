import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RoomCardComponent } from './room-card.component';
import { RoomTypePipe } from '../../pipes/room-type.pipe';
import { RoomType, RoomWithDetails } from '../../models';
import { DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';

describe('RoomCardComponent', () => {
  let component: RoomCardComponent;
  let fixture: ComponentFixture<RoomCardComponent>;

  const mockRoom: RoomWithDetails = {
    roomId: 'room-123',
    provider: 'PremierStays',
    destination: 'BOM',
    location: 'Mumbai, India',
    roomType: RoomType.Deluxe,
    checkIn: '2026-08-01',
    checkOut: '2026-08-04',
    perNightRate: 150,
    currency: 'USD',
    numberOfNights: 3,
    totalPrice: 450,
    cancellationPolicy: 'Free cancellation up to 48h before check-in',
    starRating: 4,
    amenities: ['WiFi', 'Pool', 'Breakfast']
  };

  const mockRoomWithoutStars: RoomWithDetails = {
    roomId: 'room-456',
    provider: 'BudgetNests',
    destination: 'BOM',
    location: 'Mumbai, India',
    roomType: RoomType.Standard,
    checkIn: '2026-08-01',
    checkOut: '2026-08-04',
    perNightRate: 100,
    currency: 'USD',
    numberOfNights: 3,
    totalPrice: 300,
    cancellationPolicy: 'Non-refundable',
    starRating: undefined,
    amenities: []
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RoomCardComponent, RoomTypePipe]
    }).compileComponents();

    fixture = TestBed.createComponent(RoomCardComponent);
    component = fixture.componentInstance;
    component.room = mockRoom;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display room provider', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('PremierStays');
  });

  it('should display room type using pipe', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    // The pipe should transform RoomType.Deluxe to a display name
    expect(compiled.textContent).toContain('Deluxe');
  });

  it('should display price per night', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('150');
  });

  it('should display total price', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('450');
  });

  it('should display cancellation policy', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Free cancellation up to 48h before check-in');
  });

  it('should generate correct stars array for star rating', () => {
    component.room = mockRoom;

    const stars = component.stars;

    expect(stars.length).toBe(4);
    expect(stars).toEqual([0, 1, 2, 3]);
  });

  it('should return empty array when star rating is undefined', () => {
    component.room = mockRoomWithoutStars;

    const stars = component.stars;

    expect(stars).toEqual([]);
  });

  it('should return empty array when star rating is null', () => {
    component.room = { ...mockRoom, starRating: null as any };

    const stars = component.stars;

    expect(stars).toEqual([]);
  });

  it('should emit book event when book button is clicked', () => {
    fixture.detectChanges();

    let emittedRoom: RoomWithDetails | undefined;
    component.book.subscribe(room => emittedRoom = room);

    component.onBook();

    expect(emittedRoom).toBe(mockRoom);
  });

  it('should handle room without amenities', () => {
    component.room = mockRoomWithoutStars;
    fixture.detectChanges();

    expect(component.room.amenities).toEqual([]);
  });

  it('should display amenities when available', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    mockRoom.amenities?.forEach(amenity => {
      expect(compiled.textContent).toContain(amenity);
    });
  });

  it('should handle rooms with different star ratings', () => {
    const fiveStarRoom = { ...mockRoom, starRating: 5 };
    component.room = fiveStarRoom;

    expect(component.stars.length).toBe(5);

    const twoStarRoom = { ...mockRoom, starRating: 2 };
    component.room = twoStarRoom;

    expect(component.stars.length).toBe(2);

    const zeroStarRoom = { ...mockRoom, starRating: 0 };
    component.room = zeroStarRoom;

    expect(component.stars.length).toBe(0);
  });

  it('should have required room input', () => {
    // This tests that the component properly requires the room input
    expect(component.room).toBeDefined();
  });

  it('should render all room details in template', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement;

    // Check that all key information is rendered
    expect(compiled.textContent).toContain('PremierStays');
    expect(compiled.textContent).toContain('Deluxe');
    expect(compiled.textContent).toContain('150');
    expect(compiled.textContent).toContain('450');
    expect(compiled.textContent).toContain('Free cancellation');
  });

  it('should handle different providers', () => {
    const budgetRoom = { ...mockRoom, provider: 'BudgetNests' };
    component.room = budgetRoom;
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('BudgetNests');
  });

  it('should handle different room types', () => {
    const suiteRoom = { ...mockRoom, roomType: RoomType.Suite };
    component.room = suiteRoom;
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Suite');
  });

  it('should emit the exact room instance on book', () => {
    let emittedRoom: RoomWithDetails | undefined;
    component.book.subscribe(room => emittedRoom = room);

    component.onBook();

    expect(emittedRoom).toBe(component.room);
    expect(emittedRoom).toBe(mockRoom);
  });
});
