import { NgModule } from '@angular/core';
import { CredentialsRoutingModule } from './credentials-routing.module';
import { SharedModule } from '../../@core/shared/shared.module';
import { CredentialsComponent } from './credentials/credentials.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { ViewCredentialsComponent } from './view-credentials/view-credentials.component';
import { ReactiveFormsModule } from '@angular/forms';
import { AddCredentialsComponent } from './add-credentials/add-credentials.component';
import { OwlDateTimeModule, OwlNativeDateTimeModule } from 'ng-pick-datetime';
@NgModule({
  declarations: [
    CredentialsComponent,
    ViewCredentialsComponent,
    AddCredentialsComponent,
  ],
  imports: [
    SharedModule,
    CredentialsRoutingModule,
    NgxPaginationModule,
    ReactiveFormsModule,
    OwlDateTimeModule,
    OwlNativeDateTimeModule,
  ],
})
export class CredentialsModule {}
