import { NgModule } from '@angular/core';
import { ProcessLogsRoutingModule } from './process-logs-routing.module';
import { AllProcessLogsComponent } from './all-process-logs/all-process-logs.component';
import { SharedModule } from '../../@core/shared/shared.module';
import { NgxPaginationModule } from 'ngx-pagination';
import { ViewProcessLogsComponent } from './view-process-logs/view-process-logs.component';


@NgModule({
  declarations: [AllProcessLogsComponent, ViewProcessLogsComponent],
  imports: [
    SharedModule,
    ProcessLogsRoutingModule,
    NgxPaginationModule,
 
  ]
})
  
export class ProcessLogsModule {}
