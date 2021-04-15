import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddBusinessEventComponent } from './add-business-event/add-business-event.component';
import { AllBusinessEventComponent } from './all-business-event/all-business-event.component';
import { ViewBusinessEventComponent } from './view-business-event/view-business-event.component';


const routes: Routes = [
  { path: 'list', component: AllBusinessEventComponent },
  { path: 'add', component: AddBusinessEventComponent },
  { path: 'edit/:id', component: AddBusinessEventComponent },
  {
    path: 'view/:id',
    component: ViewBusinessEventComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BusinessEventRoutingModule { }
