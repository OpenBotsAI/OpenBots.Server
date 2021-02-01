import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AllFilesComponent } from './all-files/all-files.component';

const routes: Routes = [{ path: '', component: AllFilesComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class FileManagerRoutingModule {}
