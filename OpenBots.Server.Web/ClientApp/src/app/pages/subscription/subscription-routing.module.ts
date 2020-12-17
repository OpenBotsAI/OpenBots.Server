import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddSubscriptionComponent } from './add-subscription/add-subscription.component';
import { AllEventSubscriptionsComponent } from './all-event-subscriptions/all-event-subscriptions.component';
import { EditSubscriptionComponent } from './edit-subscription/edit-subscription.component';
import { GetSubscriptionIdComponent } from './get-subscription-id/get-subscription-id.component';

const routes: Routes = [
  {
    path: 'list',
    component: AllEventSubscriptionsComponent,
  },
  {
    path: 'add',
    component: AddSubscriptionComponent,
  },
  {
    path: 'get-subscription-id',
    component: GetSubscriptionIdComponent,
  },
  {
    path: 'edit',
    component: EditSubscriptionComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SubscriptionRoutingModule {}
