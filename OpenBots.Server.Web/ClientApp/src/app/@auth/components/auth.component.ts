import { Component, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { NbAuthService } from '@nebular/auth';
import { takeWhile } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'ngx-auth',
  styleUrls: ['./auth.component.scss'],
  template: `
    <nb-layout>
      <nb-layout-column>
        <nb-card [ngClass]="{ loginPage: loginStyle }">
          <!-- <nb-card-header *ngIf="show_header">
            <nav class="navigation">
              <a href="#" (click)="back()" class="link back-link" aria-label="Back">
                <nb-icon icon="arrow-back"></nb-icon>
              </a>
            </nav>
          </nb-card-header> -->
          <nb-card-body>
            <nb-auth-block>
              <router-outlet></router-outlet>
            </nb-auth-block>
          </nb-card-body>
        </nb-card>
      </nb-layout-column>
    </nb-layout>
  `,
})
export class NgxAuthComponent implements OnDestroy {
  private alive = true;
  show_header: boolean = true;
  subscription: any;
  url: any;
  authenticated: boolean = false;
  token: string = '';
  loginStyle = false;


  constructor(
    protected auth: NbAuthService,
    protected location: Location,
    protected route: ActivatedRoute
  ) {
    this.url = this.route.snapshot['_routerState'].url;
    if (
      this.url == '/auth/login' ||
      this.url == '/auth/register' ||
      this.url == '/auth/request-password' ||
      this.url == '/auth/reset-password'
    ) {
      this.loginStyle = true;
    }
    if (this.url === '/auth/terms-condition') {
      this.show_header = false;
    } else {
      this.show_header = true;
    }
    this.subscription = auth
      .onAuthenticationChange()
      .pipe(takeWhile(() => this.alive))
      .subscribe((authenticated: boolean) => {
        this.authenticated = authenticated;
      });
  }

  back() {
    this.location.back();
    return false;
  }

  ngOnDestroy(): void {
    this.alive = false;
  }
}
