import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { SearchFormComponent } from './search-form.component';
import { LookupService } from '../../services/lookup.service';
import { Country, City, RoomType } from '../../models';

describe('SearchFormComponent', () => {
  let component: SearchFormComponent;
  let fixture: ComponentFixture<SearchFormComponent>;
  let lookupService: jasmine.SpyObj<LookupService>;

  const mockCountries: Country[] = [
    { code: 'IN', name: 'India' },
    { code: 'US', name: 'United States' }
  ];

  const mockCities: City[] = [
    { code: 'BOM', name: 'Mumbai', countryCode: 'IN' },
    { code: 'DEL', name: 'Delhi', countryCode: 'IN' }
  ];

  beforeEach(async () => {
    const lookupServiceSpy = jasmine.createSpyObj('LookupService', ['getCountries', 'getCities']);

    await TestBed.configureTestingModule({
      imports: [SearchFormComponent, ReactiveFormsModule],
      providers: [
        { provide: LookupService, useValue: lookupServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SearchFormComponent);
    component = fixture.componentInstance;
    lookupService = TestBed.inject(LookupService) as jasmine.SpyObj<LookupService>;

    lookupService.getCountries.and.returnValue(of(mockCountries));
    lookupService.getCities.and.returnValue(of(mockCities));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load countries on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    expect(lookupService.getCountries).toHaveBeenCalled();
    expect(component.countries).toEqual(mockCountries);
  }));

  it('should have city control disabled initially', () => {
    fixture.detectChanges();

    expect(component.form.controls.city.disabled).toBe(true);
  });

  it('should enable city control when country is selected', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    component.form.controls.country.setValue('IN');
    tick();

    expect(lookupService.getCities).toHaveBeenCalledWith('IN');
    expect(component.form.controls.city.disabled).toBe(false);
    expect(component.cities).toEqual(mockCities);
  }));

  it('should disable and reset city control when country is cleared', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    // First select a country
    component.form.controls.country.setValue('IN');
    tick();
    component.form.controls.city.setValue('BOM');

    // Then clear the country
    component.form.controls.country.setValue('');
    tick();

    expect(component.form.controls.city.disabled).toBe(true);
    expect(component.form.controls.city.value).toBe('');
    expect(component.cities).toEqual([]);
  }));

  it('should mark form as invalid when required fields are missing', () => {
    fixture.detectChanges();

    expect(component.form.valid).toBe(false);
  });

  it('should mark form as valid when all required fields are filled', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    component.form.controls.country.setValue('IN');
    tick();

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const dayAfter = new Date();
    dayAfter.setDate(dayAfter.getDate() + 2);

    component.form.patchValue({
      city: 'BOM',
      checkIn: tomorrow.toISOString().split('T')[0],
      checkOut: dayAfter.toISOString().split('T')[0]
    });

    expect(component.form.valid).toBe(true);
  }));

  it('should reject past check-in dates', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    component.form.controls.country.setValue('IN');
    tick();

    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);

    component.form.patchValue({
      city: 'BOM',
      checkIn: yesterday.toISOString().split('T')[0],
      checkOut: tomorrow.toISOString().split('T')[0]
    });

    expect(component.form.controls.checkIn.invalid).toBe(true);
  }));

  it('should reject check-out date before or equal to check-in date', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    component.form.controls.country.setValue('IN');
    tick();

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);

    component.form.patchValue({
      city: 'BOM',
      checkIn: tomorrow.toISOString().split('T')[0],
      checkOut: tomorrow.toISOString().split('T')[0]
    });

    expect(component.form.invalid).toBe(true);
    expect(component.form.errors?.['invalidDateRange']).toBeTruthy();
  }));

  it('should emit search event with correct data on valid submit', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    component.form.controls.country.setValue('IN');
    tick();

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const dayAfter = new Date();
    dayAfter.setDate(dayAfter.getDate() + 2);

    component.form.patchValue({
      city: 'BOM',
      checkIn: tomorrow.toISOString().split('T')[0],
      checkOut: dayAfter.toISOString().split('T')[0],
      roomType: RoomType.Deluxe
    });

    let emittedData: any;
    component.search.subscribe(data => emittedData = data);

    component.onSubmit();

    expect(emittedData).toBeDefined();
    expect(emittedData.destination).toBe('BOM');
    expect(emittedData.countryCode).toBe('IN');
    expect(emittedData.cityName).toBe('Mumbai');
    expect(emittedData.roomType).toBe(RoomType.Deluxe);
  }));

  it('should not emit search event when form is invalid', () => {
    fixture.detectChanges();

    let emitted = false;
    component.search.subscribe(() => emitted = true);

    component.onSubmit();

    expect(emitted).toBe(false);
  });

  it('should mark all fields as touched when submitting invalid form', () => {
    fixture.detectChanges();

    component.onSubmit();

    expect(component.form.controls.country.touched).toBe(true);
    expect(component.form.controls.city.touched).toBe(true);
    expect(component.form.controls.checkIn.touched).toBe(true);
    expect(component.form.controls.checkOut.touched).toBe(true);
  });

  it('should calculate minCheckOut based on checkIn date', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const expectedMin = tomorrow.toISOString().split('T')[0];

    component.form.controls.checkIn.setValue(expectedMin);

    expect(component.minCheckOut).toBe(expectedMin);
  }));

  it('should use today as minCheckOut when checkIn is not set', () => {
    fixture.detectChanges();

    expect(component.minCheckOut).toBe(component.today);
  });

  it('should handle optional roomType field', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    component.form.controls.country.setValue('IN');
    tick();

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const dayAfter = new Date();
    dayAfter.setDate(dayAfter.getDate() + 2);

    component.form.patchValue({
      city: 'BOM',
      checkIn: tomorrow.toISOString().split('T')[0],
      checkOut: dayAfter.toISOString().split('T')[0]
      // roomType not set
    });

    let emittedData: any;
    component.search.subscribe(data => emittedData = data);

    component.onSubmit();

    expect(emittedData.roomType).toBeUndefined();
  }));
});
