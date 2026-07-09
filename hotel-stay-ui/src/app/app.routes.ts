import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    title: 'Search Hotels — Hotel Stay',
    loadComponent: () =>
      import('./pages/search-page/search-page.component').then(m => m.SearchPageComponent)
  },
  {
    path: 'bookings',
    title: 'My Bookings — Hotel Stay',
    loadComponent: () =>
      import('./pages/bookings-page/bookings-page.component').then(m => m.BookingsPageComponent)
  },
  {
    path: 'reserve/:roomId',
    title: 'Complete Reservation — Hotel Stay',
    loadComponent: () =>
      import('./pages/reservation-page/reservation-page.component').then(m => m.ReservationPageComponent)
  },
  {
    path: 'confirmation/:reference',
    title: 'Booking Confirmed — Hotel Stay',
    loadComponent: () =>
      import('./pages/confirmation-page/confirmation-page.component').then(m => m.ConfirmationPageComponent)
  },
  { path: '**', redirectTo: '' }
];
