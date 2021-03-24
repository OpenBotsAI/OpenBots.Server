import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AssetService } from '../asset.service';
import { DatePipe } from '@angular/common';
import { FileSaverService } from 'ngx-filesaver';
import { TimeDatePipe } from '../../../@core/pipe';
import { HttpResponse } from '@angular/common/http';
@Component({
  selector: 'ngx-get-asset-id',
  templateUrl: './get-asset-id.component.html',
  styleUrls: ['./get-asset-id.component.scss'],
})
export class GetAssetIdComponent implements OnInit {
  jsonValue: any = [];
  AssetType: any = [];
  addAsset: FormGroup;
  pipe = new DatePipe('en-US');
  now = Date();
  showGlobalAsset: boolean = false;
  showAgentAsstData: any = [];
  constructor(
    private acroute: ActivatedRoute,
    protected assetService: AssetService,
    private formBuilder: FormBuilder,
    private FileSaverService: FileSaverService,
    protected router: Router
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.getAllAsset(params.id);
    });
  }

  ngOnInit(): void {
    this.addAsset = this.formBuilder.group({
      binaryObjectID: [''],
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      id: [''],
      isDeleted: [''],
      jsonValue: [''],
      name: [''],
      numberValue: [''],
      textValue: [''],
      timestamp: [''],
      type: [''],
      updatedBy: [''],
      updatedOn: [''],
    });
  }

  onDown() {
    if (this.AssetType.type == 'Text') {
      let type = 'txt';
      const fileName = `${this.AssetType.name}.${type}`;
      const fileType = this.FileSaverService.genType(fileName);
      const txtBlob = new Blob([this.addAsset.value.textValue], {
        type: fileType,
      });
      this.FileSaverService.save(txtBlob, fileName);
    } else if (this.AssetType.type == 'JSON') {
      let type = 'json';
      const fileName = `${this.AssetType.name}.${type}`;
      const fileType = this.FileSaverService.genType(fileName);
      const txtBlob = new Blob([this.addAsset.value.jsonValue], {
        type: fileType,
      });
      this.FileSaverService.save(txtBlob, fileName);
    } else if (this.AssetType.type == 'Number') {
      let type = 'txt';
      const fileName = `${this.AssetType.name}.${type}`;
      const fileType = this.FileSaverService.genType(fileName);
      const txtBlob = new Blob([this.addAsset.value.numberValue], {
        type: fileType,
      });
      this.FileSaverService.save(txtBlob, fileName);
    } else if (this.AssetType.type == 'File') {
      let fileName: string;
      this.assetService
        .assetFileExport(this.AssetType.id)
        .subscribe((data: HttpResponse<Blob>) => {
          fileName = data.headers
            .get('content-disposition')
            .split(';')[1]
            .split('=')[1]
            .replace(/\"/g, '');
          this.FileSaverService.save(data.body, fileName);
        });
    }
  }

  getAllAsset(id) {
    this.assetService.getAssetbyId(id).subscribe((data: HttpResponse<any>) => {
      this.AssetType = data.body;
      const filterPipe = new TimeDatePipe();
      const fiteredArr = filterPipe.transform(this.AssetType.createdOn, 'lll');
      this.AssetType.createdOn = filterPipe.transform(
        this.AssetType.createdOn,
        'lll'
      );
      if (this.AssetType.jsonValue) {
        this.jsonValue = this.AssetType.jsonValue;
        this.jsonValue = JSON.parse(this.jsonValue);
      }
      this.addAsset.patchValue(this.AssetType);
      this.addAsset.disable();
      this.assetService
        .getAssetByname(this.AssetType.name, id)
        .subscribe((data: any) => {
          console.log(data);

          this.showAgentAsstData = data.items;
          for (let abc of this.showAgentAsstData) {
            abc.jsonValue = JSON.parse(abc.jsonValue);
          }
          if (this.showAgentAsstData.length == 0) {
            this.showGlobalAsset = false;
          } else {
            this.showGlobalAsset = true;
          }
        });
    });
  }
  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], {
      queryParams: { PageName: 'Asset', id: this.AssetType.id },
    });
  }

  downloadFile(id) {
    let fileName: string;
    this.assetService
      .assetFileExport(id)
      .subscribe((data: HttpResponse<Blob>) => {
        fileName = data.headers
          .get('content-disposition')
          .split(';')[1]
          .split('=')[1]
          .replace(/\"/g, '');
        this.FileSaverService.save(data.body, fileName);
      });
  }
}
