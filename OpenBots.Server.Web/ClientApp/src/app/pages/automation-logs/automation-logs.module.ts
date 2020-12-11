import { NgModule } from '@angular/core';
import { SharedModule } from '../../@core/shared/shared.module';
import { NgxPaginationModule } from 'ngx-pagination';
import { AllAutomationLogsComponent } from './all-automation-logs/all-automation-logs.component';
import { ViewAutomationLogsComponent } from './view-automation-logs/view-automation-logs.component';
import { AutomationLogsRoutingModule } from './automation-logs-routing.module';

@NgModule({
  declarations: [AllAutomationLogsComponent, ViewAutomationLogsComponent],
  imports: [SharedModule, AutomationLogsRoutingModule, NgxPaginationModule],
})
export class AutomationLogsModule {}
