import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AgentGroupRoutingModule } from './agentgroup-routing.module';
import { SharedModule } from '../../@core/shared';
import { NgxPaginationModule } from 'ngx-pagination';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AgentGroupComponent } from './agent-group/agent-group.component';
import { AllAgentGroupGridComponent } from './all-agent-group-grid/all-agent-group-grid.component';
import { AngularMultiSelectModule } from 'angular2-multiselect-dropdown';
@NgModule({
  declarations: [AgentGroupComponent, AllAgentGroupGridComponent],
  imports: [
    AgentGroupRoutingModule,
    SharedModule,
    NgxPaginationModule,
    ReactiveFormsModule,
    AngularMultiSelectModule,
    FormsModule,
  ],
  // schemas: [NO_ERRORS_SCHEMA],
})
export class AgentGroupModule {}
