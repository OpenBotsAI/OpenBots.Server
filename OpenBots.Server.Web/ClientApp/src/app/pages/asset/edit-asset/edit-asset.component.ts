import { Component, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { AssetService } from '../asset.service';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { UploadOutput, UploadInput, UploadFile, humanizeBytes, UploaderOptions } from 'ngx-uploader';
import { HttpResponse } from '@angular/common/http';

@Component({
  selector: 'ngx-edit-asset',
  templateUrl: './edit-asset.component.html',
  styleUrls: ['./edit-asset.component.scss'],
})
export class EditAssetComponent implements OnInit {
  //// file upload declartion ////
  options: UploaderOptions;
  files: UploadFile[];
  uploadInput: EventEmitter<UploadInput>;
  humanizeBytes: Function;
  dragOver: boolean;
  native_file: any;
  native_file_name: any;
  ///// end declartion////
  etag;
  jsonValue: any = [];
  assetagent: FormGroup;
  submitted = false;
  assetId: any = [];
  show_allagents: any = [];
  show_upload: boolean = false;
  fileSize = false;
  public editorOptions: JsonEditorOptions;
  @ViewChild(JsonEditorComponent, { static: false })
  editor: JsonEditorComponent;
  constructor(
    private acroute: ActivatedRoute,
    private router: Router,
    private formBuilder: FormBuilder,
    protected assetService: AssetService,
    private toastrService: NbToastrService
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.assetId = params.id;
      this.getAssetById(params.id);
    });
    this.editorOptions = new JsonEditorOptions();
    this.editorOptions.modes = ['code', 'text', 'tree', 'view']; 
  }

  ngOnInit(): void {
    this.assetagent = this.formBuilder.group({
      binaryObjectID: [''],
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      id: [''],
      isDeleted: [''],
      jsonValue: [''],
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
        ],
      ],
      numberValue: [''],
      textValue: [''],
      timestamp: [''],
      type: [''],
      updatedBy: [''],
      updatedOn: [''],
    });
  }

  getAssetById(id) {
    this.assetService.getAssetbyId(id).subscribe((data: HttpResponse<any>) => {
      this.show_allagents = data.body;
      this.etag = data.headers.get('ETag').replace(/\"/g, '')
      if (this.show_allagents.jsonValue) {
        this.show_allagents.jsonValue = JSON.parse(this.show_allagents.jsonValue);
      }
      this.assetagent.patchValue(this.show_allagents);
    });
  }

  get f() {
    return this.assetagent.controls;
  }

  onUploadOutput(output: UploadOutput): void {
    switch (output.type) {
      case 'addedToQueue':
        if (typeof output.file !== 'undefined') {
          if (!output.file.size) {
            this.fileSize = true;
            this.submitted = true;
          }
          else {
            this.fileSize = false;
            this.submitted = false;
          }
          this.native_file = output.file.nativeFile
          this.native_file_name = output.file.nativeFile.name
          this.show_upload = false;
        }
        break;

    }
  }

  onSubmit() {
    this.submitted = true;

    if (this.show_allagents.type == 'File') {

      if (this.native_file) {
        let FileUploadformData = new FormData();
        FileUploadformData.append('file', this.native_file, this.native_file_name);
        FileUploadformData.append('name', this.assetagent.value.name);
        FileUploadformData.append('type', this.assetagent.value.type);
        this.assetService
          .editAssetbyUpload(this.assetId, FileUploadformData, this.etag)
          .subscribe((data: HttpResponse<any>) => {
            this.toastrService.success('Asset Details Upate Successfully!', 'Success');
            this.router.navigate(['pages/asset/list']);
            this.native_file = undefined;
            this.native_file_name = undefined;
          },
            (error) => {

              console.log(error, error.status)
              if (error.error.status === 409) {
                this.toastrService.danger(error.error.serviceErrors, 'error')
                this.getAssetById(this.assetId)
              }
            }
        )
      }
      else if (this.native_file == undefined) {
        let fileObj = {
          'name': this.assetagent.value.name,
          'type': this.assetagent.value.type
        }
        this.assetService
          .editAsset(this.assetId, fileObj, this.etag)
          .subscribe((data: HttpResponse<any>) => {
            this.toastrService.success('Updated successfully', 'Success');
            this.router.navigate(['pages/asset/list']);
            this.native_file = undefined;
            this.native_file_name = undefined;
          }, (error) => {

            if (error.error.status === 409) {
              this.toastrService.danger(error.error.serviceErrors, 'error')
              this.getAssetById(this.assetId)
            }
          })
      }
      else {
        this.show_upload = true;
        this.native_file_name = undefined;
        this.native_file = undefined;
      }

    }
    else if (this.show_allagents.type == 'Json') {
      if (!this.editor.isValidJson()) {
        this.toastrService.danger('Provided json is not valid', 'error');
        this.submitted = false;
      }
      this.assetagent.value.jsonValue = JSON.stringify(this.editor.get());
      let jsondata = {
        name: this.assetagent.value.name,
        type: this.assetagent.value.type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        jsonValue: this.assetagent.value.jsonValue,
      };
      this.assetService
        .editAsset(this.assetId, jsondata, this.etag)
        .subscribe(() => {
          this.toastrService.success('Asset Details Upate Successfully!', 'Success');
          this.router.navigate(['pages/asset/list']);
        }, (error) => {

            if (error.error.error.status === 409) {
              this.toastrService.danger(error.error.serviceErrors, 'error')
              this.getAssetById(this.assetId)
          }
        })
    }
    else if (this.show_allagents.type == 'Text') {
      let textdata = {
        name: this.assetagent.value.name,
        type: this.assetagent.value.type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        textValue: this.assetagent.value.textValue,
      };
      this.assetService
        .editAsset(this.assetId, textdata, this.etag)
        .subscribe(() => {
          this.toastrService.success('Asset Details Upate Successfully!', 'Success');
          this.router.navigate(['pages/asset/list']);
        }, (error) => {

            if (error.error.status === 409) {
              this.toastrService.danger(error.error.serviceErrors, 'error')
              this.getAssetById(this.assetId)
          }
        })
    }
    else if (this.show_allagents.type == 'Number') {

      let numberdata = {
        name: this.assetagent.value.name,
        type: this.assetagent.value.type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        numberValue: this.assetagent.value.numberValue,
      };
      this.assetService
        .editAsset(this.assetId, numberdata, this.etag)
        .subscribe(() => {
          this.toastrService.success('Asset Details Upate Successfully!', 'Success');
          this.router.navigate(['pages/asset/list']);
        }, (error) => {

        
            if (error.error.status === 409) {
              this.toastrService.danger(error.error.serviceErrors, 'error')
              this.getAssetById(this.assetId)
            }
        })
    }
    this.submitted = false;
    

  }

  onReset() {
    this.submitted = false;
    this.assetagent.reset();
  }
}
