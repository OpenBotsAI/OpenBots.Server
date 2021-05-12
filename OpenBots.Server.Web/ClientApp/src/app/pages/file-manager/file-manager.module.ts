import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FileManagerRoutingModule } from './file-manager-routing.module';
import { AllFilesComponent } from './all-files/all-files.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared';
import { FileSaverModule } from 'ngx-filesaver';

@NgModule({
  declarations: [AllFilesComponent],
  imports: [
    CommonModule,
    FileManagerRoutingModule,
    SharedModule,
    NgxPaginationModule,
    FileSaverModule,
  ],
})
export class FileManagerModule {}
