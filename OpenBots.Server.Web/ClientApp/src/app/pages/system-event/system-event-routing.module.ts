import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllSystemEventsComponent } from './all-system-events/all-system-events.component';
import { GetSystemEventsIdComponent } from './get-system-events-id/get-system-events-id.component';

const routes: Routes = [
  { path: 'list', component: AllSystemEventsComponent },
  {
    path: 'get-system-event-id',
    component: GetSystemEventsIdComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SystemEventRoutingModule {}
