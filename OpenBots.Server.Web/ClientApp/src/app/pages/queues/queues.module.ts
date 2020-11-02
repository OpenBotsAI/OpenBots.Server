import { NgModule } from '@angular/core';
import { QueuesRoutingModule } from './queues-routing.module';
import { AllQueuesComponent } from './all-queues/all-queues.component';
import { SharedModule } from '../../@core/shared';
import { NgxPaginationModule } from 'ngx-pagination';
import { ReactiveFormsModule } from '@angular/forms';
import { AddQueueComponent } from './queue/queue.component';
import { ViewQueuesComponent } from './view-queues/view-queues.component';

@NgModule({
  declarations: [AllQueuesComponent, AddQueueComponent, ViewQueuesComponent],
  imports: [
    SharedModule,
    QueuesRoutingModule,
    NgxPaginationModule,
    ReactiveFormsModule,
  ],
})
export class QueuesModule {}
