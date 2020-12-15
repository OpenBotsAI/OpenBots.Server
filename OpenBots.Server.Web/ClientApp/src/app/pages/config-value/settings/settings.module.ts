import { NgModule } from '@angular/core';
import { SettingsRoutingModule } from './settings-routing.module';
import { AllSettingsComponent } from './all-settings/all-settings.component';
import { SharedModule } from '../../../@core/shared';
import { AddRuleComponent } from './add-rule/add-rule.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { ViewIPFencingComponent } from './view-ipfencing/view-ipfencing.component';
import { RxReactiveFormsModule } from '@rxweb/reactive-form-validators';

@NgModule({
  declarations: [
    AllSettingsComponent,
    AddRuleComponent,
    ViewIPFencingComponent,
  ],
  imports: [
    SharedModule,
    SettingsRoutingModule,
    NgxPaginationModule,
    RxReactiveFormsModule,
  ],
})
export class SettingsModule {}
