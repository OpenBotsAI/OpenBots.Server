import { NgModule } from '@angular/core';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared/shared.module';;
import { AddFileComponent } from './add-file/add-file.component';
import { FileRoutingModule } from './files-routing.module';
import { FileComponent } from './files/files.component';
import { GetFileIdComponent } from './get-files-id/get-file-id.component';
 
@NgModule({
  declarations: [FileComponent, GetFileIdComponent, AddFileComponent],
  imports: [
    SharedModule,
    FileRoutingModule,
    NgxPaginationModule,
  ],
})
export class FileModule {}
