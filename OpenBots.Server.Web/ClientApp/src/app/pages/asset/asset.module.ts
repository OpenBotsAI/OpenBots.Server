import { NgModule } from '@angular/core';
import { AssestRoutingModule } from './asset-routing.module';
import { AllAssetComponent } from './all-asset/all-asset.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { AssetService } from './asset.service';
import { AddAssetComponent } from './add-asset/add-asset.component';
import { GetAssetIdComponent } from './get-asset-id/get-asset-id.component';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import { FileSaverModule } from 'ngx-filesaver';
import { SharedModule } from '../../@core/shared/shared.module';

@NgModule({
  declarations: [AllAssetComponent, AddAssetComponent, GetAssetIdComponent],
  imports: [
    SharedModule,
    AssestRoutingModule,
    NgxPaginationModule,
    NgxJsonViewerModule,
    FileSaverModule,
  ],
  providers: [AssetService],
})
export class AssestModule {}
