import { NgModule } from '@angular/core';
import { NgxPaginationModule } from 'ngx-pagination';
import { EmailAccountRoutingModule } from './email-account-routing.module';
import { AllEmailAccountComponent } from './all-email-account/all-email-account.component';
import { OwlDateTimeModule, OwlNativeDateTimeModule } from 'ng-pick-datetime'; 
import { SharedModule } from '../../@core/shared/shared.module';
import { EmailAccountsService } from './email-accounts.service';
import { GetEmailIdComponent } from './get-email-id/get-email-id.component';
import { EditEmailAccountComponent } from './edit-email-account/edit-email-account.component';
import { AddEmailAccountComponent } from './add-email-account/add-email-account.component';

@NgModule({
  declarations: [AllEmailAccountComponent, GetEmailIdComponent, EditEmailAccountComponent, AddEmailAccountComponent

  ],
  imports: [

    EmailAccountRoutingModule,
    SharedModule,
    NgxPaginationModule,
  

  ],
  providers: [EmailAccountsService],

})
export class EmailAccountModule { }
