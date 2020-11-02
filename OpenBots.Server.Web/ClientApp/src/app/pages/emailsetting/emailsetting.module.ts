import { NgModule } from '@angular/core';
import { NgxPaginationModule } from 'ngx-pagination';
import { EmailsettingRoutingModule } from './emailsetting-routing.module';
import { AllEmailSettingComponent } from './all-email-setting/all-email-setting.component';
import { SharedModule } from '../../@core/shared/shared.module';


@NgModule({
  declarations: [AllEmailSettingComponent],
  imports: [

    EmailsettingRoutingModule,
    SharedModule,
    NgxPaginationModule,

  ]
})
export class EmailsettingModule { }
