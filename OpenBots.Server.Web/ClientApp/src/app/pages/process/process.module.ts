import { NgModule } from '@angular/core';
import { ProcessRoutingModule } from './process-routing.module';
import { ProcessService } from './process.service';
import { AllProcessComponent } from './all-process/all-process.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '../../@core/shared/shared.module';
import { GetProcessIdComponent } from './get-process-id/get-process-id.component';
import { AddProcessComponent } from './add-process/add-process.component';
import { EditProcessComponent } from './edit-process/edit-process.component';
import { FileSaverModule } from 'ngx-filesaver';

@NgModule({
  declarations: [AllProcessComponent, GetProcessIdComponent, AddProcessComponent, EditProcessComponent],
  imports: [
    ProcessRoutingModule,
    NgxPaginationModule,
    FileSaverModule,
    SharedModule
  ],
  providers:[
    ProcessService
  ]
})
export class ProcessModule { }
