import { NgModule } from '@angular/core';
import { SettingsRoutingModule } from './settings-routing.module';
import { AllSettingsComponent } from './all-settings/all-settings.component';
import { SharedModule } from '../../../@core/shared';
import { AddRuleComponent } from './add-rule/add-rule.component';

@NgModule({
  declarations: [AllSettingsComponent, AddRuleComponent],
  imports: [SharedModule, SettingsRoutingModule],
})
export class SettingsModule {}
