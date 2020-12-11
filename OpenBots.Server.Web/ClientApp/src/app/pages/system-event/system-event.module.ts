import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AllSystemEventsComponent } from './all-system-events/all-system-events.component';
import { GetSystemEventsIdComponent } from './get-system-events-id/get-system-events-id.component';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared';
import { SystemEventRoutingModule } from './system-event-routing.module';
import { SystemEventService } from './system-event.service';

@NgModule({
  declarations: [AllSystemEventsComponent, GetSystemEventsIdComponent],
  imports: [
    CommonModule,
    SystemEventRoutingModule,
    SharedModule,
    NgxPaginationModule,
    NgxJsonViewerModule,
  ],
  providers: [SystemEventService],
})
export class SyetemEventModule {}
