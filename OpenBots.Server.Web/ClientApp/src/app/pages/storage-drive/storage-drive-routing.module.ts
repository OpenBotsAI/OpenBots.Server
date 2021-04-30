import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { StorageDriveGridComponent } from './storage-drive-grid/storage-drive-grid.component';
import { StorageDriveComponent } from './storage-drive/storage-drive.component';
import { ViewStorageDriveComponent } from './view-storage-drive/view-storage-drive.component';

const routes: Routes = [
  { path: '', component: StorageDriveGridComponent },
  { path: 'add', component: StorageDriveComponent },
  { path: 'edit/:id', component: StorageDriveComponent },
  { path: 'view/:id', component: ViewStorageDriveComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StorageDriveRoutingModule {}
