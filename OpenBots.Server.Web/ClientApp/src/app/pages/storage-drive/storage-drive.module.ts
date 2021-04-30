import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StorageDriveRoutingModule } from './storage-drive-routing.module';
import { StorageDriveGridComponent } from './storage-drive-grid/storage-drive-grid.component';
import { SharedModule } from '../../@core/shared';
import { NgxPaginationModule } from 'ngx-pagination';
import { StorageDriveComponent } from './storage-drive/storage-drive.component';
import { ViewStorageDriveComponent } from './view-storage-drive/view-storage-drive.component';

@NgModule({
  declarations: [StorageDriveGridComponent, StorageDriveComponent, ViewStorageDriveComponent],
  imports: [SharedModule, StorageDriveRoutingModule, NgxPaginationModule],
})
export class StorageDriveModule {}
