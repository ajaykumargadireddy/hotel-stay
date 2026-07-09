import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../environments/environment';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const isRelative = !/^https?:\/\//i.test(req.url);
  const url = isRelative ? `${environment.apiUrl}${req.url.startsWith('/') ? req.url : '/' + req.url}` : req.url;

  const apiReq = req.clone({
    url,
    setHeaders: {
      'X-Correlation-Id': crypto.randomUUID()
    }
  });

  return next(apiReq);
};
