import { Component, OnDestroy, OnInit } from '@angular/core';
import { AnalyticsService } from './@core/utils';
import { Subject } from 'rxjs';
import { BlockUI, NgBlockUI } from 'ng-block-ui';
import { HttpService } from './@core/services/http.service';
@Component({
  selector: 'ngx-app',
  templateUrl: 'app.component.html',
})
export class AppComponent implements OnInit, OnDestroy {
  timer: any;
  private destroy$: Subject<void> = new Subject<void>();
  @BlockUI() blockUI: NgBlockUI;

  constructor(private analytics: AnalyticsService, private httpService: HttpService) { }

  ngOnInit(): void {
    this.analytics.trackPageViews();
    this.toggleBlocking();
  }
 

  toggleBlocking() {
    this.httpService.currentMessagetotal.subscribe(
      (res: any) => {
        if (res.error == 429) {
          let counter = res.time;
          const interval = setInterval(() => {
            this.blockUI.start('Server Busy, You can try after ' + counter + ' Seconds');
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

      })
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
