import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AgentGroupComponent } from './agent-group/agent-group.component';
import { AllAgentGroupGridComponent } from './all-agent-group-grid/all-agent-group-grid.component';

const routes: Routes = [
  { path: '', component: AgentGroupComponent },
  { path: 'edit/:id', component: AgentGroupComponent },
  { path: 'list', component: AllAgentGroupGridComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AgentGroupRoutingModule {}
