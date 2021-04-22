import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllAssetComponent } from './all-asset/all-asset.component';
import { AddAssetComponent } from './add-asset/add-asset.component';
import { GetAssetIdComponent } from './get-asset-id/get-asset-id.component';

const routes: Routes = [
  {
    path: 'list',
    component: AllAssetComponent,
  },
  {
    path: 'add',
    component: AddAssetComponent,
  },
  {
    path: 'get-asset-id',
    component: GetAssetIdComponent,
  },
  {
    path: 'edit/:id',
    component: AddAssetComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AssestRoutingModule { }
