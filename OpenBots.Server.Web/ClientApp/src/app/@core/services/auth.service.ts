import { Injectable } from '@angular/core';
import { HttpService } from './http.service';
import { HttpHeaders } from '@angular/common/http';
import { tap } from 'rxjs/internal/operators/tap';

@Injectable({
  providedIn: 'root', 
})
export class AuthService {
  headerData = new HttpHeaders({ 'Content-Type': 'application/json' });
  constructor(private httpService: HttpService) {}

  refreshToken() {
    const obj = {
      token: localStorage.getItem('accessToken'),
      refreshToken: localStorage.getItem('refreshToken'),
    };
    return this.httpService.post('Auth/Refresh', obj, this.headerData).pipe(
      tap((response) => {
        this.storeToken(response);
      })
    );
  }

  storeToken(data) {
    localStorage.setItem('accessToken', data.jwt);
    localStorage.setItem('refreshToken', data.refreshToken);
  }
}
