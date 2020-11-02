import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
@Injectable({
  providedIn: 'root'
})
export class LoginGuard implements CanActivate {
  constructor(private router: Router) { }
  canActivate(): boolean {
    if (localStorage.getItem('accessToken') != null || localStorage.getItem('accessToken') != undefined) {
       
      return true;
    }
    else {
      this.router.navigate(['auth/login']);
      return false;
    }
  }
}
