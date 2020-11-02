import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllEmailLogComponent } from './all-email-log/all-email-log.component';
import { GetEmailLogIdComponent } from './get-email-log-id/get-email-log-id.component';


const routes: Routes = [{
  path: 'list',
  component: AllEmailLogComponent
},
{
  path: 'get-emaillog-id',
  component: GetEmailLogIdComponent
},
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EmailLogRoutingModule { }
