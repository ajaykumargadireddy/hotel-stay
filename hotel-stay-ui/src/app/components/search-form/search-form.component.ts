import { Component, EventEmitter, Output, OnInit, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LookupService } from '../../services/lookup.service';
import { Country, City, RoomType, HotelSearchRequest } from '../../models';
import { dateRangeValidator } from '../../validators/date-range.validator';
import { pastDateValidator } from '../../validators/past-date.validator';

@Component({
  selector: 'app-search-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './search-form.component.html',
  styleUrl: './search-form.component.scss'
})
export class SearchFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly lookup = inject(LookupService);
  private readonly cdr = inject(ChangeDetectorRef);

  @Output() search = new EventEmitter<HotelSearchRequest & { countryCode: string; cityName: string }>();

  countries: Country[] = [];
  cities: City[] = [];
  roomTypes: RoomType[] = [RoomType.Standard, RoomType.Deluxe, RoomType.Suite];

  readonly today = new Date().toISOString().split('T')[0];

  form = this.fb.group(
    {
      country: ['', Validators.required],
      city: [{ value: '', disabled: true }, Validators.required],
      checkIn: ['', [Validators.required, pastDateValidator()]],
      checkOut: ['', Validators.required],
      roomType: ['']
    },
    { validators: [dateRangeValidator()] }
  );

  ngOnInit(): void {
    this.lookup.getCountries().subscribe(countries => {
      this.countries = countries;
      this.cdr.markForCheck();
    });

    this.form.controls.country.valueChanges.subscribe(countryCode => {
      const cityControl = this.form.controls.city;
      cityControl.reset('');
      if (countryCode) {
        this.lookup.getCities(countryCode).subscribe(cities => {
          this.cities = cities;
          cityControl.enable();
          this.cdr.markForCheck();
        });
      } else {
        this.cities = [];
        cityControl.disable();
        this.cdr.markForCheck();
      }
    });
  }

  get minCheckOut(): string {
    return this.form.controls.checkIn.value || this.today;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const selectedCity = this.cities.find(c => c.code === raw.city);
    const cityName = selectedCity?.name ?? raw.city!;

    this.search.emit({
      destination: raw.city!,
      checkIn: raw.checkIn!,
      checkOut: raw.checkOut!,
      roomType: (raw.roomType as RoomType) || undefined,
      countryCode: raw.country!,
      cityName
    });
  }
}
