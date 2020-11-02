import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllJobsComponent } from './all-jobs/all-jobs.component';
import { GetJobIdComponent } from './get-job-id/get-job-id.component';


const routes: Routes = [
  {
    path:'list', component:AllJobsComponent
  },
  {
    path:'get-jobs-id' , component:GetJobIdComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class JobsRoutingModule { }
