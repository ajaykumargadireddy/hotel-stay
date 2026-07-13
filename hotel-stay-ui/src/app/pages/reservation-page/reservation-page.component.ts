import { Component, ChangeDetectionStrategy, ChangeDetectorRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { HotelService } from '../../services/hotel.service';
import { RoomWithDetails, ReservationRequest, ReservationResponse, RoomDetails } from '../../models';
import { ReservationFormComponent } from '../../components/reservation-form/reservation-form.component';
import { isInternationalCountry } from '../../validators/document-type.validator';
import { RoomTypePipe } from '../../pipes/room-type.pipe';

@Component({
  selector: 'app-reservation-page',
  standalone: true,
  imports: [CommonModule, ReservationFormComponent, RoomTypePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './reservation-page.component.html',
  styleUrl: './reservation-page.component.scss'
})
export class ReservationPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly hotelService = inject(HotelService);
  private readonly cdr = inject(ChangeDetectorRef);

  room: RoomWithDetails | null = null;
  checkIn: string = '';
  checkOut: string = '';
  isInternational = false;
  submitting = false;
  loading = false;
  errorMessage: string | null = null;

  ngOnInit(): void {
    // Get dates from query params (supports deep-linking)
    this.route.queryParams.subscribe(params => {
      this.checkIn = params['checkIn'] || '';
      this.checkOut = params['checkOut'] || '';

      // Validate required query params
      if (!this.checkIn || !this.checkOut) {
        this.router.navigate(['/']);
        return;
      }

      // Try to get room from navigation state first (fast path)
      const state = this.router.getCurrentNavigation()?.extras.state ?? history.state;

      if (state?.room) {
        // Fast path: room data available in state
        this.room = state.room;
        this.isInternational = isInternationalCountry(state.countryCode);
        this.cdr.detectChanges();
      } else {
        // Fallback: fetch room by ID from API (supports deep-linking)
        this.fetchRoomDetails();
      }
    });
  }

  private fetchRoomDetails(): void {
    const roomId = this.route.snapshot.params['roomId'];
    
    if (!roomId) {
      this.router.navigate(['/']);
      return;
    }

    this.loading = true;
    this.hotelService.getRoomById(roomId).subscribe({
      next: (roomDetails: RoomDetails) => {
        // Calculate nights and total price
        const checkInDate = new Date(this.checkIn);
        const checkOutDate = new Date(this.checkOut);
        const nights = Math.ceil((checkOutDate.getTime() - checkInDate.getTime()) / (1000 * 60 * 60 * 24));
        
        this.room = {
          roomId: roomDetails.roomId,
          provider: roomDetails.provider,
          destination: roomDetails.destination,
          location: roomDetails.location,
          roomType: roomDetails.roomType as any,
          checkIn: this.checkIn,
          checkOut: this.checkOut,
          perNightRate: roomDetails.perNightRate,
          currency: roomDetails.currency,
          cancellationPolicy: roomDetails.cancellationPolicy,
          amenities: roomDetails.amenities,
          starRating: roomDetails.starRating,
          numberOfNights: nights,
          totalPrice: roomDetails.perNightRate * nights
        };
        
        this.isInternational = roomDetails.location !== 'India';
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = err.status === 404 
          ? 'Room not found. Please search again.' 
          : 'Unable to load room details.';
        this.cdr.detectChanges();
      }
    });
  }

  onReserve(request: ReservationRequest): void {
    this.submitting = true;
    this.errorMessage = null;

    this.hotelService.reserveRoom(request).subscribe({
      next: (reference: string) => {
        this.hotelService.getReservation(reference).subscribe({
          next: (reservation: ReservationResponse) => {
            this.submitting = false;
            this.router.navigate(['/confirmation', reference], {
              state: { reservation }
            });
            this.cdr.detectChanges();
          },
          error: () => {
            this.submitting = false;
            this.router.navigate(['/confirmation', reference]);
            this.cdr.detectChanges();
          }
        });
      },
      error: (err: HttpErrorResponse) => {
        this.submitting = false;
        this.errorMessage = err.error?.detail
          || (err.status === 422 ? 'Document validation failed.' : 'Unable to complete reservation.');
      }
    });
  }

  goToSearch(): void {
    this.router.navigate(['/']);
  }
}
