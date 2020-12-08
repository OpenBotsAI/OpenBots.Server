import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
} from '@angular/common/http';
import { catchError, filter, switchMap, take } from 'rxjs/operators';
import { NbToastrService } from '@nebular/theme';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { throwError } from 'rxjs/internal/observable/throwError';
import { Observable } from 'rxjs/internal/Observable';
import { HttpService } from '../services/http.service';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<any>(null);
  constructor(
    private toastrService: NbToastrService,
    private authService: AuthService,
    private router: Router,
    private httpService: HttpService
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
        } else if (error.status == 409) {
          return throwError(error);
        } else if (error.status == 429) {
          if (error.headers.get('Retry-After')) {
            if (this.httpService.countapi !== 1) {
              this.httpService.watchtotal(
                error.status,
                error.headers.get('Retry-After')
              );
            }
          } else if (this.httpService.countapi !== 1) {
            this.httpService.watchtotal(error.status, 30);
          }
        } else if (error.status != 401) {
          return this.handleErrorGlobal(error);
        }
      })
    );
  }

  handleError(request: HttpRequest<unknown>, next: HttpHandler) {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);
      return this.authService.refreshToken().pipe(
        switchMap((token: any) => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(token.jwt);
          return next.handle(this.attachToken(request, token.jwt));
        })
      );
    } else {
      return this.refreshTokenSubject.pipe(
        filter((token) => token != null),
        take(1),
        switchMap((token) => {
          return next.handle(this.attachToken(request, token));
        })
      );
    }
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
        error.error.serviceErrors &&
        error.error.serviceErrors[0] ==
          'Token is no longer valid. Please log back in.'
      ) {
        this.toastrService.danger(`${error.error.serviceErrors[0]}`, 'Failed');
        this.router.navigate(['auth/login']);
        localStorage.clear();
      }
      if (
        error.status == 400 &&
        error.error.serviceErrors &&
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
