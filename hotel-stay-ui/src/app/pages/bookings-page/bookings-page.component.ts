import { Component, ChangeDetectionStrategy, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { HotelService } from '../../services/hotel.service';
import { ReservationResponse } from '../../models';
import { RoomTypePipe } from '../../pipes/room-type.pipe';
import { DocumentTypePipe } from '../../pipes/document-type.pipe';

@Component({
  selector: 'app-bookings-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, RoomTypePipe, DocumentTypePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './bookings-page.component.html',
  styleUrl: './bookings-page.component.scss'
})
export class BookingsPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly hotelService = inject(HotelService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  form = this.fb.group({
    referenceNumber: ['', [Validators.required, Validators.minLength(3)]]
  });

  reservation: ReservationResponse | null = null;
  loading = false;
  errorMessage: string | null = null;

  onSearch(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    this.reservation = null;

    const refNumber = this.form.value.referenceNumber!;

    this.hotelService.getReservation(refNumber).subscribe({
      next: (reservation: ReservationResponse) => {
        this.loading = false;
        this.reservation = reservation;
        this.cdr.markForCheck();
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = err.status === 404
          ? 'Reservation not found. Please check the reference number.'
          : err.error?.detail || 'Unable to retrieve reservation. Please try again.';
        this.cdr.markForCheck();
      }
    });
  }

  clearSearch(): void {
    this.reservation = null;
    this.errorMessage = null;
    this.form.reset();
  }
}
