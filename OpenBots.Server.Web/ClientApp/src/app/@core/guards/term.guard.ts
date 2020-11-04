import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
@Injectable({
  providedIn: 'root',
})
export class TermGuard implements CanActivate {
  constructor(private router: Router) {}
  canActivate(): boolean {
    if (JSON.parse(localStorage.getItem('isUserConsentRequired'))) {
      this.router.navigate(['auth/terms-condition']);
      return false;
    } else {
      return true;
    }
  }
}
