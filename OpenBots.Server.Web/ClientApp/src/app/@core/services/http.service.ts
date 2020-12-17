import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NbToastrService } from '@nebular/theme';
import { environment } from '../../../environments/environment';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';

@Injectable()
export class HttpService {
  countapi = 0;
  get apiUrl(): string {
    return environment.apiUrl;
  }
  watchtotalSubject = new BehaviorSubject<any>('');
  currentMessagetotal = this.watchtotalSubject.asObservable();

  constructor(
    private http: HttpClient,
    private toastrService: NbToastrService
  ) {}

  getHealthStatus(endpoint: string): Observable<any> {
    return this.http.get(`${environment.healthUrl}/${endpoint}`);
  }
  get(endpoint: string, options?): Observable<any> {
    return this.http.get(`${this.apiUrl}/${endpoint}`, options);
  }

  post(endpoint: string, data?, options?): Observable<any> {
    return this.http.post(`${this.apiUrl}/${endpoint}`, data, options);
  }

  put(endpoint: string, data, options?): Observable<any> {
    return this.http.put(`${this.apiUrl}/${endpoint}`, data, options);
  }

  patch(endpoint: string, data, options?): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${endpoint}`, data, options);
  }

  delete(endpoint: string, options?): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${endpoint}`, options);
  }

  success(param): void {
    this.toastrService.success(`${param}`, 'Success');
  }

  error(param): void {
    this.toastrService.danger(`${param}`, 'Error');
  }

  warning(param): void {
    this.toastrService.warning(`${param}`, 'Warning');
  }
  primary(param): void {
    this.toastrService.primary(`${param}`, 'Info');
  }

  info(param): void {
    this.toastrService.info(`${param}`, 'Info');
  }

  /// for 429 error with data sharing flag .
  watchtotal(error, time) {
    const errorTime = {
      error,
      time,
    };
    this.countapi = 1;
    this.watchtotalSubject.next(errorTime);
  }
}
