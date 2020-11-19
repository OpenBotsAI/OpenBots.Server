import { NgModule } from '@angular/core';
import { ConfigValueRoutingModule } from './config-value-routing.module';
import { AllConfigValueComponent } from './all-config-value/all-config-value.component';
import { SharedModule } from '../../@core/shared';
import { NgxPaginationModule } from 'ngx-pagination';
import { GetConfigValueComponent } from './get-config-value/get-config-value.component';
import { EditConfigValueComponent } from './edit-config-value/edit-config-value.component';


@NgModule({
  declarations: [AllConfigValueComponent, GetConfigValueComponent, EditConfigValueComponent],
  imports: [
    ConfigValueRoutingModule,
    SharedModule,
    NgxPaginationModule,

  ]
})
export class ConfigValueModule { }
