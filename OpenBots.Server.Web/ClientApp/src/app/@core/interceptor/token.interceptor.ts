import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor, HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { NbToastrService } from '@nebular/theme';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';


@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  constructor(
    private toastrService: NbToastrService,
    private authService: AuthService,
    private router: Router
  ) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('accessToken');
    if (token) {
      request = this.attachToken(request, token);
    }
    return next.handle(request).pipe(
      catchError((error) => {
        if (error.status == 401) {
          if (error.error != null) {
            this.toastrService.danger('Your Credentials are wrong', 'Failed');
          }
          if (error.error == null) {
            return this.handleError(request, next);
          }
        }
        if (error.status != 401) {
          return this.handleErrorGlobal(error);
        }
      })
    );
  }

  handleError(request: HttpRequest<unknown>, next: HttpHandler) {
    return this.authService.refreshToken().pipe(
      switchMap((token: any) => {
        localStorage.setItem('accessToken', token.jwt);
        localStorage.setItem('refreshToken', token.refreshToken);
        return next.handle(this.attachToken(request, token.jwt));
      })
    );
  }

  handleErrorGlobal(error) {
    let errorMessage = '';
    if (error.error instanceof HttpErrorResponse) {
      this.toastrService.danger(
        `${error.status} ${error.error.serviceErrors[0]}`
      );
    } else {
      if (
        error.status == 400 &&
        error.error.serviceErrors[0] ==
          'Token is no longer valid. Please log back in.'
      ) {
        this.toastrService.danger(`${error.error.serviceErrors[0]}`, 'Failed');
        this.router.navigate(['auth/login']);
        localStorage.clear();
      }
      if (
        error.status == 400 &&
        error.error.serviceErrors[0] !=
          'Token is no longer valid. Please log back in.'
      ) {
        this.toastrService.danger(`${error.error.serviceErrors[0]}`, 'Failed');
      }
    }
    return throwError(errorMessage);
  }

  attachToken(request: HttpRequest<unknown>, token: string) {
    return request.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }
}
