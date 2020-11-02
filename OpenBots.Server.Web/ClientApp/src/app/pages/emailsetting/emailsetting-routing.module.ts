import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllEmailSettingComponent } from './all-email-setting/all-email-setting.component';


const routes: Routes = [
  {
    path: 'list', component: AllEmailSettingComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EmailsettingRoutingModule { }
