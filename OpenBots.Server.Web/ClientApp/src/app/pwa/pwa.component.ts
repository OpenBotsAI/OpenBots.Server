import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'ngx-pwa',
  templateUrl: './pwa.component.html',
  styleUrls: ['./pwa.component.scss'],
})
export class PwaComponent implements OnInit {
  isConnectionAvailable: boolean = navigator.onLine;
  constructor() {
    // window.addEventListener('online', (internet) => {
    //   this.isConnectionAvailable = true;
    // });
    // window.addEventListener('offline', (internet) => {
    //   this.isConnectionAvailable = false;
    // });
  }

  ngOnInit(): void {}
}
