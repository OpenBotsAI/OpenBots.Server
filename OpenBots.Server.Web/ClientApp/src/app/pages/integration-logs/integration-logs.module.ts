import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { IntegrationLogsRoutingModule } from './integration-logs-routing.module';
import { AllIntegrationLogsComponent } from './all-integration-logs/all-integration-logs.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared';
import { IntegrationLogsService } from './integration-logs.service';
import { GetIntegrationLogsIdComponent } from './get-integration-logs-id/get-integration-logs-id.component';
import { NgxJsonViewerModule } from 'ngx-json-viewer';

@NgModule({
  declarations: [AllIntegrationLogsComponent, GetIntegrationLogsIdComponent],
  imports: [
    CommonModule,
    IntegrationLogsRoutingModule,
    SharedModule,
    NgxPaginationModule,
    NgxJsonViewerModule,
  ],
  providers: [IntegrationLogsService],
})
export class IntegrationLogsModule {}
