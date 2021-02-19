import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllAgentsComponent } from './all-agents/all-agents.component';
import { GetAgentsIdComponent } from './get-agents-id/get-agents-id.component';
import { AddAgentsComponent } from './add-agents/add-agents.component';
import { EditAgentsComponent } from './edit-agents/edit-agents.component';
const routes: Routes = [
  {
    path: 'list',
    component: AllAgentsComponent,
  },
  {
    path: 'new',
    component: AddAgentsComponent,
  },
  {
    path: 'get-agents-id',
    component: GetAgentsIdComponent,
  },
  {
    path: 'edit/:id',
    component: AddAgentsComponent,
  },

  //   { path: 'list', component: FileComponent },
  // { path: 'add', component: AddFileComponent },
  // { path: 'get-file-id/:id', component: GetFileIdComponent },
  // { path: 'edit/:id', component: AddFileComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AgentsRoutingModule { }
