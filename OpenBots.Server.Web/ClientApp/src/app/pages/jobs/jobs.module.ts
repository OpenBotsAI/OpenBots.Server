import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JobsRoutingModule } from './jobs-routing.module';
import { AllJobsComponent } from './all-jobs/all-jobs.component';
import { GetJobIdComponent } from './get-job-id/get-job-id.component';
import { JobsService } from './jobs.service';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared/shared.module';

@NgModule({
  declarations: [AllJobsComponent, GetJobIdComponent],
  imports: [
    JobsRoutingModule,
    NgxPaginationModule,
    SharedModule
  ],providers:[
    JobsService
  ]
})
export class JobsModule { }
