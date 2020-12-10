import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddAutomationComponent } from './add-automation/add-automation.component';
import { AllAutomationComponent } from './all-automation/all-automation.component';
import { EditAutomationComponent } from './edit-automation/edit-automation.component';
import { GetAutomationIdComponent } from './get-automation-id/get-automation-id.component';

const routes: Routes = [
  {
    path: 'list',
    component: AllAutomationComponent,
  },
  {
    path: 'get-automation-id',
    component: GetAutomationIdComponent,
  },
  {
    path: 'add',
    component: AddAutomationComponent,
  },
  {
    path: 'edit',
    component: EditAutomationComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AutomationRoutingModule {}
