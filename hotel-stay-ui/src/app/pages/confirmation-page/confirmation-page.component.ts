import { Component, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { HotelService } from '../../services/hotel.service';
import { ReservationResponse } from '../../models';
import { RoomTypePipe } from '../../pipes/room-type.pipe';
import { DocumentTypePipe } from '../../pipes/document-type.pipe';

@Component({
  selector: 'app-confirmation-page',
  standalone: true,
  imports: [CommonModule, RouterLink, RoomTypePipe, DocumentTypePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './confirmation-page.component.html',
  styleUrl: './confirmation-page.component.scss'
})
export class ConfirmationPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly hotelService = inject(HotelService);

  reservation: ReservationResponse | null = null;
  loading = false;
  errorMessage: string | null = null;

  ngOnInit(): void {
    const stateRes = (this.router.getCurrentNavigation()?.extras.state
      ?? history.state) as { reservation?: ReservationResponse };

    if (stateRes?.reservation) {
      this.reservation = stateRes.reservation;
      return;
    }

    const reference = this.route.snapshot.paramMap.get('reference');
    if (!reference) {
      this.router.navigate(['/']);
      return;
    }

    this.loading = true;
    this.hotelService.getReservation(reference).subscribe({
      next: res => {
        this.reservation = res;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = err.status === 404
          ? `Reservation "${reference}" not found.`
          : 'Unable to load reservation.';
      }
    });
  }
}
