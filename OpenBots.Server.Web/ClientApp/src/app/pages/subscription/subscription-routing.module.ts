import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddSubscriptionComponent } from './add-subscription/add-subscription.component';
import { AllEventSubscriptionsComponent } from './all-event-subscriptions/all-event-subscriptions.component';

const routes: Routes = [
  {
    path: 'list',
    component: AllEventSubscriptionsComponent,
  },
  {
    path: 'add',
    component: AddSubscriptionComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SubscriptionRoutingModule {}
