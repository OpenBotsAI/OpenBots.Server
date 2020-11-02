import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
@Injectable({
  providedIn: 'root'
})
export class TermGuard implements CanActivate {
  constructor(private router: Router) { }
  canActivate(): boolean {
    if (localStorage.getItem('isUserConsentRequired') === 'true') {
      this.router.navigate(['auth/terms-condition']);
      return false
    } else {
      return true;
    }
  }
}

