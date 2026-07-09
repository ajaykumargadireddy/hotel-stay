import { Component, ChangeDetectionStrategy, ChangeDetectorRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { HotelService } from '../../services/hotel.service';
import { RoomWithDetails, ReservationRequest, ReservationResponse } from '../../models';
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
  errorMessage: string | null = null;

  ngOnInit(): void {
    const nav = this.router.getCurrentNavigation()?.extras.state
      ?? (history.state as { room?: RoomWithDetails; countryCode?: string; checkIn?: string; checkOut?: string });

    if (nav?.room && nav?.checkIn && nav?.checkOut) {
      this.room = nav.room;
      this.checkIn = nav.checkIn;
      this.checkOut = nav.checkOut;
      this.isInternational = isInternationalCountry(nav.countryCode);
    } else {
      // No room in state — redirect back to search
      this.router.navigate(['/']);
    }
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
}
