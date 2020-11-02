import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllChangeLogComponent } from './all-change-log/all-change-log.component';
import { GetChangelogIdComponent } from './get-change-id/get-change-log-id.component';


const routes: Routes = [
  {
    path: 'list',
    component: AllChangeLogComponent,
  },
  {
    path: 'get-change-log-id',
    component: GetChangelogIdComponent,
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ChangelogRoutingModule { }
