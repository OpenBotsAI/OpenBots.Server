import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllQueuesComponent } from './all-queues/all-queues.component';
import { AddQueueComponent } from './queue/queue.component';
import { ViewQueuesComponent } from './view-queues/view-queues.component';

const routes: Routes = [
  { path: '', component: AllQueuesComponent },
  { path: 'add', component: AddQueueComponent },
  { path: 'edit/:id', component: AddQueueComponent },
  { path: 'view/:id', component: ViewQueuesComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class QueuesRoutingModule {}
