import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Observable, finalize } from 'rxjs';

import { AuthService } from '../services/auth.service';
import { LoadingService } from '../services/loading.service';
import { environment } from '../../../environments/environment';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private readonly authService: AuthService,
    private readonly loadingService: LoadingService,
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.authService.getToken();
    const shouldAttachToken = request.url.startsWith(environment.apiBaseUrl);
    const authRequest = token && shouldAttachToken
      ? request.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`,
          },
        })
      : request;

    this.loadingService.show();

    return next.handle(authRequest).pipe(finalize(() => this.loadingService.hide()));
  }
}
