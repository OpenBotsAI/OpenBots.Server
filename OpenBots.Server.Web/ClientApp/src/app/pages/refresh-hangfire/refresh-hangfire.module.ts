import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { RefreshHangfireRoutingModule } from './refresh-hangfire-routing.module';
import { RefreshHangfireComponent } from './refresh-hangfire/refresh-hangfire.component';


@NgModule({
  declarations: [RefreshHangfireComponent],
  imports: [
    CommonModule,
    RefreshHangfireRoutingModule
  ]
})
export class RefreshHangfireModule { }
