import { Component, OnDestroy, OnInit } from '@angular/core';
import { AnalyticsService } from './@core/utils';
import { Subject } from 'rxjs';
import { BlockUI, NgBlockUI } from 'ng-block-ui';
import { HttpService } from './@core/services/http.service';
import { SwUpdate } from '@angular/service-worker';
@Component({
  selector: 'ngx-app',
  templateUrl: 'app.component.html',
})
export class AppComponent implements OnInit, OnDestroy {
  timer: any;
  showTemp: boolean;
  private destroy$: Subject<void> = new Subject<void>();
  @BlockUI() blockUI: NgBlockUI;
  isConnectionAvailable: boolean = navigator.onLine;
  constructor(
    private analytics: AnalyticsService,
    private httpService: HttpService,
    private swUpdate: SwUpdate
  ) {
    if (window.addEventListener.length == 0) {
      this.showTemp = true;
    }
         window.addEventListener('online', (internet) => {
           this.isConnectionAvailable = true;
           console.log(internet);
         });

         window.addEventListener('offline', (internet) => {
           this.isConnectionAvailable = false;
                console.log(internet);
         });
  }

  ngOnInit(): void {
    this.blockUI.start('loading');
    if (this.swUpdate.isEnabled) {
      this.swUpdate.available.subscribe((data: any) => {
        console.log(data);
        if (confirm('new version availale')) {
          window.location.reload();
        }
      });
    }
    if (window.matchMedia('(display-mode: standalone)').matches) {
      console.log('display-mode is standalone');
    }
    window.addEventListener('appinstalled', (evt) => {
      if (evt.type == 'appinstalled') {
        this.showTemp = false;
        console.log(this.showTemp);
        console.log(evt);
        console.log('a2hs installed');
      }
    });

    this.analytics.trackPageViews();
    this.blockUI.stop();
    this.toggleBlocking();
  }

  toggleBlocking() {
    this.httpService.currentMessagetotal.subscribe((res: any) => {
      if (res.error == 429) {
        let counter = res.time;
        const interval = setInterval(() => {
          this.blockUI.start(
            'Server Busy, You can try after ' + counter + ' Seconds'
          );
          counter--;
          if (counter < 0) {
            clearInterval(interval);
            this.blockUI.stop();
          }
          setTimeout(() => {
            this.blockUI.stop();
          }, 1000);
        }, 1000);
      }
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
