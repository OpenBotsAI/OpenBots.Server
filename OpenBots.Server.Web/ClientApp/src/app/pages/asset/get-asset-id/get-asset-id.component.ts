import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AssetService } from '../asset.service';
import { DatePipe } from '@angular/common';
import { FileSaverService } from 'ngx-filesaver';
import * as moment from 'moment';
import { TimeDatePipe } from '../../../@core/pipe';
import { HttpResponse } from '@angular/common/http';




@Component({
  selector: 'ngx-get-asset-id',
  templateUrl: './get-asset-id.component.html',
  styleUrls: ['./get-asset-id.component.scss'],
})
export class GetAssetIdComponent implements OnInit {
  jsonValue: any = [];
  show_allagents: any = [];
  addagent: FormGroup;
  pipe = new DatePipe('en-US');
  now = Date();
  show_createdon: any = [];

  constructor(
    private acroute: ActivatedRoute,
    protected assetService: AssetService,
    private formBuilder: FormBuilder, private _FileSaverService: FileSaverService, protected router: Router,
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.get_allagent(params.id);
    });
  }

  ngOnInit(): void {
    this.addagent = this.formBuilder.group({
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
    if (this.show_allagents.type == 'Text') {
      let type = 'txt';
      const fileName = `${this.show_allagents.name}.${type}`;
      const fileType = this._FileSaverService.genType(fileName);
      const txtBlob = new Blob([this.addagent.value.textValue], { type: fileType });
      this._FileSaverService.save(txtBlob, fileName);
    }
    else if (this.show_allagents.type == 'Json') {
      let type = 'json';
      const fileName = `${this.show_allagents.name}.${type}`;
      const fileType = this._FileSaverService.genType(fileName);
      const txtBlob = new Blob([this.addagent.value.jsonValue], { type: fileType });
      this._FileSaverService.save(txtBlob, fileName);
    }
    else if (this.show_allagents.type == 'Number') {
      let type = 'txt';
      const fileName = `${this.show_allagents.name}.${type}`;
      const fileType = this._FileSaverService.genType(fileName);
      const txtBlob = new Blob([this.addagent.value.numberValue], { type: fileType });
      this._FileSaverService.save(txtBlob, fileName);
    }
    else if (this.show_allagents.type == 'File') {
      let fileName: string;
      this.assetService.assetFileExport(this.show_allagents.id).subscribe((data: HttpResponse<Blob>) => {
        fileName = data.headers.get('content-disposition').split(';')[1].split('=')[1].replace(/\"/g, '')
        console.log(fileName)
        this._FileSaverService.save(data.body, fileName);
      });
    }

  }



  get_allagent(id) {
    this.assetService.getAssetbyId(id).subscribe((data: HttpResponse<any>) => {
      this.show_allagents = data.body;
      const filterPipe = new TimeDatePipe();
      const fiteredArr = filterPipe.transform(this.show_allagents.createdOn, 'lll');
      this.show_allagents.createdOn = filterPipe.transform(this.show_allagents.createdOn, 'lll');
      if (this.show_allagents.jsonValue) {
        this.jsonValue = this.show_allagents.jsonValue;
        this.jsonValue = JSON.parse(this.jsonValue);
      }
      this.addagent.patchValue(this.show_allagents);
      this.addagent.disable();
    });
  }
  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'Asset', id: this.show_allagents.id } })
  }
}
