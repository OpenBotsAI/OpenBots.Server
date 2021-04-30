import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { StorageDriveGridComponent } from './storage-drive-grid/storage-drive-grid.component';
import { StorageDriveComponent } from './storage-drive/storage-drive.component';

const routes: Routes = [
  { path: '', component: StorageDriveGridComponent },
  { path: 'add', component: StorageDriveComponent },
  { path: 'edit/:id', component: StorageDriveComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StorageDriveRoutingModule {}
