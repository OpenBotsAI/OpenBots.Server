import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CredentialsComponent } from './credentials';
import { ViewCredentialsComponent } from './view-credentials/view-credentials.component';
import { AddCredentialsComponent } from './add-credentials/add-credentials.component';

const routes: Routes = [
  { path: '', component: CredentialsComponent },
  { path: 'add', component: AddCredentialsComponent },
  { path: 'view/:id', component: ViewCredentialsComponent },
  { path: 'edit/:id', component: AddCredentialsComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CredentialsRoutingModule {}
