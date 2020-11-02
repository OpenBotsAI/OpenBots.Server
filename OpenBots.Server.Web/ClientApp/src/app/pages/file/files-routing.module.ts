import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddFileComponent } from './add-file/add-file.component';
import { FileComponent } from './files/files.component';
import { GetFileIdComponent } from './get-files-id/get-file-id.component';

const routes: Routes = [
  { path: 'list', component: FileComponent },
  { path: 'add', component: AddFileComponent },
  { path: 'get-file-id/:id', component: GetFileIdComponent },
  { path: 'edit/:id', component: AddFileComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class FileRoutingModule {}
