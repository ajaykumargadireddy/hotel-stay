import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, retry, timer } from 'rxjs';
import { environment } from '../../environments/environment';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  
  const isRelative = !/^https?:\/\//i.test(req.url);
  const url = isRelative ? `${environment.apiUrl}${req.url.startsWith('/') ? req.url : '/' + req.url}` : req.url;

  const apiReq = req.clone({
    url,
    setHeaders: {
      'X-Correlation-Id': crypto.randomUUID()
    }
  });

  return next(apiReq).pipe(
    retry({
      count: environment.api.retryCount,
      delay: (error, retryCount) => {
        // Only retry on network errors or 5xx server errors
        if (error instanceof HttpErrorResponse) {
          const shouldRetry = error.status === 0 || error.status >= 500;
          if (shouldRetry) {
            // Exponential backoff: 2s, 4s, 8s...
            return timer(Math.pow(2, retryCount) * environment.api.retryDelayMs);
          }
        }
        throw error;
      }
    }),
    catchError((error: HttpErrorResponse) => {
      // Centralized error logging
      console.error('[API Error]', {
        url: req.url,
        method: req.method,
        status: error.status,
        message: error.message,
        correlationId: apiReq.headers.get('X-Correlation-Id')
      });

      // Handle specific error scenarios
      if (error.status === 401 || error.status === 403) {
        // Unauthorized - could redirect to login if auth is implemented
        console.warn('Unauthorized access attempt');
      }

      if (error.status === 0) {
        // Network error
        console.error('Network error - API may be unavailable');
      }

      // Re-throw the error for component-level handling
      throw error;
    })
  );
};
