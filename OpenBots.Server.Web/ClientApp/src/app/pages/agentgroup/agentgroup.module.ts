import { NgModule } from '@angular/core';
import { AgentGroupRoutingModule } from './agentgroup-routing.module';
import { SharedModule } from '../../@core/shared';
import { NgxPaginationModule } from 'ngx-pagination';
import { ReactiveFormsModule } from '@angular/forms';
import { AgentGroupComponent } from './agent-group/agent-group.component';
import { AllAgentGroupGridComponent } from './all-agent-group-grid/all-agent-group-grid.component';
import { ViewAgentGroupComponent } from './view-agent-group/view-agent-group.component';
import { NgSelectModule } from '@ng-select/ng-select';
@NgModule({
  declarations: [
    AgentGroupComponent,
    AllAgentGroupGridComponent,
    ViewAgentGroupComponent,
  ],
  imports: [
    AgentGroupRoutingModule,
    SharedModule,
    NgxPaginationModule,
    ReactiveFormsModule,
    NgSelectModule,
  ],
})
export class AgentGroupModule {}
