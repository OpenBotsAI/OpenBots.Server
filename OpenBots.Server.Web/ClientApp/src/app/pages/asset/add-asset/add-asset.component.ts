import {
  Component,
  EventEmitter,
  OnInit,
  TemplateRef,
  ViewChild,
} from '@angular/core';
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
import { DialogService } from '../../../@core/dialogservices';

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
  showLookUpagent: any = [];
  isDeleted = false;
  fileSize = false;
  etag;
  delId: any = [];
  showAssetbyID: any = [];
  showAgentAsstData: any = [];
  title = 'Add';
  showUpload: boolean = false;
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
  showGlobalAsset: boolean = false;
  @ViewChild(JsonEditorComponent, { static: false })
  editor: JsonEditorComponent;
  viewDialog: any;
  showAgentAssetBtn: boolean = true;
  hideAgentAssetBtn: boolean = false;

  constructor(
    private formBuilder: FormBuilder,
    protected assetService: AssetService,
    protected router: Router,
    private toastrService: NbToastrService,
    private route: ActivatedRoute,
    private dialogService: DialogService
  ) {
    this.editorOptions = new JsonEditorOptions();
    this.editorOptions.modes = ['code', 'text', 'tree', 'view'];
    this.urlId = this.route.snapshot.params['id'];
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
      Name: [''],
      AgentId: [''],
    });
    if (this.urlId) {
      this.getAssetById(this.urlId);
      this.title = 'Update';
      this.addasset.get('type').disable();
    }
  }

  getAssetById(id) {
    this.assetService.getAssetbyId(id).subscribe((data: HttpResponse<any>) => {
      this.showAssetbyID = data.body;
      this.etag = data.headers.get('ETag').replace(/\"/g, '');
      this.addasset.patchValue(this.showAssetbyID);
      this.assetService
        .getAssetByname(this.showAssetbyID.name, this.urlId)
        .subscribe((data: any) => {
          console.log(data);
          this.showAgentAsstData = data.items;
           
        });

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
          this.showUpload = false;
        }
        break;
    }
  }

  get f() {
    return this.addasset.controls;
  }
  onSubmit() {
    if (this.urlId) {
      this.UpdateAsset();
    } else {
      this.addAsset();
    }
  }
  showAssetAgentBox() {
    this.showGlobalAsset = true;
    this.hideAgentAssetBtn = true;
    this.showAgentAssetBtn = false;
    this.assetService.getAgentName().subscribe((data: any) => {
      this.showLookUpagent = data;
    });
  }
  hideAssetAgentBox() {
    this.showGlobalAsset = false;
    this.hideAgentAssetBtn = false;
    this.showAgentAssetBtn = true;
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

      let jsonObjFromData = new FormData();
      jsonObjFromData.append('name', this.addasset.value.name);
      jsonObjFromData.append('type', this.addasset.value.type);
      jsonObjFromData.append('JsonValue', this.addasset.value.JsonValue);

      this.assetService.addAsset(jsonObjFromData).subscribe(
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
      let numberObjFromData = new FormData();
      numberObjFromData.append('name', this.addasset.value.name);
      numberObjFromData.append('type', this.addasset.value.type);
      numberObjFromData.append('NumberValue', this.addasset.value.NumberValue);
      this.assetService.addAsset(numberObjFromData).subscribe(
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
      let textObjFromData = new FormData();
      textObjFromData.append('name', this.addasset.value.name);
      textObjFromData.append('type', this.addasset.value.type);
      textObjFromData.append('TextValue', this.addasset.value.TextValue);

      this.assetService.addAsset(textObjFromData).subscribe(
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
        let fileObjFromData = new FormData();
        fileObjFromData.append('name', this.addasset.value.name);
        fileObjFromData.append('type', this.addasset.value.type);
        fileObjFromData.append('file', this.native_file, this.native_file_name);

        this.assetService.addAsset(fileObjFromData).subscribe(
          (data: any) => {
            this.toastrService.success('Asset added Successfully', 'Success');
            this.router.navigate(['pages/asset/list']);
          },
          () => (this.submitted = false)
        );
      } else {
        this.showUpload = true;
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
        FileUploadformData.append('type', this.addasset.getRawValue().type);
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
              this.submitted = false;
              if (error.error.status === 409) {
                this.toastrService.danger(error.error.serviceErrors, 'error');
                this.getAssetById(this.urlId);
              }
            }
          );
      } else if (this.native_file == undefined) {
        let fileObj = {
          name: this.addasset.value.name,
          type: this.addasset.getRawValue().type,
        };
        this.assetService.editAsset(this.urlId, fileObj, this.etag).subscribe(
          (data: HttpResponse<any>) => {
            this.toastrService.success('Updated successfully', 'Success');
            this.router.navigate(['pages/asset/list']);
            this.native_file = undefined;
            this.native_file_name = undefined;
          },
          (error) => {
            this.submitted = false;
            if (error.error.status === 409) {
              this.toastrService.danger(error.error.serviceErrors, 'error');
              this.getAssetById(this.urlId);
            }
          }
        );
      } else {
        this.showUpload = true;
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
        type: this.addasset.getRawValue().type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        jsonValue: this.addasset.value.JsonValue,
      };
      this.assetService.editAsset(this.urlId, jsondata, this.etag).subscribe(
        () => {
          this.toastrService.success(
            'Asset Details Update Successfully!',
            'Success'
          );
          this.router.navigate(['pages/asset/list']);
        },
        (error) => {
          this.submitted = false;
          if (error.error.error.status === 409) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            this.getAssetById(this.urlId);
          }
        }
      );
    } else if (this.showAssetbyID.type == 'Text') {
      let textdata = {
        name: this.addasset.value.name,
        type: this.addasset.getRawValue().type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        textValue: this.addasset.value.TextValue,
      };
      this.assetService.editAsset(this.urlId, textdata, this.etag).subscribe(
        () => {
          this.toastrService.success(
            'Asset Details Update Successfully!',
            'Success'
          );
          this.router.navigate(['pages/asset/list']);
        },
        (error) => {
          this.submitted = false;
          if (error.error.status === 409) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            this.getAssetById(this.urlId);
          }
        }
      );
    } else if (this.showAssetbyID.type == 'Number') {
      let numberdata = {
        name: this.addasset.value.name,
        type: this.addasset.getRawValue().type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        numberValue: this.addasset.value.NumberValue,
      };
      this.assetService.editAsset(this.urlId, numberdata, this.etag).subscribe(
        () => {
          this.toastrService.success(
            'Asset Details Update Successfully!',
            'Success'
          );
          this.router.navigate(['pages/asset/list']);
        },
        (error) => {
          this.submitted = false;
          if (error.error.status === 409) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            this.getAssetById(this.urlId);
          }
        }
      );
    }

    this.submitted = false;
  }

  SaveAgentAsset() {
    ////// code of add agent asset
    if (this.addasset.value.Name != '' && this.addasset.value.AgentId != '') {
      let agentdata = new FormData();
      agentdata.append('Name', this.addasset.value.name);
      agentdata.append('AgentId', this.addasset.value.AgentId);
      if (this.showAssetbyID.type == 'Text') {
        agentdata.append('TextValue', this.addasset.value.Name);
      } else if (this.showAssetbyID.type == 'Number') {
        agentdata.append('NumberValue', this.addasset.value.Name);
      } else if (this.showAssetbyID.type == 'Json') {
        agentdata.append('JsonValue', this.addasset.value.Name);
      } else if (this.showAssetbyID.type == 'File') {
        //if (this.native_file)
        agentdata.append('File', this.native_file, this.native_file_name);
      }

      this.assetService.addAssetAgent(agentdata).subscribe(() => {
        this.toastrService.success(
          'Asset Agent Value Save Successfully!',
          'Success'
        );
        //// this.router.navigate(['pages/asset/list']);
      });
    }
  }

  open2(dialog: TemplateRef<any>, id: any) {
    this.delId = [];
    this.viewDialog = dialog;
    this.dialogService.openDialog(dialog);
    this.delId = id;
  }

  del_agent(ref) {
    this.isDeleted = true;
       // const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.assetService.delAssetbyID(this.delId).subscribe(
      () => {
        this.isDeleted = false;
        this.toastrService.success('Deleted Successfully');
        ref.close();
        // this.get_allasset(this.page.pageSize, skip);
        //this.pagination(this.page.pageNumber, this.page.pageSize);
      },
      () => (this.isDeleted = false)
    );
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
