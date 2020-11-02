import { NgModule } from '@angular/core';
import { NgxPaginationModule } from 'ngx-pagination';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import { SharedModule } from '../../@core/shared/shared.module';
import { ChangelogService } from './change-log.service';
import { ChangelogRoutingModule } from './change-log-routing.module';
import { AllChangeLogComponent } from './all-change-log/all-change-log.component';
import { GetChangelogIdComponent } from './get-change-id/get-change-log-id.component';

@NgModule({
  declarations: [AllChangeLogComponent, GetChangelogIdComponent],
  imports: [
    SharedModule,
    ChangelogRoutingModule,
    NgxPaginationModule,
    NgxJsonViewerModule,
  ],
  providers: [ChangelogService],
})
export class ChangelogModule {}
