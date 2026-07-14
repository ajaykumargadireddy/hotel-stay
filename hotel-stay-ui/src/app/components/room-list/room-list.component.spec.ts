import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RoomListComponent } from './room-list.component';
import { RoomCardComponent } from '../room-card/room-card.component';
import { RoomType, RoomWithDetails } from '../../models';
import { DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';

describe('RoomListComponent', () => {
  let component: RoomListComponent;
  let fixture: ComponentFixture<RoomListComponent>;

  const mockRooms: RoomWithDetails[] = [
    {
      roomId: 'room-1',
      provider: 'PremierStays',
      destination: 'BOM',
      location: 'Mumbai, India',
      roomType: RoomType.Standard,
      checkIn: '2026-08-01',
      checkOut: '2026-08-04',
      perNightRate: 100,
      currency: 'USD',
      numberOfNights: 3,
      totalPrice: 300,
      cancellationPolicy: 'Free cancellation',
      starRating: 3,
      amenities: ['WiFi']
    },
    {
      roomId: 'room-2',
      provider: 'BudgetNests',
      destination: 'BOM',
      location: 'Mumbai, India',
      roomType: RoomType.Deluxe,
      checkIn: '2026-08-01',
      checkOut: '2026-08-04',
      perNightRate: 150,
      currency: 'USD',
      numberOfNights: 3,
      totalPrice: 450,
      cancellationPolicy: 'Non-refundable',
      starRating: 4,
      amenities: ['WiFi', 'Pool']
    },
    {
      roomId: 'room-3',
      provider: 'PremierStays',
      destination: 'BOM',
      location: 'Mumbai, India',
      roomType: RoomType.Suite,
      checkIn: '2026-08-01',
      checkOut: '2026-08-04',
      perNightRate: 200,
      currency: 'USD',
      numberOfNights: 3,
      totalPrice: 600,
      cancellationPolicy: 'Flexible',
      starRating: 5,
      amenities: ['WiFi', 'Pool', 'Spa']
    }
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RoomListComponent, RoomCardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(RoomListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default sortOrder to ascending', () => {
    expect(component.sortOrder).toBe('asc');
  });

  it('should sort rooms by total price in ascending order', () => {
    component.rooms = mockRooms;
    fixture.detectChanges();

    const sorted = component.sortedRooms;

    expect(sorted[0].totalPrice).toBe(300);
    expect(sorted[1].totalPrice).toBe(450);
    expect(sorted[2].totalPrice).toBe(600);
  });

  it('should sort rooms by total price in descending order', () => {
    component.rooms = mockRooms;
    component.sortOrder = 'desc';
    fixture.detectChanges();

    const sorted = component.sortedRooms;

    expect(sorted[0].totalPrice).toBe(600);
    expect(sorted[1].totalPrice).toBe(450);
    expect(sorted[2].totalPrice).toBe(300);
  });

  it('should toggle sort order from asc to desc', () => {
    component.sortOrder = 'asc';

    component.toggleSort();

    expect(component.sortOrder).toBe('desc');
  });

  it('should toggle sort order from desc to asc', () => {
    component.sortOrder = 'desc';

    component.toggleSort();

    expect(component.sortOrder).toBe('asc');
  });

  it('should not mutate original rooms array when sorting', () => {
    component.rooms = mockRooms;
    fixture.detectChanges();

    const originalOrder = [...mockRooms];
    const sorted = component.sortedRooms;

    expect(component.rooms).toEqual(originalOrder);
    expect(sorted).not.toBe(component.rooms);
  });

  it('should emit book event when room card emits book', () => {
    component.rooms = [mockRooms[0]];
    fixture.detectChanges();

    let emittedRoom: RoomWithDetails | undefined;
    component.book.subscribe(room => emittedRoom = room);

    const roomCard = fixture.debugElement.query(By.directive(RoomCardComponent));
    roomCard.componentInstance.book.emit(mockRooms[0]);

    expect(emittedRoom).toBe(mockRooms[0]);
  });

  it('should track rooms by roomId', () => {
    const room = mockRooms[0];
    const trackResult = component.trackByRoomId(0, room);

    expect(trackResult).toBe('room-1');
  });

  it('should display loading state when loading is true', () => {
    component.loading = true;
    component.hasSearched = true;
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Searching');
  });

  it('should display empty state when no rooms and search has been performed', () => {
    component.rooms = [];
    component.loading = false;
    component.hasSearched = true;
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('No rooms available');
  });

  it('should not display empty state when search has not been performed', () => {
    component.rooms = [];
    component.loading = false;
    component.hasSearched = false;
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).not.toContain('No rooms available');
    expect(compiled.textContent).not.toContain('Searching');
  });

  it('should render room cards when rooms are available', () => {
    component.rooms = mockRooms;
    component.hasSearched = true;
    fixture.detectChanges();

    const roomCards = fixture.debugElement.queryAll(By.directive(RoomCardComponent));
    expect(roomCards.length).toBe(3);
  });

  it('should pass correct room data to each room card', () => {
    component.rooms = mockRooms;
    fixture.detectChanges();

    const roomCards = fixture.debugElement.queryAll(By.directive(RoomCardComponent));
    
    expect(roomCards[0].componentInstance.room.totalPrice).toBe(300);
    expect(roomCards[1].componentInstance.room.totalPrice).toBe(450);
    expect(roomCards[2].componentInstance.room.totalPrice).toBe(600);
  });

  it('should handle empty rooms array without errors', () => {
    component.rooms = [];
    fixture.detectChanges();

    expect(component.sortedRooms).toEqual([]);
  });

  it('should update sorted rooms when rooms input changes', () => {
    component.rooms = [mockRooms[0]];
    fixture.detectChanges();

    expect(component.sortedRooms.length).toBe(1);

    component.rooms = mockRooms;
    fixture.detectChanges();

    expect(component.sortedRooms.length).toBe(3);
  });

  it('should maintain sort order when rooms change', () => {
    component.rooms = mockRooms;
    component.sortOrder = 'desc';
    fixture.detectChanges();

    const sorted = component.sortedRooms;
    expect(sorted[0].totalPrice).toBe(600);

    // Add a new room
    const newRooms = [...mockRooms, {
      roomId: 'room-4',
      provider: 'PremierStays',
      destination: 'BOM',
      location: 'Mumbai, India',
      roomType: RoomType.Standard,
      checkIn: '2026-08-01',
      checkOut: '2026-08-04',
      perNightRate: 120,
      currency: 'USD',
      numberOfNights: 3,
      totalPrice: 360,
      cancellationPolicy: 'Free cancellation',
      starRating: 3,
      amenities: []
    }];

    component.rooms = newRooms;
    fixture.detectChanges();

    const newSorted = component.sortedRooms;
    expect(newSorted[0].totalPrice).toBe(600);
    expect(component.sortOrder).toBe('desc');
  });
});
