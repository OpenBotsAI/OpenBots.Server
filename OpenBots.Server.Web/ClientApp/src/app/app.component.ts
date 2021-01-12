import { Component, OnDestroy, OnInit } from '@angular/core';
import { AnalyticsService } from './@core/utils';
import { Subject } from 'rxjs';
import { BlockUI, NgBlockUI } from 'ng-block-ui';
import { HttpService } from './@core/services/http.service';
import { SwUpdate } from '@angular/service-worker';
import { ConnectionService } from 'ng-connection-service';
@Component({
  selector: 'ngx-app',
  templateUrl: 'app.component.html',
})
export class AppComponent implements OnInit, OnDestroy {
  timer: any;
  showScreen: boolean;
  private destroy$: Subject<void> = new Subject<void>();
  @BlockUI() blockUI: NgBlockUI;
  isConnectionAvailable: boolean = navigator.onLine;

  isConnected = true;
  noInternetConnection: boolean;
  constructor(
    private connectionService: ConnectionService,
    private analytics: AnalyticsService,
    private httpService: HttpService,
    private swUpdate: SwUpdate
  ) {
    // this.showScreen = true;
    // // if (window.addEventListener.length == 0) {
    // //   this.showTemp = true;
    // // }
    // window.addEventListener('online', (internet) => {
    //   this.isConnectionAvailable = true;
    //   this.showScreen = true;
    //   console.log(internet.type);
    // });

    // window.addEventListener('offline', (internet) => {
    //   this.isConnectionAvailable = false;
    //   this.showScreen = false;
    //   console.log(internet.type);
    // });
    this.connectionService.monitor().subscribe((isConnected) => {
      this.isConnected = isConnected;
      if (this.isConnected) {
        this.noInternetConnection = false;
      } else {
        this.noInternetConnection = true;
      }
    });
  }

  ngOnInit(): void {
    this.blockUI.start('loading');
    if (this.swUpdate.isEnabled) {
      this.swUpdate.available.subscribe((data: any) => {
        console.log(data);
        if (confirm('New OpenBots Server version available ')) {
          window.location.reload();
        }
      });
    }
    if (window.matchMedia('(display-mode: standalone)').matches) {
      console.log('display-mode is standalone');
    }
    window.addEventListener('appinstalled', (evt) => {
      if (evt.type == 'appinstalled') {
        this.showScreen = false;
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
