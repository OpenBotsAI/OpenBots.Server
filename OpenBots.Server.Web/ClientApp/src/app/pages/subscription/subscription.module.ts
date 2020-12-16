import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SubscriptionRoutingModule } from './subscription-routing.module';
import { AllEventSubscriptionsComponent } from './all-event-subscriptions/all-event-subscriptions.component';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared';
import { SubscriptionService } from './subscription.service';
import { AddSubscriptionComponent } from './add-subscription/add-subscription.component';
import { GetSubscriptionIdComponent } from './get-subscription-id/get-subscription-id.component';
import { EditSubscriptionComponent } from './edit-subscription/edit-subscription.component';

@NgModule({
  declarations: [AllEventSubscriptionsComponent, AddSubscriptionComponent, GetSubscriptionIdComponent, EditSubscriptionComponent],
  imports: [
    CommonModule,
    SubscriptionRoutingModule,
    SharedModule,
    NgxPaginationModule,
    NgxJsonViewerModule,
  ],
  providers: [SubscriptionService],
})
export class SubscriptionModule {}
