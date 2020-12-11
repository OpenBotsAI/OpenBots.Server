import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddRuleComponent } from './add-rule/add-rule.component';
import { AllSettingsComponent } from './all-settings/all-settings.component';

const routes: Routes = [
  { path: '', component: AllSettingsComponent },
  { path: 'rule/add', component: AddRuleComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SettingsRoutingModule {}
