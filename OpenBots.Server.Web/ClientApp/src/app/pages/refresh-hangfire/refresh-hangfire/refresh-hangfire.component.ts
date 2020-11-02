import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../@core/services/auth.service';

@Component({
  selector: 'ngx-refresh-hangfire',
  templateUrl: './refresh-hangfire.component.html',
  styleUrls: ['./refresh-hangfire.component.scss']
})
export class RefreshHangfireComponent implements OnInit {
  token: any = [];
  constructor(public refresh: AuthService, protected router: Router) {
    this.refresh.refreshToken().subscribe((res: any) => {
      this.token = localStorage.getItem('accessToken');
      this.gotoHangfire()
    })
  }

  ngOnInit(): void {
  }
  gotoHangfire() {
    window.location.href =
      `/hangfire?access_token=${this.token}`

  }
}
