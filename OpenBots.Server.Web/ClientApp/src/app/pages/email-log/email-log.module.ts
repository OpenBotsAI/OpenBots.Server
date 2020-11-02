import { NgModule } from '@angular/core';
import { SharedModule } from '../../@core/shared/shared.module';
import { EmailLogRoutingModule } from './email-log-routing.module';
import { AllEmailLogComponent } from './all-email-log/all-email-log.component';
import { GetEmailLogIdComponent } from './get-email-log-id/get-email-log-id.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { EmailLogService } from './email-log.service';
import { NgxJsonViewerModule } from 'ngx-json-viewer';


@NgModule({
  declarations: [AllEmailLogComponent, GetEmailLogIdComponent],
  imports: [

    EmailLogRoutingModule,
    SharedModule,
    NgxPaginationModule,
    NgxJsonViewerModule,


  ],
  providers: [EmailLogService],
})
export class EmailLogModule { }
