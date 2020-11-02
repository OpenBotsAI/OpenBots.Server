import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllProcessComponent } from './all-process/all-process.component';
import { GetProcessIdComponent } from './get-process-id/get-process-id.component';
import { AddProcessComponent } from './add-process/add-process.component';
import { EditProcessComponent } from './edit-process/edit-process.component';


const routes: Routes = [
  {
    path:'list', component:AllProcessComponent
  },
  {
    path:'get-process-id' , component:GetProcessIdComponent
  },
  {
    path:'add' , component:AddProcessComponent  },
    {
      path: 'edit',
      component:EditProcessComponent
    }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProcessRoutingModule { }
