import { TestBed } from '@angular/core/testing';
import { HttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { LookupService } from './lookup.service';

describe('LookupService', () => {
  let service: LookupService;
  let httpSpy: jasmine.SpyObj<HttpClient>;

  beforeEach(() => {
    httpSpy = jasmine.createSpyObj('HttpClient', ['get']);
    TestBed.configureTestingModule({
      providers: [
        LookupService,
        { provide: HttpClient, useValue: httpSpy }
      ]
    });
    service = TestBed.inject(LookupService);
  });

  it('getCountries fetches from /lookups/countries', (done) => {
    httpSpy.get.and.returnValue(of([{ code: 'IN', name: 'India' }]));

    service.getCountries().subscribe(countries => {
      expect(countries.length).toBe(1);
      expect(httpSpy.get).toHaveBeenCalledWith('/lookups/countries');
      done();
    });
  });

  it('getCountries caches subsequent calls', (done) => {
    httpSpy.get.and.returnValue(of([{ code: 'IN', name: 'India' }]));

    service.getCountries().subscribe(() => {
      service.getCountries().subscribe(() => {
        expect(httpSpy.get).toHaveBeenCalledTimes(1);
        done();
      });
    });
  });

  it('getCities passes countryCode as query param', (done) => {
    httpSpy.get.and.returnValue(of([{ code: 'BOM', name: 'Mumbai', countryCode: 'IN' }]));

    service.getCities('IN').subscribe(cities => {
      expect(cities.length).toBe(1);
      const opts = httpSpy.get.calls.mostRecent().args[1] as { params: { get: (k: string) => string } };
      expect(opts.params.get('countryCode')).toBe('IN');
      done();
    });
  });

  it('getCities caches per countryCode', (done) => {
    httpSpy.get.and.returnValue(of([{ code: 'BOM', name: 'Mumbai', countryCode: 'IN' }]));

    service.getCities('IN').subscribe(() => {
      service.getCities('IN').subscribe(() => {
        expect(httpSpy.get).toHaveBeenCalledTimes(1);
        done();
      });
    });
  });
});
