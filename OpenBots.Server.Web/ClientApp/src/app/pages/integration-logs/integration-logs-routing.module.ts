import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllIntegrationLogsComponent } from './all-integration-logs/all-integration-logs.component';
import { GetIntegrationLogsIdComponent } from './get-integration-logs-id/get-integration-logs-id.component';

const routes: Routes = [
  {
    path: 'list',
    component: AllIntegrationLogsComponent,
  },
  {
    path: 'get-integration-log-id',
    component: GetIntegrationLogsIdComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class IntegrationLogsRoutingModule {}
