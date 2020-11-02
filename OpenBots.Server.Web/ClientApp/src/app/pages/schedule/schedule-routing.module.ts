import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllScheduleComponent } from './all-schedule/all-schedule.component';
import { AddScheduleComponent } from './add-schedule/add-schedule.component';
import { ViewScheduleComponent } from './view-schedule/view-schedule.component';

const routes: Routes = [
  { path: '', component: AllScheduleComponent },
  { path: 'add', component: AddScheduleComponent },
  { path: 'edit/:id', component: AddScheduleComponent },
  { path: 'view/:id', component: ViewScheduleComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ScheduleRoutingModule {}
