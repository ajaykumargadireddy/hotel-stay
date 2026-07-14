import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { Country, City } from '../models';

@Injectable({ providedIn: 'root' })
export class LookupService {
  private readonly http = inject(HttpClient);

  private countriesCache: Country[] | null = null;
  private citiesCache = new Map<string, City[]>();

  getCountries(): Observable<Country[]> {
    if (this.countriesCache) {
      return of(this.countriesCache);
    }
    return this.http
      .get<Country[]>('/lookups/countries')
      .pipe(tap(countries => (this.countriesCache = countries)));
  }

  getCities(countryCode?: string): Observable<City[]> {
    const key = countryCode ?? '__all__';
    const cached = this.citiesCache.get(key);
    if (cached) {
      return of(cached);
    }

    let params = new HttpParams();
    if (countryCode) {
      params = params.set('countryCode', countryCode);
    }

    return this.http
      .get<City[]>('/lookups/cities', { params })
      .pipe(tap(cities => this.citiesCache.set(key, cities)));
  }
}
