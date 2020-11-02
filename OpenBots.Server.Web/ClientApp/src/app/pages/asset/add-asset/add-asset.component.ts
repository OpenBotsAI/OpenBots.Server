import { Component, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { Router } from '@angular/router';
import { AssetService } from '../asset.service';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { UploadOutput, UploadInput, UploadFile, humanizeBytes, UploaderOptions } from 'ngx-uploader';

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
  show_upload: boolean = false;
  save_value: any = [];
  addasset: FormGroup;
  submitted = false;
  json: boolean = false;
  file: boolean = false;
  numbervalue: boolean = false;
  textvalue: boolean = false;
  value = ['JSON', 'Number', 'Text', 'File'];
  public editorOptions: JsonEditorOptions;
  public data: any;
  @ViewChild(JsonEditorComponent, { static: false })
  editor: JsonEditorComponent;

  constructor(
    private formBuilder: FormBuilder,
    protected assetService: AssetService,
    protected router: Router,
    private toastrService: NbToastrService) {
    this.editorOptions = new JsonEditorOptions();
    this.editorOptions.modes = ['code', 'text', 'tree', 'view'];

  }

  ngOnInit(): void {
    this.addasset = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100), Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
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






  onUploadOutput(output: UploadOutput): void {
    switch (output.type) {
      case 'addedToQueue':
        if (typeof output.file !== 'undefined') {

          this.native_file = output.file.nativeFile
          this.native_file_name = output.file.nativeFile.name;
          this.show_upload = false;
        }
        break;
    }
  }

  get f() {
    return this.addasset.controls;
  }

  onSubmit() {
    this.submitted = true;

    if (this.json == true && this.numbervalue == false && this.textvalue == false && this.file == false) {
      if (!this.editor.isValidJson()) {
        this.toastrService.danger('Provided json is not valid', 'error');
        this.submitted = false;
      }
      this.addasset.value.JsonValue = JSON.stringify(this.editor.get());
      // let formData = new FormData();
      // formData.append('name', this.addasset.value.name);
      // formData.append('type', this.addasset.value.type);
      // formData.append('JsonValue', this.addasset.value.JsonValue);
      let jsonObj = {
        'name': this.addasset.value.name,
        'type': this.addasset.value.type,
        'JsonValue': this.addasset.value.JsonValue
      }
      // this.save_value = jsonObj;
      this.assetService.addAsset(jsonObj).subscribe(
        (data) => {
          this.toastrService.success('Asset Add  Successfully!', 'Success');
          this.router.navigate(['pages/asset/list']);
        }, () => this.submitted = false
      );
    } else if (this.json == false && this.numbervalue == true && this.textvalue == false && this.file == false) {
      // let formData = new FormData();
      // formData.append('name', this.addasset.value.name);
      // formData.append('type', this.addasset.value.type);
      // formData.append('NumberValue', this.addasset.value.NumberValue);
      let numberObj = {
        'name': this.addasset.value.name,
        'type': this.addasset.value.type,
        'NumberValue': this.addasset.value.NumberValue
      }
      // this.save_value = numberObj;
      this.assetService.addAsset(numberObj).subscribe(
        (data) => {
          this.toastrService.success('Asset Add  Successfully!', 'Success');
          this.router.navigate(['pages/asset/list']);
        }, () => this.submitted = false
      );
    } else if (this.json == false && this.numbervalue == false && this.textvalue == true && this.file == false) {
      // let formData = new FormData();
      // formData.append('name', this.addasset.value.name);
      // formData.append('type', this.addasset.value.type);
      // formData.append('TextValue', this.addasset.value.TextValue);
      let textObj = {
        'name': this.addasset.value.name,
        'type': this.addasset.value.type,
        'TextValue': this.addasset.value.TextValue
      }
      // this.save_value = textObj;
      this.assetService.addAsset(textObj).subscribe(
        (data) => {
          this.toastrService.success('Asset Add  Successfully!', 'Success');
          this.router.navigate(['pages/asset/list']);
        }, () => this.submitted = false
      );
    }
    else if (this.json == false && this.numbervalue == false && this.textvalue == false && this.file == true) {
      if (this.native_file) {

        let formData = new FormData();
        formData.append('file', this.native_file, this.native_file_name);
        // formData.append('name', this.addasset.value.name);
        // formData.append('type', this.addasset.value.type);
        // formData.append('Organizationid', localStorage.getItem('ActiveOrganizationID'));
        let fileObj = {
          'name': this.addasset.value.name,
          'type': this.addasset.value.type
        }
        // this.save_value = formData;
        this.assetService.addAsset(fileObj).subscribe(
          (data: any) => {
            this.assetService.AssetFile(data.id, formData).subscribe((filedata: any) => {
              this.toastrService.success('Asset Add  Successfully!', 'Success');
              this.router.navigate(['pages/asset/list']);
            }
              , () => this.submitted = false
            )

          }, () => this.submitted = false
        );
      }
      else {
        this.show_upload = true;
        this.native_file_name = undefined;
        this.native_file = undefined;
      }

    }


  }

  onReset() {
    this.submitted = false;
    this.addasset.reset();
  }
  get_val(val) {
    if (val == 'JSON') {
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
