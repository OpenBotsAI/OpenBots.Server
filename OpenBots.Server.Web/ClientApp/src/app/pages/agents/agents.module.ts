import { NgModule } from '@angular/core';
import { AgentsRoutingModule } from './agents-routing.module';
import { AllAgentsComponent } from './all-agents/all-agents.component';
import { GetAgentsIdComponent } from './get-agents-id/get-agents-id.component';
import { AddAgentsComponent } from './add-agents/add-agents.component';
import { AgentsService } from './agents.service';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared/shared.module';
import { RxReactiveFormsModule } from '@rxweb/reactive-form-validators';

@NgModule({
  declarations: [AllAgentsComponent, GetAgentsIdComponent, AddAgentsComponent],
  imports: [
    SharedModule,
    AgentsRoutingModule,
    NgxPaginationModule,
    RxReactiveFormsModule,
  ],
  providers: [AgentsService],
})
export class AgentsModule {}
