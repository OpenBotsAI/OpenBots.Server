import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddQueueItemsComponent } from './add-queue-items/add-queue-items.component';
import { AllQueueItemsComponent } from './all-queue-items/all-queue-items.component';
import { ViewQueueItemComponent } from './view-queue-item/view-queue-item.component';

const routes: Routes = [
  { path: '', component: AllQueueItemsComponent },
  { path: 'view/:id', component: ViewQueueItemComponent },
  { path: 'edit/:id', component: AddQueueItemsComponent },
  { path: 'new', component: AddQueueItemsComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class QueueItemsRoutingModule {}
