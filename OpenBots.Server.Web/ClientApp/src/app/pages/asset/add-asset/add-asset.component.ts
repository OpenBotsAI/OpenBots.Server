import { Component, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { ActivatedRoute, Router } from '@angular/router';
import { AssetService } from '../asset.service';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import {
  UploadOutput,
  UploadInput,
  UploadFile,
  UploaderOptions,
} from 'ngx-uploader';
import { HttpResponse } from '@angular/common/http';

@Component({
  selector: 'ngx-add-asset',
  templateUrl: './add-asset.component.html',
  styleUrls: ['./add-asset.component.scss'],
})
export class AddAssetComponent implements OnInit {
  //// file upload declartion ////
  options: UploaderOptions;
  files: UploadFile[];
  uploadInput: EventEmitter<UploadInput>;
  humanizeBytes: Function;
  dragOver: boolean;
  native_file: any;
  native_file_name: any;
  ///// end declartion////
  fileSize = false;
  etag;
  showAssetbyID: any = [];
  title = 'Add';
  show_upload: boolean = false;
  save_value: any = [];
  addasset: FormGroup;
  submitted = false;
  json: boolean = false;
  file: boolean = false;
  numbervalue: boolean = false;
  textvalue: boolean = false;
  value = ['Json', 'Number', 'Text', 'File'];
  urlId: string;
  public editorOptions: JsonEditorOptions;
  public data: any;
  @ViewChild(JsonEditorComponent, { static: false })
  editor: JsonEditorComponent;

  constructor(
    private formBuilder: FormBuilder,
    protected assetService: AssetService,
    protected router: Router,
    private toastrService: NbToastrService,
    private route: ActivatedRoute
  ) {
    this.editorOptions = new JsonEditorOptions();
    this.editorOptions.modes = ['code', 'text', 'tree', 'view'];
    this.urlId = this.route.snapshot.params['id'];
    if (this.urlId) {
      this.getAssetById(this.urlId);
      this.title = 'Update';
    }
  }

  ngOnInit(): void {
    this.addasset = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
        ],
      ],
      JsonValue: ['', [Validators.minLength(2), Validators.maxLength(100000)]],
      TextValue: ['', [Validators.minLength(2), Validators.maxLength(100000)]],
      NumberValue: [
        '',
        [
          Validators.minLength(1),
          Validators.maxLength(1000),
          Validators.pattern('^[0-9]+(.[0-9]*){0,1}$'),
        ],
      ],
      type: ['', Validators.required],
    });
  }

  getAssetById(id) {
    this.assetService.getAssetbyId(id).subscribe((data: HttpResponse<any>) => {
      this.showAssetbyID = data.body;
      this.etag = data.headers.get('ETag').replace(/\"/g, '');
      this.addasset.patchValue(this.showAssetbyID);
      if (this.showAssetbyID.jsonValue) {
        this.showAssetbyID.jsonValue = JSON.parse(this.showAssetbyID.jsonValue);
      }

      if (this.showAssetbyID.type == 'Text') {
        this.json = false;
        this.numbervalue = false;
        this.textvalue = true;
        this.file = false;

        this.addasset.patchValue({
          TextValue: this.showAssetbyID.textValue,
        });
      } else if (this.showAssetbyID.type == 'Number') {
        this.json = false;
        this.numbervalue = true;
        this.textvalue = false;
        this.file = false;
        this.addasset.patchValue({
          NumberValue: this.showAssetbyID.numberValue,
        });
      } else if (this.showAssetbyID.type == 'File') {
        this.json = false;
        this.numbervalue = false;
        this.textvalue = false;
        this.file = true;
      } else if (this.showAssetbyID.type == 'Json') {
        this.json = true;
        this.numbervalue = false;
        this.textvalue = false;
        this.file = false;
        this.addasset.patchValue({
          JsonValue: this.showAssetbyID.jsonValue,
        });
      }
    });
  }

  onUploadOutput(output: UploadOutput): void {
    switch (output.type) {
      case 'addedToQueue':
        if (typeof output.file !== 'undefined') {
          if (!output.file.size) {
            this.fileSize = true;
            this.submitted = true;
          } else {
            this.fileSize = false;
            this.submitted = false;
          }
          this.native_file = output.file.nativeFile;
          this.native_file_name = output.file.nativeFile.name;
          this.show_upload = false;
        }
        break;
    }
  }

  get f() {
    return this.addasset.controls;
  }
  onSubmit(){
    if(this.urlId){
      this.UpdateAsset();
    }
    else {
      this.addAsset();
    }
  }
  addAsset() {
    this.submitted = true;

    if (
      this.json == true &&
      this.numbervalue == false &&
      this.textvalue == false &&
      this.file == false
    ) {
      if (!this.editor.isValidJson()) {
        this.toastrService.danger('Provided json is not valid', 'error');
        this.submitted = false;
      }
      this.addasset.value.JsonValue = JSON.stringify(this.editor.get());

      let jsonObj = {
        name: this.addasset.value.name,
        type: this.addasset.value.type,
        JsonValue: this.addasset.value.JsonValue,
      };

      this.assetService.addAsset(jsonObj).subscribe(
        (data) => {
          this.toastrService.success('Asset added Successfully', 'Success');
          this.router.navigate(['pages/asset/list']);
        },
        () => (this.submitted = false)
      );
    } else if (
      this.json == false &&
      this.numbervalue == true &&
      this.textvalue == false &&
      this.file == false
    ) {
      let numberObj = {
        name: this.addasset.value.name,
        type: this.addasset.value.type,
        NumberValue: this.addasset.value.NumberValue,
      };

      this.assetService.addAsset(numberObj).subscribe(
        (data) => {
          this.toastrService.success('Asset added Successfully', 'Success');
          this.router.navigate(['pages/asset/list']);
        },
        () => (this.submitted = false)
      );
    } else if (
      this.json == false &&
      this.numbervalue == false &&
      this.textvalue == true &&
      this.file == false
    ) {
      let textObj = {
        name: this.addasset.value.name,
        type: this.addasset.value.type,
        TextValue: this.addasset.value.TextValue,
      };

      this.assetService.addAsset(textObj).subscribe(
        (data) => {
          this.toastrService.success('Asset added Successfully', 'Success');
          this.router.navigate(['pages/asset/list']);
        },
        () => (this.submitted = false)
      );
    } else if (
      this.json == false &&
      this.numbervalue == false &&
      this.textvalue == false &&
      this.file == true
    ) {
      if (this.native_file) {
        let formData = new FormData();
        formData.append('file', this.native_file, this.native_file_name);
        let fileObj = {
          name: this.addasset.value.name,
          type: this.addasset.value.type,
        };
        this.assetService.addAsset(fileObj).subscribe(
          (data: any) => {
            this.assetService.AssetFile(data.id, formData).subscribe(
              (filedata: any) => {
                this.toastrService.success(
                  'Asset added Successfully',
                  'Success'
                );
                this.router.navigate(['pages/asset/list']);
              },
              () => (this.submitted = false)
            );
          },
          () => (this.submitted = false)
        );
      } else {
        this.show_upload = true;
        this.native_file_name = undefined;
        this.native_file = undefined;
      }
    }
  }

  UpdateAsset() {
    this.submitted = true;

    if (this.showAssetbyID.type == 'File') {
      if (this.native_file) {
        let FileUploadformData = new FormData();
        FileUploadformData.append(
          'file',
          this.native_file,
          this.native_file_name
        );
        FileUploadformData.append('name', this.addasset.value.name);
        FileUploadformData.append('type', this.addasset.value.type);
        this.assetService
          .editAssetbyUpload(this.urlId, FileUploadformData, this.etag)
          .subscribe(
            (data: HttpResponse<any>) => {
              this.toastrService.success(
                'Asset Details Upate Successfully!',
                'Success'
              );
              this.router.navigate(['pages/asset/list']);
              this.native_file = undefined;
              this.native_file_name = undefined;
            },
            (error) => {
              console.log(error, error.status);
              if (error.error.status === 409) {
                this.toastrService.danger(error.error.serviceErrors, 'error');
                this.getAssetById(this.urlId);
              }
            }
          );
      } else if (this.native_file == undefined) {
        let fileObj = {
         
          name: this.addasset.value.name,
          type: this.addasset.value.type,
        };
        this.assetService.editAsset(this.urlId, fileObj, this.etag).subscribe(
          (data: HttpResponse<any>) => {
            this.toastrService.success('Updated successfully', 'Success');
            this.router.navigate(['pages/asset/list']);
            this.native_file = undefined;
            this.native_file_name = undefined;
          },
          (error) => {
            if (error.error.status === 409) {
              this.toastrService.danger(error.error.serviceErrors, 'error');
              this.getAssetById(this.urlId);
            }
          }
        );
      } else {
        this.show_upload = true;
        this.native_file_name = undefined;
        this.native_file = undefined;
      }
    } else if (this.showAssetbyID.type == 'Json') {
      if (!this.editor.isValidJson()) {
        this.toastrService.danger('Provided json is not valid', 'error');
        this.submitted = false;
      }
      this.addasset.value.JsonValue = JSON.stringify(this.editor.get());
      let jsondata = {
        name: this.addasset.value.name,
        type: this.addasset.value.type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        jsonValue: this.addasset.value.JsonValue,
      };
      this.assetService.editAsset(this.urlId, jsondata, this.etag).subscribe(
        () => {
          this.toastrService.success(
            'Asset Details Upate Successfully!',
            'Success'
          );
          this.router.navigate(['pages/asset/list']);
        },
        (error) => {
          if (error.error.error.status === 409) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            this.getAssetById(this.urlId);
          }
        }
      );
    } else if (this.showAssetbyID.type == 'Text') {
      let textdata = {
        name: this.addasset.value.name,
        type: this.addasset.value.type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        textValue: this.addasset.value.TextValue,
      };
      this.assetService.editAsset(this.urlId, textdata, this.etag).subscribe(
        () => {
          this.toastrService.success(
            'Asset Details Upate Successfully!',
            'Success'
          );
          this.router.navigate(['pages/asset/list']);
        },
        (error) => {
          if (error.error.status === 409) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            this.getAssetById(this.urlId);
          }
        }
      );
    } else if (this.showAssetbyID.type == 'Number') {
      let numberdata = {
        name: this.addasset.value.name,
        type: this.addasset.value.type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        numberValue: this.addasset.value.NumberValue,
      };
      this.assetService.editAsset(this.urlId, numberdata, this.etag).subscribe(
        () => {
          this.toastrService.success(
            'Asset Details Upate Successfully!',
            'Success'
          );
          this.router.navigate(['pages/asset/list']);
        },
        (error) => {
          if (error.error.status === 409) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            this.getAssetById(this.urlId);
          }
        }
      );
    }
    this.submitted = false;
  }

  onReset() {
    this.submitted = false;
    this.addasset.reset();
  }
  get_val(val) {
    if (val == 'Json') {
      this.json = true;
      this.numbervalue = false;
      this.file = false;
      this.textvalue = false;
    } else if (val == 'Number') {
      this.numbervalue = true;
      this.file = false;
      this.json = false;
      this.textvalue = false;
    } else if (val == 'Text') {
      this.textvalue = true;
      this.json = false;
      this.numbervalue = false;
      this.file = false;
    } else if (val == 'File') {
      this.file = true;
      this.textvalue = false;
      this.json = false;
      this.numbervalue = false;
    }
  }
}
