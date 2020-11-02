import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllProcessLogsComponent } from './all-process-logs/all-process-logs.component';
import { ViewProcessLogsComponent } from './view-process-logs/view-process-logs.component';

const routes: Routes = [
  { path: '', component: AllProcessLogsComponent },
  { path: 'view/:id', component: ViewProcessLogsComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ProcessLogsRoutingModule {}
