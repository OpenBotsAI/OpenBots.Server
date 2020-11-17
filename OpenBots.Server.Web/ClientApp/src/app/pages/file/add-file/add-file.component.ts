import { Component, EventEmitter, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  UploadOutput,
  UploadInput,
  UploadFile,
  UploaderOptions,
} from 'ngx-uploader';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { BinaryFile } from '../../../interfaces/file';

@Component({
  selector: 'ngx-add-file',
  templateUrl: './add-file.component.html',
  styleUrls: ['./add-file.component.scss'],
})
export class AddFileComponent implements OnInit {
  //// file upload declartion ////
  options: UploaderOptions;
  files: UploadFile[];
  uploadInput: EventEmitter<UploadInput>;
  humanizeBytes: Function;
  dragOver: boolean;
  native_file: any = [];
  native_file_name: any = [];
  ///// end declartion////
  count = 0;
  showKeyError: boolean = false;
  orgId = localStorage.getItem('ActiveOrganizationID');
  fileId: any = [];
  saveForm: any = [];
  addfile: FormGroup;
  submitted = false;
  cred_value: any = [];
  show_upload: boolean = false;
  confrimUpoad: boolean = false;
  value = ['JSON', 'Number', 'Text'];
  urlId: string;
  fileByIdData: BinaryFile;
  title = 'Add';
  fileSize = false;
  eTag: string;
  constructor(
    private formBuilder: FormBuilder,
    protected router: Router,
    private httpService: HttpService,
    private route: ActivatedRoute,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.urlId = this.route.snapshot.params['id'];
    if (this.urlId) {
      this.title = 'Update';
      this.getFileDataById();
    }
    this.addfile = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          // Validators.pattern('^[0-9a-zA-Z -]+$'),
        ],
      ],
      folder: [''],
    });
  }

  onUploadOutput(output: UploadOutput): void {
    switch (output.type) {
      case 'addedToQueue':
        if (typeof output.file !== 'undefined') {
          if (!output.file.size) this.fileSize = true;
          else this.fileSize = false;
          this.native_file = output.file.nativeFile;
          this.native_file_name = output.file.nativeFile.name;
          this.confrimUpoad = true;
          this.addfile.patchValue({ name: this.native_file_name });
        }
    }
  }

  // convenience getter for easy access to form fields
  get f() {
    return this.addfile.controls;
  }

  onSubmit() {
    this.submitted = true;
    if (this.addfile.invalid) {
      return;
    }

    if (this.confrimUpoad == true) {
      let formData = new FormData();
      formData.append('file', this.native_file, this.native_file_name);
      formData.append('name', this.addfile.value.name);
      this.saveForm = formData;
    }
    if (this.urlId) this.updateFile();
    else this.addFile();
  }

  onReset(): void {
    this.submitted = false;
    this.addfile.reset();
  }

  handleInput(event): void {
    if (event.code == 'Slash' || event.code == 'Backslash') {
      this.showKeyError = true;
      this.submitted = true;
    } else {
      this.showKeyError = false;
    }
  }

  getFileDataById(): void {
    this.httpService
      .get(`BinaryObjects/${this.urlId}`, { observe: 'response' })
      .subscribe((response) => {
        if (response && response.body) {
          this.eTag = response.headers.get('etag');
          this.fileByIdData = { ...response.body };
          this.addfile.patchValue({ ...response.body });
        }
      });
  }

  addFile(): void {
    this.httpService.post('binaryobjects', this.addfile.value).subscribe(
      (response) => {
        if (response) {
          if (this.confrimUpoad && response.id) {
            this.httpService
              .post(`binaryobjects/${response.id}/upload`, this.saveForm)
              .subscribe(
                () => {
                  this.httpService.success(
                    'File uploaded and created successfully'
                  );
                  this.router.navigate(['pages/file/list']);
                },
                () => (this.submitted = false)
              );
          } else {
            this.httpService.success('File created successfully');
            this.router.navigate(['pages/file/list']);
          }
        }
      },
      () => (this.submitted = false)
    );
  }

  updateFile(): void {
    const headers = this.helperService.getETagHeaders(this.eTag);
    if (this.confrimUpoad) {
      this.saveForm.append('folder', this.addfile.value.folder);
      this.httpService
        .put(`binaryobjects/${this.urlId}/update`, this.saveForm, {
          observe: 'response',
          headers,
        })
        .subscribe(
          (response) => {
            if (response && response.status) {
              this.httpService.success('File updated successfully');
              this.router.navigate(['pages/file/list']);
            }
          },
          (error) => {
            if (error && error.error && error.error.status === 409) {
              this.submitted = false;
              this.httpService.error(error.error.serviceErrors);
              this.getFileDataById();
            }
          }
        );
    } else {
      this.httpService
        .put(`binaryobjects/${this.urlId}`, this.addfile.value, {
          observe: 'response',
          headers,
        })
        .subscribe(
          (response) => {
            if (response && response.status) {
              this.httpService.success('File updated successfully');
              this.router.navigate(['pages/file/list']);
            }
          },
          (error) => {
            if (error && error.error && error.error.status === 409) {
              this.submitted = false;
              this.httpService.error(error.error.serviceErrors);
              this.getFileDataById();
            }
          }
        );
    }
  }
}
