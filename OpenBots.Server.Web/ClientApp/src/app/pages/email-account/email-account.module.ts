import { NgModule } from '@angular/core';
import { NgxPaginationModule } from 'ngx-pagination';
import { EmailAccountRoutingModule } from './email-account-routing.module';
import { AllEmailAccountComponent } from './all-email-account/all-email-account.component';
import { OwlDateTimeModule, OwlNativeDateTimeModule } from 'ng-pick-datetime'; 
import { SharedModule } from '../../@core/shared/shared.module';
import { EmailAccountsService } from './email-accounts.service';
import { GetEmailIdComponent } from './get-email-id/get-email-id.component';
import { AddEmailAccountComponent } from './add-email-account/add-email-account.component';
import { EmailTestingAccountComponent } from './email-testing-account/email-testing-account.component';
import { CKEditorModule } from 'ng2-ckeditor';
 

@NgModule({
  declarations: [
    AllEmailAccountComponent,
    GetEmailIdComponent,
    AddEmailAccountComponent,
    EmailTestingAccountComponent,
  ],
  imports: [
    EmailAccountRoutingModule,
    SharedModule,
    NgxPaginationModule,
    CKEditorModule,
  ],
  providers: [EmailAccountsService],
})
export class EmailAccountModule {}
