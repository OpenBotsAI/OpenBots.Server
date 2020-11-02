import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { RefreshHangfireComponent } from './refresh-hangfire/refresh-hangfire.component';


const routes: Routes = [
  {
    path: '', component: RefreshHangfireComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class RefreshHangfireRoutingModule { }
