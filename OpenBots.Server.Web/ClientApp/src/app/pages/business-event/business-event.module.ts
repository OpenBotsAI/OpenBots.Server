import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessEventRoutingModule } from './business-event-routing.module';
import { AllBusinessEventComponent } from './all-business-event/all-business-event.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared';
import { BusinessEventService } from './business-event.service';
import { AddBusinessEventComponent } from './add-business-event/add-business-event.component';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import { ViewBusinessEventComponent } from './view-business-event/view-business-event.component';
import { RaiseBusinessEventComponent } from './raise-business-event/raise-business-event.component';


@NgModule({
  declarations: [AllBusinessEventComponent, AddBusinessEventComponent, ViewBusinessEventComponent, RaiseBusinessEventComponent],
  imports: [
    CommonModule,
    BusinessEventRoutingModule,
    SharedModule,
    NgxPaginationModule,
    NgxJsonViewerModule,
  ],
  providers: [BusinessEventService]
})
export class BusinessEventModule { }
