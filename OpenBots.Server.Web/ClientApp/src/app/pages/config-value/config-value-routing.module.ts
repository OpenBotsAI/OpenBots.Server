import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllConfigValueComponent } from './all-config-value/all-config-value.component';
import { EditConfigValueComponent } from './edit-config-value/edit-config-value.component';
import { GetConfigValueComponent } from './get-config-value/get-config-value.component';

const routes: Routes = [
  { path: 'list', component: AllConfigValueComponent },
  { path: 'get-config-id', component: GetConfigValueComponent },
  { path: 'edit', component: EditConfigValueComponent },
  {
    path: 'settings',
    loadChildren: () =>
      import('./settings/settings.module').then((mod) => mod.SettingsModule),
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ConfigValueRoutingModule {}
