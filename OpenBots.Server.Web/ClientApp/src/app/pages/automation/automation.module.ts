import { NgModule } from '@angular/core';

import { AutomationService } from './automation.service';

import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared/shared.module';

import { FileSaverModule } from 'ngx-filesaver';
import { AddAutomationComponent } from './add-automation/add-automation.component';
import { AllAutomationComponent } from './all-automation/all-automation.component';
import { EditAutomationComponent } from './edit-automation/edit-automation.component';
import { GetAutomationIdComponent } from './get-automation-id/get-automation-id.component';
import { AutomationRoutingModule } from './automation-routing.module';
@NgModule({
  declarations: [
    AllAutomationComponent,
    GetAutomationIdComponent,
    AddAutomationComponent,
    EditAutomationComponent,
  ],
  imports: [
    AutomationRoutingModule,
    NgxPaginationModule,
    FileSaverModule,
    SharedModule,
  ],
  providers: [AutomationService],
})
export class AutomationModule {}
