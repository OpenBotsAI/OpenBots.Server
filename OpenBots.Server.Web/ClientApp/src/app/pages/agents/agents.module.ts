import { NgModule } from '@angular/core';
import { AgentsRoutingModule } from './agents-routing.module';
import { AllAgentsComponent } from './all-agents/all-agents.component';
import { GetAgentsIdComponent } from './get-agents-id/get-agents-id.component';
import { AddAgentsComponent } from './add-agents/add-agents.component';
import { EditAgentsComponent } from './edit-agents/edit-agents.component';
import { AgentsService } from './agents.service';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared/shared.module';

@NgModule({
  declarations: [
    AllAgentsComponent,
    GetAgentsIdComponent,
    AddAgentsComponent,
    EditAgentsComponent,
  ],
  imports: [
    SharedModule,
    AgentsRoutingModule,
    NgxPaginationModule,
  ],
  providers: [AgentsService]
})
export class AgentsModule {}
