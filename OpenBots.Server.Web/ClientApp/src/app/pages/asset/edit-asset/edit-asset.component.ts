import { Component, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { AssetService } from '../asset.service';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { UploadOutput, UploadInput, UploadFile, humanizeBytes, UploaderOptions } from 'ngx-uploader';

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
  jsonValue: any = [];
  assetagent: FormGroup;
  submitted = false;
  agent_id: any = [];
  show_allagents: any = [];
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
      this.agent_id = params.id;
      this.get_allagent(params.id);
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

  get_allagent(id) {
    this.assetService.getAssetbyId(id).subscribe((data: any) => {
      this.show_allagents = data;
      if (data.jsonValue) {
        data.jsonValue = JSON.parse(data.jsonValue);
      }
      this.assetagent.patchValue(data);
    });
  }

  get f() {
    return this.assetagent.controls;
  }

  onUploadOutput(output: UploadOutput): void {
    switch (output.type) {
      case 'addedToQueue':
        if (typeof output.file !== 'undefined') {
          this.native_file = output.file.nativeFile
          this.native_file_name = output.file.nativeFile.name
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
        // FileUploadformData.append('Organizationid', localStorage.getItem('ActiveOrganizationID'));


        this.assetService
          .editAssetbyUpload(this.agent_id, FileUploadformData)
          .subscribe(() => {
            this.toastrService.success('Asset Details Upate Successfully!', 'Success');
            this.router.navigate(['pages/asset/list']);
            this.native_file = undefined;
            this.native_file_name = undefined;
          });
      }
      else if (this.native_file == undefined) {
        let fileObj = {
          'name': this.assetagent.value.name,
          'type': this.assetagent.value.type
        }
        // let formData = new FormData();
        // formData.append('name', this.assetagent.value.name);
        // formData.append('type', this.assetagent.value.type);
        // formData.append('Organizationid', localStorage.getItem('ActiveOrganizationID'));

        this.assetService
          .editAsset(this.agent_id, fileObj)
          .subscribe(() => {
            this.toastrService.success('Updated successfully', 'Success');
            this.router.navigate(['pages/asset/list']);
            this.native_file = undefined;
            this.native_file_name = undefined;
          });
      }

    }
    else if (this.show_allagents.type == 'JSON') {
      if (!this.editor.isValidJson()) {
        this.toastrService.danger('Provided json is not valid', 'error');
        this.submitted = false;
      }
      this.assetagent.value.jsonValue = JSON.stringify(this.editor.get());
      let jsondata = {
        name: this.assetagent.value.name,
        type: this.assetagent.value.type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        jsonValue: this.assetagent.value.numberValue,
      };
      this.assetService
        .editAsset(this.agent_id, jsondata)
        .subscribe(() => {
          this.toastrService.success('Asset Details Upate Successfully!', 'Success');
          this.router.navigate(['pages/asset/list']);
        });
    }
    else if (this.show_allagents.type == 'Text') {
      let textdata = {
        name: this.assetagent.value.name,
        type: this.assetagent.value.type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        textValue: this.assetagent.value.textValue,
      };
      this.assetService
        .editAsset(this.agent_id, textdata)
        .subscribe(() => {
          this.toastrService.success('Asset Details Upate Successfully!', 'Success');
          this.router.navigate(['pages/asset/list']);
        });
    }
    else if (this.show_allagents.type == 'Number') {

      let numberdata = {
        name: this.assetagent.value.name,
        type: this.assetagent.value.type,
        Organizationid: localStorage.getItem('ActiveOrganizationID'),
        numberValue: this.assetagent.value.numberValue,
      };
      this.assetService
        .editAsset(this.agent_id, numberdata)
        .subscribe(() => {
          this.toastrService.success('Asset Details Upate Successfully!', 'Success');
          this.router.navigate(['pages/asset/list']);
        });
    }
    this.submitted = false;

  }

  onReset() {
    this.submitted = false;
    this.assetagent.reset();
  }
}
