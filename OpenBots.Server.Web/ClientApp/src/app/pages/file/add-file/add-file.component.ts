import { Component, EventEmitter, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  UploadOutput,
  UploadInput,
  UploadFile,
  UploaderOptions,
} from 'ngx-uploader';
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
  constructor(
    private formBuilder: FormBuilder,
    protected router: Router,
    private httpService: HttpService,
    private route: ActivatedRoute
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
      //   if (this.urlId) {
      //     this.updateFile(formData);
      //   } else {
      //     this.addFile(formData);
      //   }
      // } else if (this.urlId) {
      //   this.updateFile();
    }
    if (this.urlId) this.updateFile();
    else this.addFile();
    // if (this.confrimUpoad == true) {
    //   let formData = new FormData();
    //   formData.append('file', this.native_file, this.native_file_name);
    //   this.saveForm = formData;
    //   this.httpService.post(
    //       `BinaryObjects/upload?organizationId=${this.orgId}&apiComponent=BinaryObjectAPI&name=${this.addfile.value.name}&folder=${this.addfile.value.folder}`,
    //       formData
    //     )
    //     .subscribe((data: any) => {
    //       this.fileId = data;
    //       this.confrimUpoad = true;
    //       // this.httpService.success('File Upload Successfully')
    //       this.httpService
    //         .post(
    //           `BinaryObjects/save?organizationId=${this.orgId}&binaryObjectId=${this.fileId}&apiComponent=BinaryObjectAPI&name=${this.addfile.value.name}&folder=${this.addfile.value.folder}`,
    //           this.saveForm
    //         )
    //         .subscribe((data: any) => {
    //           this.httpService.success(
    //             'File Save Successfully and File Upload Successfully'
    //           );
    //           this.router.navigate(['pages/file/list']);
    //           this.show_upload = false;
    //         });
    //     });
    // }
    // else if (this.confrimUpoad == false) {
    //   this.httpService.error('Please Upload File');
    //   this.show_upload = true;
    // }
    // this.submitted = false;
  }

  onReset() {
    this.submitted = false;
    this.addfile.reset();
  }

  handleInput(event) {
    if (event.code == 'Slash' || event.code == 'Backslash') {
      this.showKeyError = true;
      this.submitted = true;
    } else {
      this.showKeyError = false;
    }
  }

  getFileDataById(): void {
    this.httpService
      .get(`BinaryObjects/${this.urlId}`)
      .subscribe((response: BinaryFile) => {
        if (response) {
          this.fileByIdData = { ...response };
          this.addfile.patchValue({ ...response });
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
    // if (this.confrimUpoad == true) {
    // let formData = new FormData();
    // formData.append('file', this.native_file, this.native_file_name);
    // this.saveForm = formData;

    // this.httpService
    //   .post(
    //     `BinaryObjects/upload?organizationId=${this.orgId}&apiComponent=BinaryObjectAPI&name=${this.addfile.value.name}&folder=${this.addfile.value.folder}`,
    //     formData
    //   )
    //   .subscribe((data: any) => {
    //     this.fileId = data;
    //     this.confrimUpoad = true;
    //     // this.httpService.success('File Upload Successfully')
    //     this.httpService
    //       .post(
    //         `BinaryObjects/save?organizationId=${this.orgId}&binaryObjectId=${this.fileId}&apiComponent=BinaryObjectAPI&name=${this.addfile.value.name}&folder=${this.addfile.value.folder}`,
    //         this.saveForm
    //       )
    //       .subscribe((data: any) => {
    //         this.httpService.success(
    //           'File Save Successfully and File Upload Successfully'
    //         );
    //         this.router.navigate(['pages/file/list']);
    //         this.show_upload = false;
    //       });
    //   });
    // if (this.confrimUpoad == false) {
    //   this.httpService.error('Please Upload File');
    //   this.show_upload = true;
    // }
    // this.submitted = false;
  }

  updateFile(): void {
    // this.fileByIdData.name = this.addfile.value.name;
    // this.fileByIdData.folder = this.addfile.value.folder;
    if (this.confrimUpoad) {
      this.httpService
        .put(`binaryobjects/${this.urlId}/update`, this.saveForm, {
          observe: 'response',
        })
        .subscribe(
          (response) => {
            if (response && response.status) {
              this.httpService.success('File updated successfully');
              this.router.navigate(['pages/file/list']);
            }
          },
          () => (this.submitted = false)
        );
    } else {
      this.httpService
        .put(`binaryobjects/${this.urlId}`, this.addfile.value, {
          observe: 'response',
        })
        .subscribe(
          (response) => {
            if (response && response.status) {
              this.httpService.success('File updated successfully');
              this.router.navigate(['pages/file/list']);
            }
          },
          () => (this.submitted = false)
        );
    }
    //   if (formData) {
    //     this.httpService
    //       .put(
    //         `BinaryObjects/${this.urlId}/upload?organizationId=${this.orgId}&apiComponent=BinaryObjectAPI&name=${this.addfile.value.name}&folder=${this.addfile.value.folder}`,
    //         formData,
    //         { observe: 'response' }
    //       )
    //       .subscribe(
    //         (response) => {
    //           if (response && response.status == 200) {
    //             this.router.navigate(['pages/file/list']);
    //           }
    //         },
    //         () => (this.submitted = false)
    //       );
    //   } else {
    //     this.fileByIdData.name = this.addfile.value.name;
    //     this.fileByIdData.folder = this.addfile.value.folder;
    //     this.httpService
    //       .put(`BinaryObjects/${this.urlId}`, this.fileByIdData, {
    //         observe: 'response',
    //       })
    //       .subscribe(
    //         (response) => {
    //           if (response && response.status) {
    //             this.router.navigate(['pages/file/list']);
    //           }
    //         },
    //         () => (this.submitted = false)
    //       );
    //   }
  }
}
