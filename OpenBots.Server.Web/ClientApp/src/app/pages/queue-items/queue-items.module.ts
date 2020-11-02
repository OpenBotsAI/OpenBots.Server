import { NgModule } from '@angular/core';
import { QueueItemsRoutingModule } from './queue-items-routing.module';
import { AddQueueItemsComponent } from './add-queue-items/add-queue-items.component';
import { AllQueueItemsComponent } from './all-queue-items/all-queue-items.component';
import { ViewQueueItemComponent } from './view-queue-item/view-queue-item.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared/shared.module';
import { OwlDateTimeModule, OwlNativeDateTimeModule } from 'ng-pick-datetime';


@NgModule({
  declarations: [
    AddQueueItemsComponent,
    AllQueueItemsComponent,
    ViewQueueItemComponent,
  ],
  imports: [
    SharedModule,
    QueueItemsRoutingModule,
    NgxPaginationModule,
    OwlDateTimeModule,
    OwlNativeDateTimeModule,
  ],
})
export class QueueItemsModule {}
