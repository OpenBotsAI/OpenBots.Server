import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddEmailAccountComponent } from './add-email-account/add-email-account.component';
import { AllEmailAccountComponent } from './all-email-account/all-email-account.component';
import { EditEmailAccountComponent } from './edit-email-account/edit-email-account.component';
import { GetEmailIdComponent } from './get-email-id/get-email-id.component';


const routes: Routes = [
  {
    path: 'list',
    component: AllEmailAccountComponent
  },
  {
    path: 'get-email-id',
    component: GetEmailIdComponent
  },
  {
    path: 'edit',
    component: EditEmailAccountComponent
  },
  {
    path: 'add',
    component: AddEmailAccountComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EmailAccountRoutingModule { }
