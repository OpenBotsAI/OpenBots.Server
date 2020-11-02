import { NgModule } from '@angular/core';
import { ScheduleRoutingModule } from './schedule-routing.module';
import { AllScheduleComponent } from './all-schedule/all-schedule.component';
import { SharedModule } from '../../@core/shared/shared.module';
import { AddScheduleComponent } from './add-schedule/add-schedule.component';
import { ViewScheduleComponent } from './view-schedule/view-schedule.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { CronEditorModule } from 'cron-editor';
import { OwlDateTimeModule, OwlNativeDateTimeModule } from 'ng-pick-datetime';
import { NbToggleModule } from '@nebular/theme';


@NgModule({
  declarations: [
    AllScheduleComponent,
    AddScheduleComponent,
    ViewScheduleComponent,
  ],
  imports: [
    SharedModule,
    ScheduleRoutingModule,
    NgxPaginationModule,
    CronEditorModule,
    OwlDateTimeModule,
    NbToggleModule,
    OwlNativeDateTimeModule,
  ],
})
export class ScheduleModule {}
