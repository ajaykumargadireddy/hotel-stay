import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HotelService } from '../../services/hotel.service';
import { RoomWithDetails, HotelSearchRequest } from '../../models';
import { SearchFormComponent } from '../../components/search-form/search-form.component';
import { RoomListComponent } from '../../components/room-list/room-list.component';
import { HttpErrorResponse } from '@angular/common/http';

type SearchEvent = HotelSearchRequest & { countryCode: string; cityName: string };

@Component({
  selector: 'app-search-page',
  standalone: true,
  imports: [CommonModule, SearchFormComponent, RoomListComponent],
  templateUrl: './search-page.component.html',
  styleUrl: './search-page.component.scss'
})
export class SearchPageComponent {
  private readonly hotelService = inject(HotelService);
  private readonly router = inject(Router);

  rooms: RoomWithDetails[] = [];
  loading = false;
  hasSearched = false;
  errorMessage: string | null = null;

  private lastSearch: SearchEvent | null = null;

  onSearch(request: SearchEvent): void {
    this.loading = true;
    this.errorMessage = null;
    this.hasSearched = true;
    this.lastSearch = request;

    this.hotelService.searchHotels(request).subscribe({
      next: rooms => {
        this.rooms = rooms;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.rooms = [];
        this.errorMessage = err.status === 0
          ? 'Cannot reach the server. Check your connection.'
          : err.error?.detail || 'Unable to search hotels. Please try again.';
      }
    });
  }

  onBook(room: RoomWithDetails): void {
    this.router.navigate(['/reserve', room.roomId], {
      state: {
        room,
        countryCode: this.lastSearch?.countryCode,
        checkIn: this.lastSearch?.checkIn,
        checkOut: this.lastSearch?.checkOut
      }
    });
  }
}
