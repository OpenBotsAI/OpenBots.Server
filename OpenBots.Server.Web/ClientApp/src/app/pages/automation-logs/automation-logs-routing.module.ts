import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllAutomationLogsComponent } from './all-automation-logs/all-automation-logs.component';
import { ViewAutomationLogsComponent } from './view-automation-logs/view-automation-logs.component';

const routes: Routes = [
  { path: '', component: AllAutomationLogsComponent },

  { path: 'view/:id', component: ViewAutomationLogsComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AutomationLogsRoutingModule {}
