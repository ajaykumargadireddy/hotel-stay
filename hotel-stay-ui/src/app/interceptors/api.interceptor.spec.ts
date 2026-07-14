import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { apiInterceptor } from './api.interceptor';
import { environment } from '../../environments/environment';

describe('apiInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let router: jasmine.SpyObj<Router>;
  let consoleErrorSpy: jasmine.Spy;
  let consoleWarnSpy: jasmine.Spy;

  beforeEach(() => {
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    consoleErrorSpy = spyOn(console, 'error');
    consoleWarnSpy = spyOn(console, 'warn');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: routerSpy }
      ]
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('URL handling', () => {
    it('should prepend base URL to relative URLs', () => {
      httpClient.get('/test-endpoint').subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/test-endpoint`);
      expect(req.request.url).toBe(`${environment.apiUrl}/test-endpoint`);
      req.flush({});
    });

    it('should prepend base URL to relative URLs without leading slash', () => {
      httpClient.get('test-endpoint').subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/test-endpoint`);
      expect(req.request.url).toBe(`${environment.apiUrl}/test-endpoint`);
      req.flush({});
    });

    it('should not modify absolute URLs', () => {
      const absoluteUrl = 'https://external-api.com/data';
      httpClient.get(absoluteUrl).subscribe();

      const req = httpMock.expectOne(absoluteUrl);
      expect(req.request.url).toBe(absoluteUrl);
      req.flush({});
    });
  });

  describe('Correlation ID', () => {
    it('should add X-Correlation-Id header to requests', () => {
      httpClient.get('/test').subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/test`);
      expect(req.request.headers.has('X-Correlation-Id')).toBe(true);
      expect(req.request.headers.get('X-Correlation-Id')).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i);
      req.flush({});
    });

    it('should generate unique correlation IDs for each request', () => {
      httpClient.get('/test1').subscribe();
      httpClient.get('/test2').subscribe();

      const req1 = httpMock.expectOne(`${environment.apiUrl}/test1`);
      const req2 = httpMock.expectOne(`${environment.apiUrl}/test2`);

      const correlationId1 = req1.request.headers.get('X-Correlation-Id');
      const correlationId2 = req2.request.headers.get('X-Correlation-Id');

      expect(correlationId1).not.toBe(correlationId2);
      req1.flush({});
      req2.flush({});
    });
  });

  describe('Retry logic', () => {
    it('should retry on network errors (status 0)', fakeAsync(() => {
      let attemptCount = 0;
      let errorReceived = false;

      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(attemptCount).toBe(environment.api.retryCount + 1); // Initial + retries
          expect(error.status).toBe(0);
          errorReceived = true;
        }
      });

      // Initial request
      const req1 = httpMock.expectOne(`${environment.apiUrl}/test`);
      attemptCount++;
      req1.error(new ProgressEvent('error'), { status: 0 });

      // Handle retries with delays
      for (let i = 1; i <= environment.api.retryCount; i++) {
        tick(Math.pow(2, i) * environment.api.retryDelayMs);
        const req = httpMock.expectOne(`${environment.apiUrl}/test`);
        attemptCount++;
        req.error(new ProgressEvent('error'), { status: 0 });
      }

      tick();
      expect(errorReceived).toBe(true);
    }));

    it('should retry on 500 server errors', fakeAsync(() => {
      let attemptCount = 0;
      let errorReceived = false;

      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(attemptCount).toBe(environment.api.retryCount + 1);
          expect(error.status).toBe(500);
          errorReceived = true;
        }
      });

      // Initial request
      const req1 = httpMock.expectOne(`${environment.apiUrl}/test`);
      attemptCount++;
      req1.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

      // Handle retries with delays
      for (let i = 1; i <= environment.api.retryCount; i++) {
        tick(Math.pow(2, i) * environment.api.retryDelayMs);
        const req = httpMock.expectOne(`${environment.apiUrl}/test`);
        attemptCount++;
        req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
      }

      tick();
      expect(errorReceived).toBe(true);
    }));

    it('should retry on 503 service unavailable errors', fakeAsync(() => {
      let attemptCount = 0;
      let errorReceived = false;

      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(attemptCount).toBe(environment.api.retryCount + 1);
          expect(error.status).toBe(503);
          errorReceived = true;
        }
      });

      // Initial request
      const req1 = httpMock.expectOne(`${environment.apiUrl}/test`);
      attemptCount++;
      req1.flush('Service Unavailable', { status: 503, statusText: 'Service Unavailable' });

      // Handle retries with delays
      for (let i = 1; i <= environment.api.retryCount; i++) {
        tick(Math.pow(2, i) * environment.api.retryDelayMs);
        const req = httpMock.expectOne(`${environment.apiUrl}/test`);
        attemptCount++;
        req.flush('Service Unavailable', { status: 503, statusText: 'Service Unavailable' });
      }

      tick();
      expect(errorReceived).toBe(true);
    }));

    it('should NOT retry on 404 client errors', (done) => {
      let attemptCount = 0;

      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(attemptCount).toBe(1); // No retries
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/test`);
      attemptCount++;
      req.flush('Not Found', { status: 404, statusText: 'Not Found' });
    });

    it('should NOT retry on 400 bad request errors', (done) => {
      let attemptCount = 0;

      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(attemptCount).toBe(1);
          expect(error.status).toBe(400);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/test`);
      attemptCount++;
      req.flush('Bad Request', { status: 400, statusText: 'Bad Request' });
    });

    it('should NOT retry on 422 validation errors', (done) => {
      let attemptCount = 0;

      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(attemptCount).toBe(1);
          expect(error.status).toBe(422);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/test`);
      attemptCount++;
      req.flush('Validation Error', { status: 422, statusText: 'Unprocessable Entity' });
    });

    it('should succeed on retry if server recovers', fakeAsync(() => {
      let attemptCount = 0;
      let responseReceived = false;

      httpClient.get('/test').subscribe({
        next: (response) => {
          expect(attemptCount).toBe(2); // Failed once, succeeded on retry
          expect(response).toEqual({ success: true });
          responseReceived = true;
        },
        error: () => fail('should have succeeded')
      });

      // First attempt fails
      const req1 = httpMock.expectOne(`${environment.apiUrl}/test`);
      attemptCount++;
      req1.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

      // Wait for retry delay
      tick(Math.pow(2, 1) * environment.api.retryDelayMs);

      // Retry succeeds
      const req2 = httpMock.expectOne(`${environment.apiUrl}/test`);
      attemptCount++;
      req2.flush({ success: true });

      tick();
      expect(responseReceived).toBe(true);
    }));
  });

  describe('Error logging', () => {
    it('should log API errors with details', fakeAsync(() => {
      let errorReceived = false;

      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: () => {
          expect(consoleErrorSpy).toHaveBeenCalledWith(
            '[API Error]',
            jasmine.objectContaining({
              url: '/test',
              method: 'GET',
              status: 500,
              message: jasmine.any(String),
              correlationId: jasmine.any(String)
            })
          );
          errorReceived = true;
        }
      });

      // Initial request
      const req1 = httpMock.expectOne(`${environment.apiUrl}/test`);
      req1.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

      // Handle retries
      for (let i = 1; i <= environment.api.retryCount; i++) {
        tick(Math.pow(2, i) * environment.api.retryDelayMs);
        const req = httpMock.expectOne(`${environment.apiUrl}/test`);
        req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
      }

      tick();
      expect(errorReceived).toBe(true);
    }));

    it('should log warning for 401 unauthorized errors', (done) => {
      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: () => {
          expect(consoleWarnSpy).toHaveBeenCalledWith('Unauthorized access attempt');
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/test`);
      req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
    });

    it('should log warning for 403 forbidden errors', (done) => {
      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: () => {
          expect(consoleWarnSpy).toHaveBeenCalledWith('Unauthorized access attempt');
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/test`);
      req.flush('Forbidden', { status: 403, statusText: 'Forbidden' });
    });

    it('should log network errors', fakeAsync(() => {
      let errorReceived = false;

      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: () => {
          expect(consoleErrorSpy).toHaveBeenCalledWith('Network error - API may be unavailable');
          errorReceived = true;
        }
      });

      // Initial request
      const req1 = httpMock.expectOne(`${environment.apiUrl}/test`);
      req1.error(new ProgressEvent('error'), { status: 0 });

      // Handle retries
      for (let i = 1; i <= environment.api.retryCount; i++) {
        tick(Math.pow(2, i) * environment.api.retryDelayMs);
        const req = httpMock.expectOne(`${environment.apiUrl}/test`);
        req.error(new ProgressEvent('error'), { status: 0 });
      }

      tick();
      expect(errorReceived).toBe(true);
    }));
  });

  describe('Error propagation', () => {
    it('should re-throw errors for component-level handling', (done) => {
      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: (error: HttpErrorResponse) => {
          expect(error).toBeInstanceOf(HttpErrorResponse);
          expect(error.status).toBe(404);
          expect(error.statusText).toBe('Not Found');
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/test`);
      req.flush('Not Found', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('Request methods', () => {
    it('should work with POST requests', () => {
      const payload = { name: 'test' };

      httpClient.post('/test', payload).subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/test`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(payload);
      req.flush({ success: true });
    });

    it('should work with PUT requests', () => {
      const payload = { id: 1, name: 'updated' };

      httpClient.put('/test/1', payload).subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/test/1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(payload);
      req.flush({ success: true });
    });

    it('should work with DELETE requests', () => {
      httpClient.delete('/test/1').subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/test/1`);
      expect(req.request.method).toBe('DELETE');
      req.flush({ success: true });
    });
  });

  describe('Configuration', () => {
    it('should use retry count from environment config', fakeAsync(() => {
      let attemptCount = 0;
      let errorReceived = false;
      const expectedAttempts = environment.api.retryCount + 1;

      httpClient.get('/test').subscribe({
        next: () => fail('should have failed'),
        error: () => {
          expect(attemptCount).toBe(expectedAttempts);
          errorReceived = true;
        }
      });

      // Initial request
      const req1 = httpMock.expectOne(`${environment.apiUrl}/test`);
      attemptCount++;
      req1.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

      // Handle retries
      for (let i = 1; i < expectedAttempts; i++) {
        tick(Math.pow(2, i) * environment.api.retryDelayMs);
        const req = httpMock.expectOne(`${environment.apiUrl}/test`);
        attemptCount++;
        req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
      }

      tick();
      expect(errorReceived).toBe(true);
    }));
  });
});
